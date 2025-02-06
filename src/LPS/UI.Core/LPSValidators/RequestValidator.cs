using LPS.Domain;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using FluentValidation;
using FluentValidation.Results;
using LPS.UI.Common;
using System.Text;
using Microsoft.AspNetCore.Http;
using LPS.DTOs;
using System.CommandLine;
using System.Linq.Expressions;
using LPS.Infrastructure.Common;
using LPS.Domain.LPSSession;
using System.Data;

namespace LPS.UI.Core.LPSValidators
{
    internal class RequestValidator : CommandBaseValidator<HttpRequestDto>
    {

        readonly HttpRequestDto _requestDto;
        readonly string[] _httpMethods = { "GET", "HEAD", "POST", "PUT", "PATCH", "DELETE", "CONNECT", "OPTIONS", "TRACE" };
        public RequestValidator(HttpRequestDto requestDto)
        {
            ArgumentNullException.ThrowIfNull(requestDto);
            _requestDto = requestDto;

            RuleFor(dto => dto.HttpVersion)
                .NotEmpty()
                .WithMessage("The 'HttpVersion' cannot be null or empty.")
                .Must(version =>
                {
                    // Allow valid HTTP versions or placeholders
                    return string.IsNullOrEmpty(version)
                        || version.StartsWith("$")
                        || version == "1.0"
                        || version == "1.1"
                        || version == "2.0";
                })
                .WithMessage("The accepted 'Http Versions' are (\"1.0\", \"1.1\", \"2.0\") or placeholders starting with '$'")
                .Must((dto, version) =>
                {
                    // Parse SupportH2C as bool and validate compatibility
                    if (string.IsNullOrWhiteSpace(dto.SupportH2C) || dto.SupportH2C.StartsWith("$"))
                        return true;

                    if (bool.TryParse(dto.SupportH2C, out bool supportH2C) && supportH2C)
                        return version == "2.0";

                    return true; // Validation passes if SupportH2C is false or invalid
                })
                .WithMessage("H2C only works with HTTP/2");

            RuleFor(dto => dto.HttpMethod)
                .NotEmpty()
                .WithMessage("The 'HttpMethod' cannot be null or empty.")
                .Must(httpMethod => string.IsNullOrEmpty(httpMethod)
                    || httpMethod.StartsWith("$")
                    || _httpMethods.Any(method => method.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("The supported 'Http Methods' are (\"GET\", \"HEAD\", \"POST\", \"PUT\", \"PATCH\", \"DELETE\", \"CONNECT\", \"OPTIONS\", \"TRACE\") or placeholders starting with '$'");

            RuleFor(dto => dto.URL)
                .NotEmpty()
                .WithMessage("The 'URL' cannot be null or empty.")
                .Must(url =>
                {
                    // Allow valid URLs or placeholders
                    return string.IsNullOrEmpty(url)
                        || url.StartsWith("$")
                        || url.StartsWith("/")
                        || (Uri.TryCreate(url, UriKind.Absolute, out Uri result)
                            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps));
                })
                .WithMessage("The 'URL' must be a valid URL according to RFC 3986, a URI path starting with '/' or a placeholder starting with '$'")
                .Must((dto, url) =>
                {
                    // Parse SupportH2C as bool and validate compatibility with URL schema
                    if (string.IsNullOrWhiteSpace(dto.SupportH2C) || dto.SupportH2C.StartsWith("$"))
                        return true;

                    if (bool.TryParse(dto.SupportH2C, out bool supportH2C) && supportH2C)
                    {
                        return string.IsNullOrEmpty(url)
                            || url.StartsWith("$")
                            || url.StartsWith("/")
                            || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
                    }

                    return true; // Validation passes if SupportH2C is false or invalid
                })
                .WithMessage("H2C only works with the HTTP schema or placeholders starting with '$'");

            RuleFor(dto => dto.SupportH2C)
                .NotEmpty()
                .When(dto =>
                    !string.IsNullOrEmpty(dto.URL)
                    && dto.URL.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrEmpty(dto.HttpVersion)
                    && dto.HttpVersion.Equals("2.0", StringComparison.OrdinalIgnoreCase))
                .WithMessage("'SupportH2C' must be a valid boolean ('true' or 'false') or a placeholder starting with '$'")
                .Must(supportH2C =>
                {
                    // Allow valid boolean values or placeholders
                    return string.IsNullOrEmpty(supportH2C)
                        || supportH2C.StartsWith("$")
                        || bool.TryParse(supportH2C, out _);
                })
                .WithMessage("'SupportH2C' must be 'true', 'false', or a placeholder starting with '$'");

            RuleFor(dto => dto.SaveResponse)
                .Must(saveResponse =>
                {
                    // Allow valid boolean values or placeholders
                    return string.IsNullOrEmpty(saveResponse)
                        || saveResponse.StartsWith("$")
                        || bool.TryParse(saveResponse, out _);
                })
                .WithMessage("'Save Response' must be 'true', 'false', or a placeholder starting with '$'");

            RuleFor(dto => dto.DownloadHtmlEmbeddedResources)
                .Must(downloadHtmlEmbeddedResources =>
                {
                    // Allow valid boolean values or placeholders
                    return string.IsNullOrEmpty(downloadHtmlEmbeddedResources)
                        || downloadHtmlEmbeddedResources.StartsWith("$")
                        || bool.TryParse(downloadHtmlEmbeddedResources, out _);
                })
                .WithMessage("'Download Html Embedded Resources' must be 'true', 'false', or a placeholder starting with '$'");
           
            When(dto => dto.Payload != null, () =>
            {

                RuleFor(dto => dto.Payload.Multipart)
                .NotNull()
                .When(dto => dto.Payload.Type == Payload.PayloadType.Multipart)
                .WithMessage("Multipart content is invalid");

                When(dto => dto.Payload.Multipart != null, () =>
                {
                    // Ensure either Files or Fields have at least one item
                    RuleFor(dto => dto.Payload.Multipart)
                        .Must(multipart =>
                            (multipart.Files != null && multipart.Files.Count > 0) ||
                            (multipart.Fields != null && multipart.Fields.Count > 0))
                        .WithMessage("The 'Multipart' object must contain at least one file or one field.");
                   
                    RuleFor(dto => dto.Payload.Multipart)
                        .Must((dto, multipart) => multipart.Files == null || multipart.Files.All(file => IsValidFilePath(file.Path)))
                        .WithMessage("All files in the Multipart must have valid file paths.");

                    RuleFor(dto => dto.Payload.Multipart)
                        .Must(multipart => multipart?.Fields == null || multipart.Fields.All(field => !string.IsNullOrWhiteSpace(field.Name) && !string.IsNullOrWhiteSpace(field.Value)))
                        .WithMessage("All fields in the Multipart must have both field names and values.");
                });

                RuleFor(dto => dto.Payload.File)
                .NotNull()
                .NotEmpty()
                .When(dto => dto.Payload.Type == Payload.PayloadType.Binary)
                .WithMessage("Invalid File");


                RuleFor(dto => dto.Payload.File)
                    .Must(file => string.IsNullOrWhiteSpace(file) || IsValidFilePath(file))
                    .WithMessage("The File property must hold a valid file path when it is not null or empty.");
            });
            // Enforce HTTP when SupportH2C is true
            When(dto =>
            {
                // Parse SupportH2C as a boolean
                return !string.IsNullOrWhiteSpace(dto.SupportH2C)
                       && !dto.SupportH2C.StartsWith("$")
                       && bool.TryParse(dto.SupportH2C, out bool supportH2C)
                       && supportH2C;
            }, () =>
            {
                RuleFor(dto => dto.URL)
                    .Must(url =>
                    {
                        // Ensure URL uses the HTTP scheme when SupportH2C is enabled
                        return string.IsNullOrEmpty(url)
                        || url.StartsWith("$")
                        || url.StartsWith("/")
                        || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
                    })
                    .WithMessage("When 'SupportH2C' is enabled, the 'URL' must use the HTTP scheme.");

                RuleFor(dto => dto.HttpVersion)
                    .Must(httpVersion =>
                    {
                        // Ensure HttpVersion is 2.0 when SupportH2C is enabled
                        return string.IsNullOrEmpty(httpVersion)
                        || httpVersion.StartsWith("$")
                        || httpVersion.Equals("2.0", StringComparison.OrdinalIgnoreCase);
                    })
                    .WithMessage("When 'SupportH2C' is enabled, the 'HttpVersion' must be set to 2.0.");
            });

            RuleFor(dto => dto.Capture)
                .SetValidator(new CaptureValidator(new CaptureHandlerDto()));
        }
        private static bool IsValidFilePath(string path)
        {
            try
            {
                // Resolve the placeholder if needed and get the absolute path
                string fullPath = Path.GetFullPath(path, AppConstants.EnvironmentCurrentDirectory);

                // Check if the file exists
                return !string.IsNullOrEmpty(fullPath) && (path.StartsWith('$') || File.Exists(fullPath));
            }
            catch (Exception)
            {
                // If any exception occurs (e.g., invalid path characters), the path is invalid
                return false;
            }
        }
        public override HttpRequestDto Dto { get { return _requestDto; } }
    }
}
