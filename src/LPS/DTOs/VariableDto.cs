using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.DTOs
{
    //TODO Create Domain entities for those so we can store them in DB once implemented.
    public class VariableDto : IDto<VariableDto>
    {
        public VariableDto()
        {
            As = string.Empty;
            Regex = string.Empty;
            Name = string.Empty;
            Value = string.Empty;
        }

        // Variable name
        public string Name { get; set; }

        // Variable value
        public string Value { get; set; }

        // Type information
        public string As { get; set; }

        // Regex pattern for validation
        public string Regex { get; set; }

        // Deep copy method
        public void DeepCopy(out VariableDto targetDto)
        {
            targetDto = new VariableDto
            {
                Name = this.Name, // Copy Name
                Value = this.Value, // Copy Value
                As = this.As, // Copy As
                Regex = this.Regex // Copy Regex
            };
        }
    }
}
