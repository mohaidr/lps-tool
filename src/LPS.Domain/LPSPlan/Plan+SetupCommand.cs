using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Exceptions;
using LPS.Domain.Domain.Common.Interfaces;
using YamlDotNet.Serialization;

namespace LPS.Domain
{
    public partial class Plan : IAggregateRoot, IValidEntity, IDomainEntity, IBusinessEntity
    {
        public class SetupCommand : ICommand<Plan>, IValidCommand<Plan>
        {
            public SetupCommand()
            {
                ValidationErrors = new Dictionary<string, List<string>>();
            }

            public virtual string Name { get; set; }

            [JsonIgnore]
            [YamlIgnore]
            public Guid? Id { get; set; }
            [JsonIgnore]
            [YamlIgnore]
            public bool IsValid { get; set; }
            [JsonIgnore]
            [YamlIgnore]
            public IDictionary<string, List<string>> ValidationErrors { get; set; }

            public void Execute(Plan entity)
            {
                ArgumentNullException.ThrowIfNull(entity);
                entity?.Setup(this);
            }

            public void Copy(SetupCommand targetCommand)
            {
                targetCommand.Id = this.Id;
                targetCommand.Name = this.Name;
                targetCommand.IsValid = this.IsValid;
                targetCommand.ValidationErrors = this.ValidationErrors.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value));
            }
        }
        public void AddRound(Round round)
        {
            if (round != null && round.IsValid)
            {
                Rounds.Add(round);
            }
            else
            {
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"The referenced LPS Entity of type {typeof(Round)} is either null or invalid.", LPSLoggingLevel.Error);
                throw new InvalidLPSEntityException($"The referenced LPS Entity of type {typeof(Round)} is either null or invalid.");
            }
        }

        //TODO:Will throw exception if enumrating while modifying it,
        //TODO:will need to implement IsEnumerating flag to handle this approach
        //or change the whole approach
        //TODO: This will matter when having DB and repos
        public IEnumerable<Round> GetReadOnlyRounds()
        {
            foreach (var round in Rounds)
            {
                yield return round;
            }
        }


        private void Setup(SetupCommand command)
        {
            //TODO: DeepCopy and then send the copy item instead of the original command for further protection 
            var validator = new Validator(this, command, _logger, _runtimeOperationIdProvider);
            if (command.IsValid)
            {

                this.Name =  command.Name;
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
