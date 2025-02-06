using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Validation;

namespace LPS.Domain
{
    public partial class Plan
    {
        public class Validator : CommandBaseValidator<Plan, Plan.SetupCommand>
        {
            ILogger _logger;
            IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            Plan _entity;
            Plan.SetupCommand _command;

            public override SetupCommand Command => _command;
            public override Plan Entity => _entity;

            public Validator(Plan entity, SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
            {
                _logger = logger;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _entity = entity;
                _command = command;

                #region Validation Rules
                RuleFor(command => command.Name)
                    .NotNull().WithMessage("The 'Name' must be a non-null value")
                    .NotEmpty().WithMessage("The 'Name' must not be empty")
                    .Matches("^[a-zA-Z0-9 _.-]+$")
                    .WithMessage("The 'Name' does not accept special characters")
                    .Length(1, 60)
                    .WithMessage("The 'Name' should be between 1 and 60 characters");
                #endregion

                if (entity.Id != default && command.Id.HasValue && entity.Id != command.Id)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "Plan: Entity Id Can't be Changed, The Id value will be ignored", LPSLoggingLevel.Warning);
                }

                _command.IsValid = base.Validate();
            }
        }
    }
}
