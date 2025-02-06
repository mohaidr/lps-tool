using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace LPS.Domain.LPSFlow.LPSHandlers
{
        public partial class CaptureHandler : ISessionHandler
        {
        public class SetupCommand : ICommand<CaptureHandler>, IValidCommand<CaptureHandler>
        {
            public SetupCommand()
            {
                As = string.Empty;
                Regex = string.Empty;
                MakeGlobal = false;
                Headers = [];
                ValidationErrors = new Dictionary<string, List<string>>();
            }

            [JsonIgnore]
            [YamlIgnore]
            public Guid? Id { get; set; }
            public string To { get; set; }
            public string As { get; set; }
            public bool? MakeGlobal { get; set; }
            public string Regex { get; set; }
            public bool IsValid { get; set; }

            public IList<string> Headers { get; set; }

            [JsonIgnore]
            [YamlIgnore]
            public IDictionary<string, List<string>> ValidationErrors { get; set; }

            public void Execute(CaptureHandler entity)
            {
                ArgumentNullException.ThrowIfNull(entity);
                entity?.Setup(this);
            }
        }

        public object Clone()
        {
            CaptureHandler clone = new(_logger, _runtimeOperationIdProvider);
            if (this.IsValid)
            {
                clone.Id = this.Id;
                clone.To = this.To;
                clone.As = this.As;
                clone.Regex = this.Regex;
                clone.MakeGlobal = this.MakeGlobal;
                clone.Headers = this.Headers.ToList();
                clone.IsValid = true;
            }
            return clone;
        }

        public IEnumerable<string> GetReadOnlyHeaders()
        {
            foreach (var header in Headers)
            {
                yield return header;
            }
        }

        protected virtual void Setup(SetupCommand command)
        {
            //TODO: DeepCopy and then send the copy item instead of the original command for further protection 
            var validator = new Validator(this, command, _logger, _runtimeOperationIdProvider);
            if (command.IsValid)
            {
                this.To = command.To;
                this.As = command.As;
                this.MakeGlobal = command.MakeGlobal;
                this.Regex = command.Regex;
                this.Headers = command.Headers.ToList();
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
