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
    internal partial class PlanValidator : CommandBaseValidator<PlanDto>
    {
        readonly PlanDto _planDto;
        public PlanValidator(PlanDto planDto)
        {
            ArgumentNullException.ThrowIfNull(planDto);
            _planDto = planDto;


            RuleFor(dto => dto.Name)
                .NotNull()
                .WithMessage("The 'Name' must be a non-null value")
                .NotEmpty()
                .WithMessage("The 'Name' must not be empty")
                .Must(name =>
                {
                    // Check if the name matches the regex or starts with a placeholder
                    return (!string.IsNullOrEmpty(name) && name.StartsWith("$")) || NameRegex().IsMatch(name ?? string.Empty);
                })
                .WithMessage("The 'Name' must either start with '$' (for placeholders) or match the pattern: only alphanumeric characters, spaces, underscores, periods, and dashes are allowed")
                .Length(1, 60)
                .WithMessage("The 'Name' should be between 1 and 60 characters");

            // Validation for Rounds
            RuleFor(dto => dto.Rounds)
                .Must(HaveUniqueRoundNames)
                .WithMessage("The Round 'Name' must be unique.")
                .ForEach(round =>
                {
                    round.SetValidator(new RoundValidator(new RoundDto()));
                });

            // Validation for Iterations
            RuleFor(dto => dto.Iterations)
                .Must(HaveUniqueIterationNames)
                .WithMessage("The Iteration 'Name' must be unique.")
                .ForEach(iteration =>
                {
                    iteration.SetValidator(new IterationValidator(new HttpIterationDto()));
                });

            // Validation for Variables
            RuleForEach(dto => dto.Variables)
                .SetValidator(new VariableValidator());

            // Validation for Environments
            RuleForEach(dto => dto.Environments)
                .SetValidator(new EnvironmentValidator());
        }
        private bool HaveUniqueRoundNames(IList<RoundDto> rounds)
        {
            if (rounds == null) return true;

            // Check for duplicate names in the provided rounds list
            var roundNames = rounds.Select(round => round.Name).ToList();
            return roundNames.Count == roundNames.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        }

        private bool HaveUniqueIterationNames(IList<HttpIterationDto> iterations)
        {
            if (iterations == null) return true;

            // Check for duplicate names in the provided rounds list
            var iterationsNames = iterations.Select(iteration => iteration.Name).ToList();
            return iterationsNames.Count == iterationsNames.Distinct(StringComparer.OrdinalIgnoreCase).Count();
        }
        public override PlanDto Dto { get { return _planDto; } }

        [GeneratedRegex("^[a-zA-Z0-9 _.-]+$")]
        private static partial Regex NameRegex();
    }
}
