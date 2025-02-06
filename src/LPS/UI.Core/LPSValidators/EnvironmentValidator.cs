using FluentValidation;
using LPS.DTOs;

namespace LPS.UI.Core.LPSValidators
{
    internal class EnvironmentValidator : AbstractValidator<EnvironmentDto>
    {
        public EnvironmentValidator()
        {
            // Validate Name property
            RuleFor(env => env.Name)
                .NotNull().NotEmpty().WithMessage("'Environment Name' must not be empty");

            // Validate Variables property using VariableValidator
            RuleForEach(env => env.Variables)
                .SetValidator(new VariableValidator())
                .WithMessage("Invalid variable detected in the environment");
        }
    }
}
