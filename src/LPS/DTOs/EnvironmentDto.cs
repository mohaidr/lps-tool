using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.DTOs
{
    public class EnvironmentDto : IDto<EnvironmentDto>
    {
        public EnvironmentDto()
        {
            Name = string.Empty;
            Variables = new List<VariableDto>();
        }

        // Environment name
        public string Name { get; set; }

        // List of variables in the environment
        public IList<VariableDto> Variables { get; set; }

        // Deep copy method
        public void DeepCopy(out EnvironmentDto targetDto)
        {
            targetDto = new EnvironmentDto
            {
                Name = this.Name, // Copy Name directly as it's a string (immutable)
                Variables = this.Variables?.Select(variable =>
                {
                    var copiedVariable = new VariableDto();
                    variable.DeepCopy(out copiedVariable);
                    return copiedVariable;
                }).ToList() ?? new List<VariableDto>()
            };
        }
    }

}
