using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Validation;
using System;
using System.Text.RegularExpressions;

namespace LPS.Domain
{

    public partial class Iteration
    {
   
        public class Validator: CommandBaseValidator<Iteration, Iteration.SetupCommand>
        {
            ILogger _logger;
            IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            Iteration _entity;
            SetupCommand _command;
            public Validator(Iteration entity, SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
            {
                _logger = logger;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _entity = entity;
                _command = command;

                #region Validation Rules
                    // No validation rules so far
                #endregion

                if (entity.Id != default && command.Id.HasValue && entity.Id != command.Id)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "LPS Run: Entity Id Can't be Changed, The Id value will be ignored", LPSLoggingLevel.Warning);
                }
                _command.IsValid = true;
            }

            public override SetupCommand Command => _command;

            public override Iteration Entity => _entity;

        }
    }
}

