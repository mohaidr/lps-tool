using LPS.Domain;
using LPS.Infrastructure.LPSClients.HeaderServices;
using LPS.Infrastructure.Caching;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.LPSClients.PlaceHolderService;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using LPS.Domain.LPSSession;
using System.Net.Http.Headers;
using LPS.Infrastructure.LPSClients.CachService;

namespace LPS.Infrastructure.LPSClients.MessageServices
{
    public class MessageService(IHttpHeadersService headersService,
                                ILogger logger,
                                IRuntimeOperationIdProvider runtimeOperationIdProvider,
                                ICacheService<long> memoryCacheService,
                                ICacheService<object> dynamicTypeMemoryCacheService,
                                IPlaceholderResolverService placeHolderResolver) : IMessageService
    {
        readonly ILogger _logger = logger;
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider;
        readonly IHttpHeadersService _headersService = headersService;
        readonly ICacheService<long> _memoryCacheService = memoryCacheService;
        readonly ICacheService<object> _dynamicTypeCacheService = dynamicTypeMemoryCacheService;
        readonly IPlaceholderResolverService _placeHolderResolver = placeHolderResolver;

        public async Task<(HttpRequestMessage HttpRequestMessage, long MessageSize)> BuildAsync(HttpRequest httpRequest, string sessionId, CancellationToken token = default)
        {
            // Resolve placeholders for HttpVersion, HttpMethod, URL
            var resolvedHttpVersion = await _placeHolderResolver.ResolvePlaceholdersAsync<string>(httpRequest.HttpVersion, sessionId, token);
            var resolvedHttpMethod = await _placeHolderResolver.ResolvePlaceholdersAsync<string>(httpRequest.HttpMethod, sessionId, token);
            var resolvedUrl = await _placeHolderResolver.ResolvePlaceholdersAsync<string>(httpRequest.Url.Url, sessionId, token);

            // Create the HttpRequestMessage with resolved values
            var httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(resolvedUrl),
                Method = new HttpMethod(resolvedHttpMethod),
                Version = GetHttpVersion(resolvedHttpVersion)
            };

            // Determine if the request supports content
            bool supportsContent = resolvedHttpMethod.Equals("post", StringComparison.CurrentCultureIgnoreCase)
                                   || resolvedHttpMethod.Equals("put", StringComparison.CurrentCultureIgnoreCase)
                                   || resolvedHttpMethod.Equals("patch", StringComparison.CurrentCultureIgnoreCase);

            if (supportsContent && httpRequest.Payload != null)
            {
                switch (httpRequest.Payload.Type)
                {
                    case Payload.PayloadType.Raw:
                        var resolvedRawValue = await _placeHolderResolver.ResolvePlaceholdersAsync<string>(httpRequest.Payload.RawValue, sessionId, token);
                        httpRequestMessage.Content = new StringContent(resolvedRawValue ?? string.Empty, Encoding.UTF8);
                        break;
                    case Payload.PayloadType.Multipart:
                        var multipartContent = new MultipartFormDataContent();
                        // Add fields
                        foreach (var field in httpRequest.Payload.Multipart.Fields)
                        {
                            var content = new StringContent(field.Value ?? string.Empty);
                            var contentType = string.IsNullOrEmpty(field.ContentType) ? "text/plain" : field.ContentType;
                            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                            multipartContent.Add(content, field.Name);
                        }
                        // Add files
                        foreach (var file in httpRequest.Payload.Multipart.Files)
                        {
                            HttpContent fileContent;

                            if (file.Content is byte[] binaryContent)
                            {
                                // Use ByteArrayContent without copying the array
                                fileContent = new ByteArrayContent(binaryContent);
                            }
                            else if (file.Content is string textContent)
                            {
                                string multipartFileCacheKey = $"{CachePrefixes.Multipartfile}{file.Name}";
                                // Use StringContent for text-based content
                                fileContent = (await _dynamicTypeCacheService.GetItemAsync(multipartFileCacheKey)) as HttpContent ?? new StringContent(textContent);
                                await _dynamicTypeCacheService.SetItemAsync(multipartFileCacheKey, fileContent, TimeSpan.FromMinutes(5));
                            }
                            else
                            {
                                throw new InvalidOperationException($"Unsupported file content type for file: {file.Name}");
                            }

                            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                            multipartContent.Add(fileContent, file.Name, file.Name);
                        }

                        httpRequestMessage.Content = multipartContent;
                        break;

                    case Payload.PayloadType.Binary:
                        var binaryData = httpRequest.Payload.BinaryValue;
                        httpRequestMessage.Content = new ByteArrayContent(binaryData)
                        {
                            Headers = { ContentType = new MediaTypeHeaderValue("application/octet-stream") }
                        };
                        break;

                    default:
                        throw new NotSupportedException($"Unsupported payload type: {httpRequest.Payload.Type}");
                }
            }

            if (httpRequest.SupportH2C.HasValue && httpRequest.SupportH2C.Value)
            {
                if (httpRequestMessage.Version != HttpVersion.Version20)
                {
                    await _logger.LogAsync(_runtimeOperationIdProvider.OperationId,
                        $"SupportH2C was enabled on a non-HTTP/2 protocol, so the version is being overridden from {httpRequestMessage.Version} to {HttpVersion.Version20}.",
                        LPSLoggingLevel.Warning, token);
                    httpRequestMessage.Version = HttpVersion.Version20;
                }
                httpRequestMessage.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
            }

            // Apply headers to the request
            await _headersService.ApplyHeadersAsync(httpRequestMessage, sessionId, httpRequest.HttpHeaders, token);

            // Cache key to identify the request profile
            string cacheKey = $"{CachePrefixes.RequestSize}{httpRequest.Id}";

            // Check if the message size is cached
            if (!_memoryCacheService.TryGetItem(cacheKey, out long messageSize))
            {
                // If not cached, calculate the message size based on the profile
                messageSize = await CalculateRequestSizeAsync(httpRequestMessage);

                // Cache the calculated size
                await _memoryCacheService.SetItemAsync(cacheKey, messageSize);
            }

            // Update the DataSent metric using MetricsService

            return (httpRequestMessage, messageSize);
        }

        private static async Task<long> CalculateRequestSizeAsync(HttpRequestMessage httpRequestMessage)
        {
            long size = 0;

            // Start-Line Size
            size += Encoding.UTF8.GetByteCount(
                $"{httpRequestMessage.Method.Method} {httpRequestMessage.RequestUri?.ToString() ?? string.Empty} HTTP/{httpRequestMessage.Version}\r\n"
            );

            // Headers Size
            var headers = httpRequestMessage.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

            // Content headers (if present)
            if (httpRequestMessage.Content?.Headers != null)
            {
                foreach (var header in httpRequestMessage.Content.Headers)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            // Add all headers
            foreach (var header in headers)
            {
                size += Encoding.UTF8.GetByteCount($"{header.Key}: {header.Value}\r\n");
            }

            // Add Host header if missing
            if (!headers.Any(header => header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)))
            {
                size += Encoding.UTF8.GetByteCount("Host: ") + Encoding.UTF8.GetByteCount(new Uri(httpRequestMessage.RequestUri?.ToString() ?? string.Empty).Host) + 2;
            }

            // Final \r\n after headers
            size += 2;

            // Content Size (if present)
            if (httpRequestMessage.Content != null)
            {
                var contentBytes = await httpRequestMessage.Content.ReadAsByteArrayAsync();
                size += contentBytes.Length;
            }

            return size;
        }

        private static Version GetHttpVersion(string version)
        {
            return version switch
            {
                "1.0" => HttpVersion.Version10,
                "1.1" => HttpVersion.Version11,
                "2.0" => HttpVersion.Version20,
                _ => HttpVersion.Version20,
            };
        }
    }
}
