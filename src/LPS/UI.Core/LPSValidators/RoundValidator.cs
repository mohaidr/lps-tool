using LPS.Domain;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FluentValidation;
using LPS.DTOs;

namespace LPS.UI.Core.LPSValidators
{
    internal partial class RoundValidator : CommandBaseValidator<RoundDto>
    {
        readonly RoundDto _roundDto;
        public RoundValidator(RoundDto roundDto)
        {
            ArgumentNullException.ThrowIfNull(roundDto);

            _roundDto = roundDto;

            RuleFor(dto => dto.Name)
                .NotNull()
                .WithMessage("The 'Name' must be a non-null value")
                .NotEmpty()
                .WithMessage("The 'Name' must not be empty")
                .Must(name =>
                {
                    // Allow valid names or placeholders
                    return (!string.IsNullOrEmpty(name) && name.StartsWith("$")) || NameRegex().IsMatch(name ?? string.Empty);
                })
                .WithMessage("The 'Name' must either start with '$' (for placeholders) or match the pattern: only alphanumeric characters, spaces, underscores, periods, and dashes are allowed")
                .Length(1, 60)
                .WithMessage("The 'Name' should be between 1 and 60 characters");

            RuleFor(dto => dto.BaseUrl)
                .Must(url =>
                {
                    // Allow valid URLs or placeholders
                    return string.IsNullOrEmpty(url) || url.StartsWith("$") || Uri.TryCreate(url, UriKind.Absolute, out Uri result)
                        && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
                })
                .When(dto => !string.IsNullOrEmpty(dto.BaseUrl))
                .WithMessage("The 'BaseUrl' must be a valid URL according to RFC 3986 or a placeholder starting with '$'");


            // Validation for Iterations
            RuleFor(dto => dto.Iterations)
                .Must(HaveUniqueIterationNames)
                .WithMessage("The Iteration 'Name' must be unique.")
                .ForEach(iteration =>
                {
                    iteration.SetValidator(new IterationValidator(new HttpIterationDto()));
                });

            RuleFor(dto => dto.StartupDelay)
                .Must(startupDelay =>
                {
                    // Allow valid numeric values or placeholders
                    return (int.TryParse(startupDelay, out int parsedValue) && parsedValue >= 0) || string.IsNullOrEmpty(startupDelay) || startupDelay.StartsWith("$");
                })
                .WithMessage("The 'StartupDelay' must be greater than or equal to 0 or a placeholder starting with '$'");

            RuleFor(dto => dto.NumberOfClients)
                .NotNull()
                .WithMessage("The 'Number Of Clients' must not be null.")
                .Must(numberOfClients =>
                {
                    // Allow valid numeric values or placeholders
                    return (!string.IsNullOrEmpty(numberOfClients) && numberOfClients.StartsWith("$"))
                        || int.TryParse(numberOfClients, out int parsedValue) && parsedValue > 0;
                })
                .WithMessage("The 'Number Of Clients' must be a positive integer or a placeholder starting with '$'");

            RuleFor(dto => dto.ArrivalDelay)
            .Must(arrivalDelay =>
            {
                // Allow valid numeric values or placeholders
                return (int.TryParse(arrivalDelay, out int parsedValue) && parsedValue > 0) || (!string.IsNullOrEmpty(arrivalDelay) && arrivalDelay.StartsWith("$"));
            })
            .When(dto => dto.NumberOfClients != null && int.TryParse(dto.NumberOfClients, out int parsedClients) && parsedClients > 1)
            .WithMessage("The 'Arrival Delay (--arrivalDelay)' must be a greater than 0 or a placeholder starting with '$' when 'Number Of Clients' > 1");

            RuleFor(dto => dto.DelayClientCreationUntilIsNeeded)
                .Must(value =>
                {
                    // Allow valid boolean values or placeholders
                    return string.IsNullOrEmpty(value) || value.StartsWith("$") || bool.TryParse(value, out _);
                })
                .WithMessage("'Delay Client Creation Until Is Needed' must be 'true', 'false', or a placeholder starting with '$'");

            RuleFor(dto => dto.RunInParallel)
            .Must(value =>
            {
                // Allow valid boolean values or placeholders
                return string.IsNullOrEmpty(value) 
                || value.StartsWith("$") 
                || bool.TryParse(value, out _);
            })
            .WithMessage("'Run In Parallel' must be 'true', 'false', or a placeholder starting with '$'");
        }
        private bool HaveUniqueIterationNames(IList<HttpIterationDto> iterations)
        {
            if (iterations == null) return true;

            // Check for duplicate names in the provided rounds list
            var iterationsNames = iterations.Select(iteration => iteration.Name).ToList();
            return iterationsNames.Count == iterationsNames.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        }
        public override RoundDto Dto { get { return _roundDto; } }

        [GeneratedRegex("^[a-zA-Z0-9 _.-]+$")]
        private static partial Regex NameRegex();
    }
}
