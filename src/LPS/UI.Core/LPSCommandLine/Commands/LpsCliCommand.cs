using LPS.Domain.Common.Interfaces;
using LPS.Domain;
using LPS.UI.Common;
using LPS.UI.Common.Options;
using LPS.UI.Core.LPSCommandLine.Bindings;
using FluentValidation.Results;
using LPS.UI.Common.Extensions;
using LPS.UI.Core.LPSValidators;
using Microsoft.Extensions.Options;
using FluentValidation;
using LPS.UI.Core.Services;
using LPS.DTOs;
using AutoMapper;
using LPS.Domain.Domain.Common.Interfaces;
using System.CommandLine;
using LPS.Infrastructure.Nodes;
using LPS.Common.Interfaces;
using Grpc.Net.Client;
using LPS.Common.Services;
using LPS.Protos.Shared;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class LpsCliCommand : ICliCommand
    {
        private readonly ITestOrchestratorService _testOrchestratorService;
        private readonly ILogger _logger;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;


        private readonly Command _rootCliCommand;
        public Command Command => _rootCliCommand;

        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public LpsCliCommand(
            Command rootCliCommand,
            ILogger logger,
            IRuntimeOperationIdProvider runtimeOperationIdProvider, 
            ITestOrchestratorService testOrchestratorService)
        {
            _rootCliCommand = rootCliCommand;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _testOrchestratorService = testOrchestratorService;
            _logger = logger;
            Setup();
        }

        private void Setup()
        {
            CommandLineOptions.AddOptionsToCommand(_rootCliCommand, typeof(CommandLineOptions.LPSCommandOptions));
        }

        public void SetHandler(CancellationToken cancellationToken)
        {
            _rootCliCommand.SetHandler(async (PlanDto planDto, bool save) =>
            {
                try
                {
                    var parameters = new TestRunParameters(planDto, cancellationToken);
                    await _testOrchestratorService.RunAsync(parameters);
                }
                catch (Exception ex)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"{ex.Message}\r\n{ex.InnerException?.Message}\r\n{ex.StackTrace}", LPSLoggingLevel.Error);
                }
            },
            new CommandBinder(),
            CommandLineOptions.LPSCommandOptions.SaveOption);
        }
    }
}
