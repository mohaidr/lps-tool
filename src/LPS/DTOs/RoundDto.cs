using LPS.Domain;
using LPS.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace LPS.DTOs
{
    public class RoundDto : IDto<RoundDto>
    {
        public RoundDto()
        {
            Name = string.Empty;
            BaseUrl = string.Empty;
            Iterations = new List<HttpIterationDto>();
            ReferencedIterations = new List<string>();
            Tags = new List<string>();
        }

        // Name of the round
        public string Name { get; set; }

        // Base URL for the round
        public string BaseUrl { get; set; }

        public string StartupDelay { get; set; }

        // Number of clients (can be a variable)
        public string NumberOfClients { get; set; }

        // Arrival delay (can be a variable)
        public string ArrivalDelay { get; set; }

        // Inline iterations for this round
        public List<HttpIterationDto> Iterations { get; set; }

        // Referenced iterations for this round
        [JsonPropertyName("reference")]
        [YamlMember(Alias = "reference")]
        public List<string> ReferencedIterations { get; set; }

        // Startup delay (can be a variable)

        // Delay client creation until needed (can be a variable)
        public string DelayClientCreationUntilIsNeeded { get; set; }

        // Run in parallel (can be a variable)
        public string RunInParallel { get; set; }

        // Tags associated with the round
        public IList<string> Tags { get; set; }

        // Deep copy method to create a new instance with the same data
        public void DeepCopy(out RoundDto targetDto)
        {
            targetDto = new RoundDto
            {
                Name = this.Name,
                BaseUrl = this.BaseUrl,
                StartupDelay = this.StartupDelay,
                NumberOfClients = this.NumberOfClients,
                ArrivalDelay = this.ArrivalDelay,
                DelayClientCreationUntilIsNeeded = this.DelayClientCreationUntilIsNeeded,
                RunInParallel = this.RunInParallel,
                Tags = new List<string>(this.Tags),
                Iterations = this.Iterations?.Select(iteration =>
                {
                    var copiedIteration = new HttpIterationDto();
                    iteration.DeepCopy(out copiedIteration);
                    return copiedIteration;
                }).ToList() ?? new List<HttpIterationDto>(),
                ReferencedIterations = this.ReferencedIterations?.ToList() ?? new List<string>()
            };
        }
    }

}
