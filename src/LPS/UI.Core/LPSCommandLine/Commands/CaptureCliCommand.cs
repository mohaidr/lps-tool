using FluentValidation;
using FluentValidation.Results;
using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.DTOs;
using LPS.Infrastructure.Common;
using LPS.Infrastructure.LPSClients.PlaceHolderService;
using LPS.UI.Common;
using LPS.UI.Common.Extensions;
using LPS.UI.Core.LPSCommandLine.Bindings;
using LPS.UI.Core.LPSValidators;
using LPS.UI.Core.Services;
using System.CommandLine;
using static LPS.UI.Core.LPSCommandLine.CommandLineOptions;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class CaptureCliCommand : ICliCommand
    {
        private readonly Command _rootCliCommand;
        private Command _captureCommand;
        public Command Command => _captureCommand;
        ILogger _logger;
        IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        IPlaceholderResolverService _placeholderResolverService;
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal CaptureCliCommand(Command rootCliCommand, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider, IPlaceholderResolverService placeholderResolverService)
        {
            _rootCliCommand = rootCliCommand;
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _placeholderResolverService = placeholderResolverService;
            Setup();
        }

        private void Setup()
        {
            _captureCommand = new Command("capture", "Capture response and store it in a variable")
            {
                CaptureCommandOptions.ConfigFileArgument
            };
            CommandLineOptions.AddOptionsToCommand(_captureCommand, typeof(CommandLineOptions.CaptureCommandOptions));
            _rootCliCommand.AddCommand(_captureCommand);
        }

        public void SetHandler(CancellationToken cancellation)
        {
            _captureCommand.SetHandler((configFile, roundName, iterationName, captureDto) =>
            {
                try
                {
                    var plandto = ConfigurationService.FetchConfiguration<PlanDto>(configFile, _placeholderResolverService);
                    if (plandto != null)
                    {
                        var variableDtoValidator = new CaptureValidator(captureDto);

                        var selectedRound = plandto.Rounds
                            .FirstOrDefault(round => round.Name.Equals(roundName, StringComparison.OrdinalIgnoreCase));

                        var selectedIteration = selectedRound?.Iterations
                            .FirstOrDefault(iteration => iteration.Name.Equals(iterationName, StringComparison.OrdinalIgnoreCase))
                            ?? plandto.Iterations
                            .FirstOrDefault(iteration => iteration.Name.Equals(iterationName, StringComparison.OrdinalIgnoreCase));

                        if (selectedIteration != null)
                            {
                                if (variableDtoValidator.Validate(captureDto).IsValid)
                                {
                                    if (selectedIteration.HttpRequest != null)
                                    {
                                        selectedIteration.HttpRequest.Capture = captureDto;
                                    }
                                    else
                                    {
                                        _logger.Log(_runtimeOperationIdProvider.OperationId, $"Http Request is undefined for the iteration {iterationName}", LPSLoggingLevel.Warning);
                                    }
                                }
                                else
                                {
                                    variableDtoValidator.ValidateAndThrow(captureDto);
                                    return;
                                }
                            }
                            else {
                                _logger.Log(_runtimeOperationIdProvider.OperationId, $"The iteration {iterationName} is unefined", LPSLoggingLevel.Warning);
                            }
                    }
                    else
                    {
                        _logger.Log(_runtimeOperationIdProvider.OperationId, "No plan defined for adding the variable to.", LPSLoggingLevel.Error);
                    }
                    ConfigurationService.SaveConfiguration(configFile, plandto);
                }
                catch(Exception ex) {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"{ex.Message}\r\n{ex.InnerException?.Message}\r\n{ex.StackTrace}", LPSLoggingLevel.Error);

                }
            },
            CaptureCommandOptions.ConfigFileArgument,
            CaptureCommandOptions.RoundNameOption, 
            CaptureCommandOptions.IterationNameOption,
            new CaptureBinder());
        }
    }
}
