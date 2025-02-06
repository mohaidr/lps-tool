using FluentValidation;
using LPS.DTOs;
using LPS.Infrastructure.LPSClients.SessionManager;
using LPS.Domain.Common;

namespace LPS.UI.Core.LPSValidators
{
    internal class VariableValidator : AbstractValidator<VariableDto>
    {
        public VariableValidator()
        {
            RuleFor(variable => variable.Name)
                .NotNull().NotEmpty()
                .WithMessage("'Variable Name' must not be empty")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("'Variable Name' must only contain letters and numbers.");
            
            RuleFor(variable => variable.Value)
            .NotNull()
            .NotEmpty()
            .WithMessage("'Variable Value' must not be empty");

            RuleFor(variable => variable.As)
                .Must(@as =>
                {
                    @as ??= string.Empty;
                    return IVariableHolder.IsKnownSupportedFormat(MimeTypeExtensions.FromKeyword(@as))
                    || @as == string.Empty;
                }).WithMessage("The provided value for 'As' is not valid or supported.");
            RuleFor(variable => variable.Regex)
                .NotNull().WithMessage("'Regex' must be a non-null value");
        }
    }
}
