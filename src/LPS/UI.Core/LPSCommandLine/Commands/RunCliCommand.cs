using LPS.Domain.Common.Interfaces;
using LPS.UI.Common;
using System.CommandLine;
using static LPS.UI.Core.LPSCommandLine.CommandLineOptions;


namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class RunCliCommand : ICliCommand
    {
        TestRunParameters _args;

        private readonly Command _rootLpsCliCommand;
        private readonly ILogger _logger;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        private readonly ITestOrchestratorService _testOrchestratorService;

        private Command _runCommand;

        public Command Command => _runCommand;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal RunCliCommand(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            Command rootCLICommandLine,
            ILogger logger,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            ITestOrchestratorService testOrchestratorService) // Injected AutoMapper
        {
            _rootLpsCliCommand = rootCLICommandLine;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _testOrchestratorService = testOrchestratorService;
            _logger = logger;
            Setup();
        }

        private void Setup()
        {
            _runCommand = new Command("run", "Run existing test");
            AddOptionsToCommand(_runCommand, typeof(LPSRunCommandOptions));
            _runCommand.AddArgument(LPSRunCommandOptions.ConfigFileArgument);
            _rootLpsCliCommand.AddCommand(_runCommand);
        }


        public void SetHandler(CancellationToken cancellationToken)
        {
            _runCommand.SetHandler(async (string configFile, IList<string> roundNames, IList<string> tags, IList<string> environments) =>
            {
                try
                {
                    var parameters = new TestRunParameters(configFile, roundNames, tags, environments, cancellationToken);
                    await _testOrchestratorService.RunAsync(parameters);

                }
                catch (Exception ex)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"{ex.Message}\r\n{ex.InnerException?.Message}\r\n{ex.StackTrace}", LPSLoggingLevel.Error);
                }
            },
            LPSRunCommandOptions.ConfigFileArgument,
            LPSRunCommandOptions.RoundNameOption,
            LPSRunCommandOptions.TagOption,
            LPSRunCommandOptions.EnvironmentOption);
        }

    }
}
