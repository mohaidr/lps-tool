#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Exceptions;
using LPS.Domain.LPSFlow.LPSHandlers;
using LPS.Domain.LPSRequest.LPSHttpRequest;
using LPS.Domain.LPSSession;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace LPS.Domain
{

    public partial class HttpRequest
    {
        new public class SetupCommand : ICommand<HttpRequest>, IValidCommand<HttpRequest>
        {
            public SetupCommand()
            {
                HttpVersion = "2.0";
                DownloadHtmlEmbeddedResources = false;
                SaveResponse = false;
                HttpHeaders = [];
                ValidationErrors = new Dictionary<string, List<string>>();
            }
            public Guid? Id { get; set; }
            public URL Url { get; set; }
            public string HttpMethod { get; set; }
            public string HttpVersion { get; set; }
            public Dictionary<string, string> HttpHeaders { get; set; }
            public Payload? Payload { get; protected set; }
            public bool? DownloadHtmlEmbeddedResources { get; set; }
            public bool? SaveResponse { get; set; }
            public bool? SupportH2C { get; set; }

            public bool IsValid { get; set; }
            public IDictionary<string, List<string>> ValidationErrors { get; set; }
            public void Execute(HttpRequest entity)
            {
                ArgumentNullException.ThrowIfNull(entity);
                entity?.Setup(this);
            }

            public void Copy(SetupCommand targetCommand)
            {
                targetCommand.Id = this.Id;
                targetCommand.Url = this.Url;
                targetCommand.HttpMethod = this.HttpMethod;
                targetCommand.HttpVersion = this.HttpVersion;
                targetCommand.HttpHeaders = new Dictionary<string, string>(this.HttpHeaders);
                targetCommand.Payload = this.Payload;
                targetCommand.DownloadHtmlEmbeddedResources = this.DownloadHtmlEmbeddedResources;
                targetCommand.SaveResponse = this.SaveResponse;
                targetCommand.SupportH2C = this.SupportH2C;
                targetCommand.IsValid = this.IsValid;
                targetCommand.ValidationErrors = this.ValidationErrors?.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value));
            }
        }

        public void SetCapture(CaptureHandler capture) 
        {
            if (capture != null && capture.IsValid)
            {
                this.Capture = capture;
            }
            else
            {
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"The referenced LPS Entity of type {typeof(CaptureHandler)} is either null or invalid.", LPSLoggingLevel.Error);
                throw new InvalidLPSEntityException($"The referenced LPS Entity of type {typeof(CaptureHandler)} is either null or invalid.");
            }
        }

        protected void Setup(SetupCommand command)
        {
            //Set the inherited properties through the parent entity setup command
            var requestSetUpCommand = new Request.SetupCommand() { Id = command.Id };
            base.Setup(requestSetUpCommand);
            //TODO: DeepCopy and then send the copy item instead of the original command for further protection 
            var validator = new Validator(this, command, _logger, _runtimeOperationIdProvider);
            if (command.IsValid && requestSetUpCommand.IsValid)
            {
                this.HttpMethod = command.HttpMethod;
                this.HttpVersion = command.HttpVersion;
                this.Url = new URL(command.Url.Url);
                if (command.Payload?.Type == Payload.PayloadType.Raw)
                {
                    this.Payload = Payload.CreateRaw(command.Payload.RawValue);
                }
                else if (command.Payload?.Type == Payload.PayloadType.Multipart)
                {
                    this.Payload = Payload.CreateMultipart(command.Payload.Multipart.Fields, command.Payload.Multipart.Files);
                }
                else if (command.Payload?.Type == Payload.PayloadType.Binary)
                {
                    this.Payload = Payload.CreateBinary(command.Payload.BinaryValue);
                }

                this.HttpHeaders = [];
                this.DownloadHtmlEmbeddedResources = command.DownloadHtmlEmbeddedResources.HasValue ? command.DownloadHtmlEmbeddedResources.Value : false;
                this.SaveResponse = command.SaveResponse ?? false;
                this.SupportH2C = command.SupportH2C;
                if (command.HttpHeaders != null)
                {
                    foreach (var header in command.HttpHeaders)
                    {
                        this.HttpHeaders.Add(header.Key, header.Value);
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

        public object Clone()
        {
            HttpRequest clone = new(_logger, _runtimeOperationIdProvider);
            if (this.IsValid)
            {
                clone.Id = this.Id;
                clone.HttpMethod = this.HttpMethod;
                clone.HttpVersion = this.HttpVersion;
                clone.Url = this.Url; // intentionally not doing deep clone here. TODO: review performence and then implement deep clone
                clone.Payload = this.Payload; // intentionally not doing deep clone here TODO: review performence and then implement deep clone
                clone.SaveResponse = this.SaveResponse;
                clone.SupportH2C = this.SupportH2C;
                clone.Capture =  (CaptureHandler)this.Capture?.Clone();
                clone.HttpHeaders = [];
                if (this.HttpHeaders != null)
                {
                    foreach (var header in this.HttpHeaders)
                    {
                        clone.HttpHeaders.Add(header.Key, header.Value);
                    }
                }
                clone.IsValid = true;
            }
            return clone;
        }
    }
}
