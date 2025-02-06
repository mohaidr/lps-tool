using FluentValidation;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.LPSFlow.LPSHandlers
{
    public partial class CaptureHandler : ISessionHandler
    {
        public class Validator : CommandBaseValidator<CaptureHandler, CaptureHandler.SetupCommand>
        {
            ILogger _logger;
            IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            CaptureHandler _entity;
            SetupCommand _command;

            public Validator(CaptureHandler entity, SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
            {
                _logger = logger;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _entity = entity;
                _command = command;
                if (entity.Id != default && command.Id.HasValue && entity.Id != command.Id)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "LPS CapturHandler: Entity Id Can't be Changed, The Id value will be ignored", LPSLoggingLevel.Warning);
                }

                #region Validation Rules
                RuleFor(command => command.To)
                    .NotNull().NotEmpty()
                    .WithMessage("'Variable Name' must not be empty")
                    .Matches("^[a-zA-Z0-9]+$")
                    .WithMessage("'Variable Name' must only contain letters and numbers.");

                RuleFor(command => command.MakeGlobal)
                    .NotNull();
                RuleFor(command => command.As)
                    .Must(@as =>
                    {
                        @as ??= string.Empty;
                        return @as.Equals("JSON", StringComparison.OrdinalIgnoreCase)
                        || @as.Equals("XML", StringComparison.OrdinalIgnoreCase)
                        || @as.Equals("Text", StringComparison.OrdinalIgnoreCase)
                        || @as.Equals("CSV", StringComparison.OrdinalIgnoreCase)
                        || @as == string.Empty;
                    }).WithMessage($"The provided value for 'As' ({command?.As}) is not valid or supported.");
                RuleFor(command => command.Regex)
                .Must(regex => string.IsNullOrEmpty(regex) || IsValidRegex(regex))
                .WithMessage("Input must be either empty or a valid .NET regular expression.");

                #endregion
                _command.IsValid = base.Validate();
            }

            private bool IsValidRegex(string pattern)
            {
                try
                {
                    // If the Regex object can be created without exceptions, the pattern is valid
                    _ = new System.Text.RegularExpressions.Regex(pattern);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public override SetupCommand Command => _command;
            public override CaptureHandler Entity => _entity;


        }

    }
}
