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
using LPS.Domain.Common.Interfaces;

namespace LPS.Domain
{

    public partial class Iteration
    {
        public class SetupCommand : ICommand<Iteration>
        {

            public SetupCommand()
            {
            }

            public void Execute(Iteration entity)
            {
                ArgumentNullException.ThrowIfNull(entity);
                entity?.Setup(this);
            }
            public Guid? Id { get; set; }

            public bool IsValid { get; set; }
            public string Name { get; set; }

        }

        protected void Setup(SetupCommand command)
        {
            //TODO: DeepCopy and then send the copy item instead of the original command for further protection 
            var validator = new Validator(this, command, _logger, _runtimeOperationIdProvider);
            if (command.IsValid)
            {
                this.Name= command.Name;
                this.IsValid= true;
            }
            else
            {
                this.IsValid = false;
                validator.PrintValidationErrors();
            }
        }
    }
}
