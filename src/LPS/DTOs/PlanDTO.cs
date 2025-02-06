using LPS.Domain;

namespace LPS.DTOs
{
    public class PlanDto: IDto<PlanDto>
    {
        public PlanDto()
        {
            Iterations = [];
            Rounds = [];
            Variables = [] ;
            Environments = [] ;
        }

        // Name of the plan
        public string Name { get; set; }

        // List of rounds in the plan
        public IList<RoundDto> Rounds { get; set; }

        // List of variables in the plan
        public IList<VariableDto> Variables { get; set; }

        // List of environments in the plan
        public IList<EnvironmentDto> Environments { get; set; }

        // Inline iterations for the plan
        public IList<HttpIterationDto> Iterations { get; set; }

        // Deep copy method to create a new instance with the same data
        public void DeepCopy(out PlanDto targetDto)
        {
            #pragma warning disable CS8601 // Possible null reference assignment.
            targetDto = new PlanDto
            {
                Name = this.Name,
                Iterations = this.Iterations?.Select(iteration =>
                {
                    var copiedIteration = new HttpIterationDto();
                    iteration.DeepCopy(out copiedIteration);
                    return copiedIteration;
                }).ToList(),
                Rounds = this.Rounds?.Select(round =>
                {
                    var copiedRound = new RoundDto();
                    round.DeepCopy(out copiedRound);
                    return copiedRound;
                }).ToList(),
                Variables = this.Variables?.Select(variable =>
                {
                    var copiedVariable = new VariableDto();
                    variable.DeepCopy(out copiedVariable);
                    return copiedVariable;
                }).ToList(),
                Environments = this.Environments?.Select(environment =>
                {
                    var copiedEnvironment = new EnvironmentDto();
                    environment.DeepCopy(out copiedEnvironment);
                    return copiedEnvironment;
                }).ToList()
            };
            #pragma warning restore CS8601 // Possible null reference assignment.
        }
    }
}
