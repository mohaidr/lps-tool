using LPS.Domain;
using System;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace LPS.DTOs
{
    public class HttpIterationDto : IDto<HttpIterationDto>
    {
        public HttpIterationDto()
        {
            Name = string.Empty;
            HttpRequest = new HttpRequestDto();
        }

        // Name of the iteration
        public string Name { get; set; }

        // HTTP request details
        public HttpRequestDto HttpRequest { get; set; }

        // Startup delay (can be a variable)
        public string StartupDelay { get; set; }

        // Maximize throughput (can be a variable)
        public string MaximizeThroughput { get; set; }

        // Iteration mode (can be a variable)
        public string Mode { get; set; }

        // Request count (can be a variable)
        public string RequestCount { get; set; }

        // Duration (can be a variable)
        public string Duration { get; set; }

        // Batch size (can be a variable)
        public string BatchSize { get; set; }

        // Cooldown time (can be a variable)
        public string CoolDownTime { get; set; }

        // Deep copy method to create a new instance with the same data
        public void DeepCopy(out HttpIterationDto targetDto)
        {
            targetDto = new HttpIterationDto
            {
                Name = this.Name,
                StartupDelay = this.StartupDelay,
                MaximizeThroughput = this.MaximizeThroughput,
                Mode = this.Mode,
                RequestCount = this.RequestCount,
                Duration = this.Duration,
                BatchSize = this.BatchSize,
                CoolDownTime = this.CoolDownTime,
            };

            // Deep copy HttpRequest
            var copiedHttpRequest = new HttpRequestDto();
            HttpRequest.DeepCopy(out copiedHttpRequest);
            targetDto.HttpRequest = copiedHttpRequest;
        }
    }
}
