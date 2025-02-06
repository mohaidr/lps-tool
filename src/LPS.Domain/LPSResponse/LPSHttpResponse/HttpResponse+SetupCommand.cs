using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Exceptions;
using YamlDotNet.Serialization;

namespace LPS.Domain
{

    public partial class HttpResponse
    {
        new public class SetupCommand : ICommand<HttpResponse>, IValidCommand<HttpResponse>
        {
            public SetupCommand()
            {
                ValidationErrors = new Dictionary<string, List<string>>();
            }
            [JsonIgnore]
            [YamlIgnore]
            public Guid? Id { get; set; }

            public MimeType ContentType { get; set; }
            public string LocationToResponse { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public string StatusMessage { get; set; }
            public Dictionary<string, string> ResponseContentHeaders { get; set; }
            public Dictionary<string, string> ResponseHeaders { get; set; }
            public bool IsSuccessStatusCode { get; set; }
            [JsonIgnore]
            [YamlIgnore]
            public bool IsValid { get; set; }
            public Guid HttpRequestId { get; set; }
            [JsonIgnore]
            [YamlIgnore]
            public IDictionary<string, List<string>> ValidationErrors { get; set; }
            public TimeSpan TotalTime { get; set; }

            public void Execute(HttpResponse entity)
            {
                ArgumentNullException.ThrowIfNull(entity);
                entity?.Setup(this);
            }
            public HttpRequest.SetupCommand HttpRequest { get; set; }

            public void Copy(SetupCommand targetCommand)
            {
                targetCommand.Id = this.Id;
                targetCommand.ContentType = this.ContentType;
                targetCommand.LocationToResponse = this.LocationToResponse;
                targetCommand.StatusCode = this.StatusCode;
                targetCommand.StatusMessage = this.StatusMessage;
                targetCommand.TotalTime = this.TotalTime;
                targetCommand.IsSuccessStatusCode = this.IsSuccessStatusCode;
                targetCommand.IsValid = this.IsValid;
                targetCommand.HttpRequestId = this.HttpRequestId;

                // Deep copy of dictionaries
                targetCommand.ResponseContentHeaders = new Dictionary<string, string>(this.ResponseContentHeaders);
                targetCommand.ResponseHeaders = new Dictionary<string, string>(this.ResponseHeaders);

                // Deep copy of ValidationErrors dictionary and its inner lists
                targetCommand.ValidationErrors = this.ValidationErrors.ToDictionary(
                    entry => entry.Key,
                    entry => new List<string>(entry.Value)
                );
                // Assuming HttpRequest has its own Clone method
                this.HttpRequest?.Copy(targetCommand.HttpRequest);
            }

        }

        public void SetHttpRequest(HttpRequest httpRequest)
        {

            if (httpRequest != null && httpRequest.IsValid) { HttpRequest = httpRequest; }
            else
            {
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"The referenced LPS Entity of type {typeof(HttpRequest)} is either null or invalid.", LPSLoggingLevel.Error);
                throw new InvalidLPSEntityException($"The referenced LPS Entity of type {typeof(HttpRequest)} is either null or invalid.");
            }
        }
        protected void Setup(SetupCommand command)
        {
            //Set the inherited properties through the parent entity setup command
            var lPSResponseSetUpCommand = new Response.SetupCommand() { Id = command.Id };
            base.Setup(lPSResponseSetUpCommand);
            //TODO: DeepCopy and then send the copy item instead of the original command for further protection 
            var validator = new Validator(this, command, _logger, _runtimeOperationIdProvider);
            if (command.IsValid && lPSResponseSetUpCommand.IsValid)
            {
                this.LocationToResponse = command.LocationToResponse;
                this.StatusCode = command.StatusCode;
                this.ContentType = command.ContentType;
                this.IsSuccessStatusCode = command.IsSuccessStatusCode;
                this.ResponseHeaders = new Dictionary<string, string>();
                this.ResponseContentHeaders = new Dictionary<string, string>();
                this.StatusMessage = command.StatusMessage;
                this.TotalTime = command.TotalTime;
                if (command.ResponseHeaders != null)
                {
                    foreach (var header in command.ResponseHeaders)
                    {
                        this.ResponseHeaders.Add(header.Key, header.Value);
                    }
                }
                if (command.ResponseContentHeaders != null)
                {
                    foreach (var header in command.ResponseContentHeaders)
                    {
                        this.ResponseContentHeaders.Add(header.Key, header.Value);
                    }
                }
                this.IsValid = true;
            }
            else
            {
                this.IsValid = false;
                validator.PrintValidationErrors();
            }
        }
    }
}
