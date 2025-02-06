using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Validation;
using LPS.Domain.LPSSession;

namespace LPS.Domain
{

    public partial class HttpRequest
    {
        public new class Validator : CommandBaseValidator<HttpRequest, HttpRequest.SetupCommand>
        {
            ILogger _logger;
            IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            HttpRequest _entity;
            HttpRequest.SetupCommand _command;
            private string[] _httpMethods = { "GET", "HEAD", "POST", "PUT", "PATCH", "DELETE", "CONNECT", "OPTIONS", "TRACE" };
            public Validator(HttpRequest entity, HttpRequest.SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
            {
                _logger = logger;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _entity = entity;
                _command = command;

                #region Validation Rules
                RuleFor(command => command.HttpVersion)
                    .NotEmpty()
                    .WithMessage("The 'HttpVersion' cannot be null or empty.")
                    .Must(version => string.IsNullOrEmpty(version)
                        || version.StartsWith("$")
                        || version == "1.0"
                        || version == "1.1"
                        || version == "2.0")
                    .WithMessage("The accepted 'Http Versions' are (\"1.0\", \"1.1\", \"2.0\") or placeholders starting with '$'")
                    .Must((command, version) =>
                        string.IsNullOrEmpty(version)
                        || !command.SupportH2C.HasValue
                        || !command.SupportH2C.Value
                        || version.Equals("2.0"))
                    .WithMessage("H2C only works with HTTP/2");

                RuleFor(command => command.HttpMethod)
                    .NotEmpty()
                    .WithMessage("The 'HttpMethod' cannot be null or empty.")
                    .Must(httpMethod => string.IsNullOrEmpty(httpMethod)
                        || httpMethod.StartsWith("$")
                        || _httpMethods.Any(method => method.Equals(httpMethod, StringComparison.OrdinalIgnoreCase)))
                    .WithMessage("The supported 'Http Methods' are (\"GET\", \"HEAD\", \"POST\", \"PUT\", \"PATCH\", \"DELETE\", \"CONNECT\", \"OPTIONS\", \"TRACE\") or placeholders starting with '$'");

                RuleFor(command => command.Url)
                    .NotEmpty()
                    .WithMessage("The 'URL' cannot be null or empty.")
                    .Must(url => string.IsNullOrEmpty(url?.Url)
                        || url.Url.StartsWith("$")
                        || (Uri.TryCreate(url?.Url, UriKind.Absolute, out Uri result)
                            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps)))
                    .WithMessage("The 'URL' must be a valid URL according to RFC 3986 or a placeholder starting with '$'")
                    .Must((command, url) =>
                        string.IsNullOrEmpty(url?.Url)
                        || url.Url.StartsWith("$")
                        || !command.SupportH2C.HasValue
                        || !command.SupportH2C.Value
                        || url.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("H2C only works with the HTTP schema or placeholders starting with '$'");

                RuleFor(command => command.SupportH2C)
                    .NotNull()
                    .When(command =>
                        !string.IsNullOrEmpty(command.Url?.Url)
                        && command.Url.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(command.HttpVersion)
                        && command.HttpVersion.Equals("2.0"))
                    .WithMessage("'SupportH2C' must be true or false");

                RuleFor(command => command.SaveResponse)
                    .NotNull()
                    .WithMessage("'SupportH2C' must be true or false");

                RuleFor(command => command.DownloadHtmlEmbeddedResources)
                    .NotNull()
                    .WithMessage("'Download Html Embedded Resources' must be true or false");

                // Enforce HTTP when SupportH2C is true
                When(command => command.SupportH2C == true, () =>
                {
                    RuleFor(command => command.Url)
                        .Must(url =>
                        {
                            return string.IsNullOrEmpty(url?.Url) || url.Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
                        })
                        .WithMessage("When 'SupportH2C' is enabled, the 'URL' must use the HTTP scheme.");
                    RuleFor(command => command.HttpVersion)
                        .Must(httpVersion =>
                        {
                            return string.IsNullOrEmpty(httpVersion) || httpVersion.Equals("2.0");
                        })
                        .WithMessage("When 'SupportH2C' is enabled, the 'Http Version' must be set to 2.0.");
                });

                When(command => command.Payload != null, () =>
                {
                    if (command.Payload.Type == Payload.PayloadType.Multipart)
                    {
                        RuleFor(command => command.Payload.Multipart)
                        .NotNull()
                        .Must(multipart =>
                            (multipart?.Files != null && multipart.Files.Count > 0) ||
                            (multipart?.Fields != null && multipart.Fields.Count > 0))
                        .WithMessage("The 'Multipart' object must contain at least one file or one field.");

                        RuleFor(command => command.Payload.Multipart)
                        .Must(multipart => multipart == null 
                            || multipart?.Files == null 
                            || multipart.Files.All(file => !string.IsNullOrWhiteSpace(file.Name) && file.Content!= null && !string.IsNullOrWhiteSpace(file.ContentType)))
                        .WithMessage("All files in the Multipart must have both field name and value.");

                        RuleFor(command => command.Payload.Multipart)
                        .Must(multipart => multipart == null 
                            || multipart?.Fields== null 
                            || multipart.Fields.All(field => !string.IsNullOrWhiteSpace(field.Name) && !string.IsNullOrWhiteSpace(field.Value) && !string.IsNullOrWhiteSpace(field.ContentType)))
                        .WithMessage("All fields in the Multipart must have both field name and value.");
                    }
                    else if (command.Payload.Type == Payload.PayloadType.Binary)
                    {
                        RuleFor(dto => dto.Payload.BinaryValue)
                        .NotNull();
                    }
                });


                //TODO: Validate http headers
                #endregion

                if (entity.Id != default && command.Id.HasValue && entity.Id != command.Id)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "LPS Request Profile: Entity Id Can't be Changed, The Id value will be ignored", LPSLoggingLevel.Warning);
                }

                _command.IsValid = base.Validate();
            }

            public override SetupCommand Command => _command;
            public override HttpRequest Entity => _entity;
        }
    }
}
