using LPS.Domain;
using LPS.Domain.LPSSession;
using LPS.Infrastructure.Common;
using LPS.Infrastructure.Common.LPSSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace LPS.DTOs
{
    public class HttpRequestDto : IDto<HttpRequestDto>
    {
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public HttpRequestDto()
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            HttpHeaders = [];
            HttpVersion = "2.0";
        }

        // URL for the HTTP request (supports placeholders)
        [YamlAlias("url")]
        [JsonAlias("url")]
        public string URL { get; set; }

        // HTTP method (supports placeholders)
        [YamlAlias("method")]
        [JsonAlias("method")]
        public string HttpMethod { get; set; }

        // HTTP version (supports placeholders)
        [YamlAlias("version")]
        [JsonAlias("version")]
        public string HttpVersion { get; set; }

        // HTTP headers
        [YamlAlias("headers")]
        [JsonAlias("headers")]
        public Dictionary<string, string> HttpHeaders { get; set; }

        public PayloadDto Payload { get; set; }

        // Whether to download HTML embedded resources (supports placeholders)
        public string DownloadHtmlEmbeddedResources { get; set; }

        // Whether to save the response (supports placeholders)
        public string SaveResponse { get; set; }

        // Whether to support H2C (HTTP/2 over cleartext) (supports placeholders)
        public string SupportH2C { get; set; }

        // Capture handler for the HTTP request
        public CaptureHandlerDto Capture { get; set; }

        // Deep copy method
        public void DeepCopy(out HttpRequestDto targetDto)
        {
            #pragma warning disable CS8601 // Possible null reference assignment.
            targetDto = new HttpRequestDto
            {
                URL = this.URL,
                HttpMethod = this.HttpMethod,
                HttpVersion = this.HttpVersion,
                Payload = this.Payload.CloneObject(),
                DownloadHtmlEmbeddedResources = this.DownloadHtmlEmbeddedResources,
                SaveResponse = this.SaveResponse,
                SupportH2C = this.SupportH2C,
                HttpHeaders = this.HttpHeaders?.ToDictionary(entry => entry.Key, entry => entry.Value)
            };
            #pragma warning restore CS8601 // Possible null reference assignment.
            // Deep copy the Capture object if it exists
            if (this.Capture != null)
            {
                var copiedCapture = new CaptureHandlerDto();
                this.Capture.DeepCopy(out copiedCapture);
                targetDto.Capture = copiedCapture;
            }
        }
    }
}
