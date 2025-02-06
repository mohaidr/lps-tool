using FluentValidation;
using FluentValidation.Results;
using LPS.Domain.Common.Interfaces;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LPS.Domain.Domain.Common.Validation
{
    public abstract class CommandBaseValidator<TEntity, TCommand> : AbstractValidator<TCommand>, IDomainValidator<TEntity, TCommand> where TEntity : IDomainEntity where TCommand : ICommand<TEntity>
    {
        FluentValidation.Results.ValidationResult _validationResult;
        public abstract TCommand Command { get; }
        public abstract TEntity Entity { get; }
        public void PrintValidationErrors()
        {
            if (!_validationResult.IsValid)
            {
                foreach (var error in _validationResult.Errors)
                {
                    AnsiConsole.MarkupLine(string.Concat("[red]- ", Markup.Escape(error.ErrorMessage), "[/]"));
                }
            }
        }

        public bool Validate()
        {

            if (Command == null)
            {
                AnsiConsole.MarkupLine(string.Concat("[red]- ", "Invalid Entity Command", "[/]"));
                throw new ArgumentNullException(nameof(Command));
            }
            if (Entity == null)
            {
                throw new InvalidOperationException("Invalid Entity State");
            }

            _validationResult = base.Validate(Command);
            return _validationResult.IsValid;
        }
    }
}
