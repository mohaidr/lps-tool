using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.LPSFlow.LPSHandlers
{
    public partial class PauseHandler : ISessionHandler
    {
        public class SetupCommand : ICommand<PauseHandler>, IValidCommand<PauseHandler>
        {
            public SetupCommand()
            {
                ValidationErrors = new Dictionary<string, List<string>>();

            }
            public Guid? Id { get; set; }
            public bool IsValid { get; set; }
            public IDictionary<string, List<string>> ValidationErrors { get; set; }

            public void Execute(PauseHandler entity)
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                entity?.Setup(this);
            }
        }

        protected virtual void Setup(SetupCommand command)
        {
            //TODO: DeepCopy and then send the copy item instead of the original command for further protection 
            var validator = new Validator(this, command, _logger, _runtimeOperationIdProvider);
            if (command.IsValid)
            {

                this.IsValid = true;
            }
            else
            {
                this.IsValid = false;
                validator.PrintValidationErrors();
            }
        }
    }
}
