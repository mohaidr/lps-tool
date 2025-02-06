using FluentValidation;
using FluentValidation.Results;
using LPS.Domain.Common.Interfaces;
using LPS.UI.Common;
using LPS.UI.Common.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LPS.Infrastructure.Common;
using Spectre.Console;
using ValidationResult = FluentValidation.Results.ValidationResult;
using System;
using LPS.DTOs;

namespace LPS.UI.Core
{
    public abstract class CommandBaseValidator<TDto> : AbstractValidator<TDto>, IBaseValidator<TDto>
        where TDto : IDto<TDto>
    {
        public abstract TDto Dto { get; }

        ValidationResult _validationResults;

        public Dictionary<string, List<string>> ValidationErrors => _validationResults.Errors
        .GroupBy(error => error.PropertyName)
        .ToDictionary(
            group => group.Key,
            group => group.Select(error => error.ErrorMessage).ToList()
        );

        public bool Validate(string property)
        {
            _validationResults = Validate(Dto);
            return !_validationResults.Errors.Any(error => error.PropertyName == property);
        }

        public void ValidateAndThrow(string property)
        {
            _validationResults = Validate(Dto);
            if (!_validationResults.Errors.Any(error => error.PropertyName == property))
            {
                StringBuilder errorMessage = new StringBuilder("Validation failed. Details:\n");
                foreach (var error in ValidationErrors)
                {
                    errorMessage.AppendLine($"{error.Key}: {error.Value}");
                }
                throw new Common.ValidationException(errorMessage.ToString());
            }
        }
        public void PrintValidationErrors(string property)
        {
            if (ValidationErrors.Keys.Contains(property))
            {
                AnsiConsole.MarkupLine(string.Concat("[Orange3]- ", Markup.Escape(string.Join("\n- ", ValidationErrors[property])), "[/]"));
            }
        }

        public ValidationResult Validate()
        {
            _validationResults = base.Validate(Dto);
            return _validationResults;
        }
    }
}
