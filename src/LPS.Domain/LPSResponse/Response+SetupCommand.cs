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

    public partial class Response
    {
        public class SetupCommand : ICommand<Response>, IValidCommand<Response>
        {
            public SetupCommand()
            {
                ValidationErrors = new Dictionary<string, List<string>>();
            }
            public Guid? Id { get; set; }

            public bool IsValid { get; set; }
            public IDictionary<string, List<string>> ValidationErrors { get; set; }

            public void Execute(Response entity)
            {
                ArgumentNullException.ThrowIfNull(entity);
                entity?.Setup(this);
            }
            public SetupCommand Clone()
            {
                return new SetupCommand
                {
                    Id = this.Id,
                    IsValid = this.IsValid,
                    ValidationErrors = this.ValidationErrors.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value))
                };
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
