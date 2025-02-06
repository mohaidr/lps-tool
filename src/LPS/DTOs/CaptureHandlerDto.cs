using LPS.Domain.LPSFlow.LPSHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace LPS.DTOs
{
    public class CaptureHandlerDto : IDto<CaptureHandlerDto>
    {
        public CaptureHandlerDto()
        {
            To = string.Empty;
            As = string.Empty;
            Regex = string.Empty;
            MakeGlobal = "false"; // Support placeholders for boolean values
            Headers = new List<string>();
        }

        // Name of the capture handler
        public string To { get; set; }

        // Type information
        public string As { get; set; }

        // Whether the capture should be global (supports placeholders)
        public string MakeGlobal { get; set; }

        // Regex pattern for capturing
        public string Regex { get; set; }

        // List of headers
        public IList<string> Headers { get; set; }

        // Deep copy method
        public void DeepCopy(out CaptureHandlerDto targetDto)
        {
            targetDto = new CaptureHandlerDto
            {
                To = this.To,
                As = this.As,
                Regex = this.Regex,
                MakeGlobal = this.MakeGlobal,
                Headers = new List<string>(this.Headers) // Create a new list to ensure deep copying
            };
        }
    }

}
