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
    public partial class ReadHandler : ISessionHandler
    {
        public class Validator : CommandBaseValidator<ReadHandler, ReadHandler.SetupCommand>
        {
            ILogger _logger;
            IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            ReadHandler _entity;
            SetupCommand _command;

            public Validator(ReadHandler entity, SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
            {
                _logger = logger;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _entity = entity;
                _command = command;
                if (entity.Id != default && command.Id.HasValue && entity.Id != command.Id)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "LPS ReadHandler: Entity Id Can't be Changed, The Id value will be ignored", LPSLoggingLevel.Warning);
                }

                #region Validation Rules
                // No validation rules so far
                #endregion
                _command.IsValid = base.Validate();
            }

            public override SetupCommand Command => _command;
            public override ReadHandler Entity => _entity;


        }

    }
}
