using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.DTOs;
using LPS.Infrastructure.Common;
using LPS.UI.Common;
using LPS.UI.Common.Extensions;
using LPS.UI.Core.LPSCommandLine.Bindings;
using LPS.UI.Core.LPSValidators;
using LPS.UI.Core.Services;
using System.CommandLine;
using System.Xml.Linq;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class RoundCliCommand : ICliCommand
    {
        private Command _rootCliCommand;
        private Command _roundCommand;
        public Command Command => _roundCommand;
        ILogger _logger;
        IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        IPlaceholderResolverService _placeholderResolverService;
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal RoundCliCommand(Command rootLpsCliCommand, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider, IPlaceholderResolverService placeholderResolverService)
        {
            _rootCliCommand = rootLpsCliCommand;
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _placeholderResolverService = placeholderResolverService;
            Setup();
        }

        private void Setup()
        {
            _roundCommand = new Command("round", "Create a new test")
            {
                CommandLineOptions.LPSRoundCommandOptions.ConfigFileArgument // Add ConfigFileArgument here
            };
            CommandLineOptions.AddOptionsToCommand(_roundCommand, typeof(CommandLineOptions.LPSRoundCommandOptions));
            _rootCliCommand.AddCommand(_roundCommand);
        }

        public void SetHandler(CancellationToken cancellationToken)
        {
            _roundCommand.SetHandler((configFile, round) =>
            {
                try
                {
                    ValidationResult results;
                    var roundValidator = new RoundValidator(round);
                    results = roundValidator.Validate();
                    if (!results.IsValid)
                    {
                        results.PrintValidationErrors();
                    }
                    else
                    {
                        PlanDto planDto = ConfigurationService.FetchConfiguration<PlanDto>(configFile, _placeholderResolverService) ?? new PlanDto() { Name = "Default" };
                        var selectedRound = planDto?.Rounds.FirstOrDefault(r => r.Name.Equals(round.Name, StringComparison.OrdinalIgnoreCase));
                        if (selectedRound != null)
                        {
                            selectedRound.Name = round.Name;
                            selectedRound.BaseUrl = round.BaseUrl;
                            selectedRound.StartupDelay = round.StartupDelay;
                            selectedRound.NumberOfClients = round.NumberOfClients;
                            selectedRound.ArrivalDelay = round.ArrivalDelay;
                            selectedRound.DelayClientCreationUntilIsNeeded = round.DelayClientCreationUntilIsNeeded;
                            selectedRound.RunInParallel = round.RunInParallel;
                            selectedRound.Tags = round.Tags;
                        }
                        else
                        {
                            planDto?.Rounds.Add(round);
                        }
                        ConfigurationService.SaveConfiguration(configFile, planDto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"{ex.Message}\r\n{ex.InnerException?.Message}\r\n{ex.StackTrace}", LPSLoggingLevel.Error);
                }
            },
            CommandLineOptions.LPSRoundCommandOptions.ConfigFileArgument,
            new RoundCommandBinder());
        }
    }
}
