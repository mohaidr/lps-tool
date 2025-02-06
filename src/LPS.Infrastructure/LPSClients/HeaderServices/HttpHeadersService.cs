using LPS.Infrastructure.LPSClients.HeaderServices;
using LPS.Infrastructure.LPSClients.PlaceHolderService;
using LPS.Infrastructure.LPSClients.SessionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;
namespace LPS.Infrastructure.LPSClients.HeaderServices
{
    public class HttpHeadersService : IHttpHeadersService
    {
        IPlaceholderResolverService _placeHolderResolver;
        public HttpHeadersService(IPlaceholderResolverService placeHolderResolver)
        {
            _placeHolderResolver = placeHolderResolver;
        }
        public async Task ApplyHeadersAsync(HttpRequestMessage httpRequestMessage, string sessionId, Dictionary<string, string> HttpHeaders, CancellationToken token)
        {
            bool supportContentHeaders = httpRequestMessage?.Method != null &&
                                         (string.Equals(httpRequestMessage.Method.Method, "POST", StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(httpRequestMessage.Method.Method, "PUT", StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(httpRequestMessage.Method.Method, "PATCH", StringComparison.OrdinalIgnoreCase));
            foreach (var header in HttpHeaders)
            {
                var resolvedValue = await _placeHolderResolver.ResolvePlaceholdersAsync<string>(header.Value, sessionId, token);
                if (supportContentHeaders)
                {
                    var contentHeaders = httpRequestMessage.Content.Headers;

                    if (contentHeaders.GetType().GetProperties().Any(property => property.Name.Equals(header.Key.Replace("-", ""), StringComparison.CurrentCultureIgnoreCase)))
                    {
                        SetContentHeader(httpRequestMessage, header.Key, resolvedValue);
                        continue;
                    }
                }

                if (!new StringContent("").Headers.GetType().GetProperties().Any(property => property.Name.Equals(header.Key.Replace("-", ""), StringComparison.CurrentCultureIgnoreCase)))
                {
                    var requestHeader = httpRequestMessage.Headers;
                    if (requestHeader.GetType().GetProperties().Any(property => property.Name.Equals(header.Key.Replace("-", ""), StringComparison.CurrentCultureIgnoreCase)))
                    {
                        SetRequestHeader(httpRequestMessage, header.Key.Trim(), resolvedValue.Trim());
                    }
                    else
                    {
                        SetUserHeader(httpRequestMessage, header.Key.Trim(), resolvedValue.Trim());
                    }
                }
            }
        }
        private static void SetContentHeader(HttpRequestMessage message, string name, string value)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return;

            switch (name.ToLower())
            {
                case "content-type":
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue(value);
                    break;
                case "content-encoding":
                    var contentEncoding = value.Trim().Split(',');
                    foreach (var encoding in contentEncoding)
                    {
                        message.Content.Headers.ContentEncoding.Add(encoding);
                    }
                    break;
                case "content-language":
                    var languages = value.Trim().Split(',');
                    foreach (var language in languages)
                    {
                        message.Content.Headers.ContentLanguage.Add(language);
                    }
                    break;
                case "content-length":
                    message.Content.Headers.ContentLength = long.Parse(value);
                    break;
                case "content-disposition":
                    message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(value);
                    break;
                case "content-md5":
                    message.Content.Headers.ContentMD5 = Convert.FromBase64String(value);
                    break;
                default:
                    throw new NotSupportedException("Unsupported Content Header, the currently supported headers are (content-type, content-encoding, content-length, content-language, content-disposition, content-location, content-md5, content-range, expires, last-modified)");
            }
        }
        private static void SetRequestHeader(HttpRequestMessage message, string name, string value)
        {

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return;

            string[] encodings;
            switch (name.Trim().ToLower())
            {
                case "authorization":
                    AuthenticationHeaderValue authValue;
                    if (AuthenticationHeaderValue.TryParse(value, out authValue))
                    {
                        message.Headers.Authorization = authValue;
                    }
                    break;
                case "accept":
                    var types = value.Trim().Split(',');

                    foreach (var type in types)
                    {
                        MediaTypeWithQualityHeaderValue typeValue;

                        if (MediaTypeWithQualityHeaderValue.TryParse(type, out typeValue))
                        {
                            message.Headers.Accept.Add(typeValue);
                        }
                    }
                    break;
                case "accept-charset":
                    var charsets = value.Trim().Split(',');

                    foreach (var charset in charsets)
                    {
                        StringWithQualityHeaderValue charsetValue;
                        if (StringWithQualityHeaderValue.TryParse(charset, out charsetValue))
                        {
                            message.Headers.AcceptCharset.Add(charsetValue);
                        }
                    }
                    break;
                case "accept-encoding":
                    encodings = value.Trim().Split(',');
                    foreach (var encoding in encodings)
                    {
                        StringWithQualityHeaderValue encodingValue;
                        if (StringWithQualityHeaderValue.TryParse(encoding, out encodingValue))
                        {
                            message.Headers.AcceptEncoding.Add(encodingValue);
                        }

                    }
                    break;
                case "accept-language":
                    var languages = value.Trim().Split(',');

                    foreach (var language in languages)
                    {
                        StringWithQualityHeaderValue languageValue;
                        if (StringWithQualityHeaderValue.TryParse(language, out languageValue))
                        {
                            message.Headers.AcceptLanguage.Add(languageValue);
                        }
                    }
                    break;
                case "connection":
                    var connectionValues = value.Trim().Split(',');

                    foreach (var connectionValue in connectionValues)
                    {
                        message.Headers.Connection.Add(connectionValue);
                        if (connectionValue.ToLower() == "close")
                        {
                            message.Headers.ConnectionClose = true;
                        }
                    }
                    break;
                case "host":
                    message.Headers.Host = value;
                    break;
                case "transfer-encoding":
                    encodings = value.Trim().Split(',');
                    foreach (var encoding in encodings)
                    {
                        TransferCodingHeaderValue encodingValue;
                        if (TransferCodingHeaderValue.TryParse(encoding, out encodingValue))
                        {
                            message.Headers.TransferEncoding.Add(encodingValue);
                            if (encoding.ToLower() == "chuncked")
                            {
                                message.Headers.TransferEncodingChunked = true;
                            }
                        }
                    }
                    break;
                case "user-agent":
                    var agents = value.Trim().Split(',');
                    foreach (var agent in agents)
                    {
                        ProductInfoHeaderValue agentValue;
                        if (ProductInfoHeaderValue.TryParse(agent, out agentValue))
                        {
                            message.Headers.UserAgent.Add(agentValue);
                        }
                    }
                    break;
                case "upgrade":
                    ProductHeaderValue upgradeValue;
                    if (ProductHeaderValue.TryParse(value, out upgradeValue))
                    {
                        message.Headers.Upgrade.Add(upgradeValue);
                    }
                    break;
                case "pragma":
                    message.Headers.Pragma.Add(new NameValueHeaderValue(value));
                    break;
                case "cache-control":
                    CacheControlHeaderValue cacheControlValue;
                    if (CacheControlHeaderValue.TryParse(value, out cacheControlValue))
                    {
                        message.Headers.CacheControl = cacheControlValue;
                    }
                    break;
                // Additional headers to apply
                case "expect":
                    message.Headers.ExpectContinue = value.Trim() == "100-continue";
                    break;
                case "date":
                    DateTimeOffset date;
                    if (DateTimeOffset.TryParse(value, out date))
                    {
                        message.Headers.Date = date;
                    }
                    break;
                case "from":
                    message.Headers.From = value;
                    break;
                case "if-match":
                    var matches = value.Trim().Split(',');
                    foreach (var match in matches)
                    {
                        EntityTagHeaderValue matchValue;
                        if (EntityTagHeaderValue.TryParse(match, out matchValue))
                        {
                            message.Headers.IfMatch.Add(matchValue);
                        }
                    }
                    break;
                case "if-none-match":
                    var noneMatches = value.Trim().Split(',');
                    foreach (var noneMatch in noneMatches)
                    {
                        EntityTagHeaderValue noneMatchValue;
                        if (EntityTagHeaderValue.TryParse(noneMatch, out noneMatchValue))
                        {
                            message.Headers.IfNoneMatch.Add(noneMatchValue);
                        }
                    }
                    break;
                case "if-unmodified-since":
                    DateTimeOffset ifUnmodifiedSince;
                    if (DateTimeOffset.TryParse(value, out ifUnmodifiedSince))
                    {
                        message.Headers.IfUnmodifiedSince = ifUnmodifiedSince;
                    }
                    break;
                case "if-modified-since":
                    DateTimeOffset ifModifiedSince;
                    if (DateTimeOffset.TryParse(value, out ifModifiedSince))
                    {
                        message.Headers.IfModifiedSince = ifModifiedSince;
                    }
                    break;
                case "max-forwards":
                    int maxForwards;
                    if (int.TryParse(value, out maxForwards))
                    {
                        message.Headers.MaxForwards = maxForwards;
                    }
                    break;
                case "proxy-authorization":
                    AuthenticationHeaderValue authHeaderValue;
                    if (AuthenticationHeaderValue.TryParse(value, out authHeaderValue))
                    {
                        message.Headers.ProxyAuthorization = authHeaderValue;
                    }
                    break;
                case "range":
                    RangeHeaderValue rangeValue;
                    if (RangeHeaderValue.TryParse(value, out rangeValue))
                    {
                        message.Headers.Range = rangeValue;
                    }
                    break;
                case "if-range":
                    RangeConditionHeaderValue ifRangeValue;
                    if (RangeConditionHeaderValue.TryParse(value, out ifRangeValue))
                    {
                        message.Headers.IfRange = ifRangeValue;
                    }

                    break;
                case "referrer":
                    Uri referrerValue;
                    if (Uri.TryCreate(value, UriKind.Absolute, out referrerValue))
                    {
                        message.Headers.Referrer = referrerValue;
                    }
                    break;
                case "te":
                    var tes = value.Trim().Split(',');
                    foreach (var te in tes)
                    {
                        TransferCodingWithQualityHeaderValue teValue;
                        if (TransferCodingWithQualityHeaderValue.TryParse(te, out teValue))
                        {
                            message.Headers.TE.Add(teValue);
                        }
                    }
                    break;
                case "trailer":
                    var trailers = value.Trim().Split(',');
                    foreach (var trailer in trailers)
                    {
                        message.Headers.Trailer.Add(trailer);
                    }
                    break;
                case "via":
                    var vias = value.Trim().Split(',');
                    foreach (var via in vias)
                    {
                        ViaHeaderValue viaValue;
                        if (ViaHeaderValue.TryParse(via, out viaValue))
                        {
                            message.Headers.Via.Add(viaValue);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException($"header {name} is an unsupported request header.");
            }
        }
        private static void SetUserHeader(HttpRequestMessage message, string name, string value)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return;

            message.Headers.Add(name, value);
        }

    }
}