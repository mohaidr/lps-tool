using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Exceptions;
using LPS.Domain.LPSFlow.LPSHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace LPS.Domain
{

    public partial class Round
    {
        public class SetupCommand : ICommand<Round>, IValidCommand<Round>
        {
            public SetupCommand()
            {
                Tags = [];
                DelayClientCreationUntilIsNeeded = false;
                RunInParallel = false;
                ValidationErrors = new Dictionary<string, List<string>>();
            }
            public void Execute(Round entity)
            {
                ArgumentNullException.ThrowIfNull(entity);
                entity.Setup(this);
            }
            [JsonIgnore]
            [YamlIgnore]
            public Guid? Id { get; set; }
            public virtual string Name { get; set; }
            public int StartupDelay { get; set; }
            public int? NumberOfClients { get; set; }
            public int? ArrivalDelay { get; set; }
            public bool? DelayClientCreationUntilIsNeeded { get; set; }
            public bool? RunInParallel { get; set; }
            public IList<string> Tags { get; set; } // TODO: Make domain level

            [JsonIgnore]
            [YamlIgnore]
            public bool IsValid { get; set; }
            [JsonIgnore]
            [YamlIgnore]
            public IDictionary<string, List<string>> ValidationErrors { get; set; }

            public void Copy(SetupCommand targetCommand)
            {
                targetCommand.Id = this.Id;
                targetCommand.Name = this.Name;
                targetCommand.StartupDelay = this.StartupDelay;
                targetCommand.NumberOfClients = this.NumberOfClients;
                targetCommand.ArrivalDelay = this.ArrivalDelay;
                targetCommand.DelayClientCreationUntilIsNeeded = this.DelayClientCreationUntilIsNeeded;
                targetCommand.RunInParallel = this.RunInParallel;
                targetCommand.IsValid = this.IsValid;
                targetCommand.ValidationErrors = this.ValidationErrors.ToDictionary(entry => entry.Key, entry => new List<string>(entry.Value));
            }
        }

        public void AddIteration(HttpIteration iteration)
        {
            string roundName = this.Name ?? string.Empty;

            if (iteration != null && iteration.IsValid)
            {
                if (iteration.HttpRequest?.Capture != null && iteration.HttpRequest.Capture.MakeGlobal == false && this.RunInParallel == true)
                {
                    throw new NotSupportedException("The 'Capture' capability is not supported in parallel mode unless MakeGlobal is set to true for use in subsequent rounds. In parallel mode, capturing a variable in one iteration and reusing it in another iteration within the same round can lead to unexpected behavior.");  
                }

                Iterations.Add(iteration);
            }
            else {
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"In the Round '{roundName}', the referenced LPS Entity of type {typeof(HttpIteration)} is either null or invalid.", LPSLoggingLevel.Error);

                throw new InvalidLPSEntityException($"In the Round '{roundName}', the referenced LPS Entity of type {typeof(HttpIteration)} is either null or invalid.");
            }
        }

        //TODO:Will throw exception if enumrating while modifying it,
        //TODO:will need to implement IsEnumerating flag to handle this approach
        //or change the whole approach
        //TODO: This will matter when having DB and repos
        public IEnumerable<Iteration> GetReadOnlyIterations()
        {
            foreach (var iteration in Iterations)
            {
                yield return iteration;
            }
        }

        public IEnumerable<string> GetReadOnlyTags()
        {
            foreach (var tag in Tags)
            {
                yield return tag;
            }
        }

        private void Setup(SetupCommand command)
        {
            //TODO: DeepCopy and then send the copy item instead of the original command for further protection 
            var validator = new Validator(this, command, _logger, _runtimeOperationIdProvider);
            if (command.IsValid)
            {

                this.Name = command.Name;
                this.StartupDelay = command.StartupDelay;
                this.NumberOfClients = command.NumberOfClients.Value;
                this.ArrivalDelay = command.ArrivalDelay;
                this.DelayClientCreationUntilIsNeeded = command.DelayClientCreationUntilIsNeeded;
                this.IsValid = true;
                this.RunInParallel = command.RunInParallel;
                this.Tags = command.Tags;
            }
            else
            {
                this.IsValid = false;
                validator.PrintValidationErrors();
            }
        }
    }
}
