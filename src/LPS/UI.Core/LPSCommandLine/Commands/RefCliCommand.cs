using FluentValidation;
using FluentValidation.Results;
using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.DTOs;
using LPS.Infrastructure.Common;
using LPS.UI.Common;
using LPS.UI.Common.Extensions;
using LPS.UI.Core.LPSCommandLine.Bindings;
using LPS.UI.Core.LPSValidators;
using LPS.UI.Core.Services;
using Spectre.Console;
using System;
using System.CommandLine;
using System.Runtime.CompilerServices;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class RefCliCommand : ICliCommand
    {
        private readonly Command _rootCliCommand;
        private Command _refCommand;
        public Command Command => _refCommand;
        ILogger _logger;
        IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        IPlaceholderResolverService _placeholderResolverService;
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal RefCliCommand(Command rootCliCommand, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider, IPlaceholderResolverService placeholderResolverService)
        {
            _rootCliCommand = rootCliCommand;
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _placeholderResolverService = placeholderResolverService;
            Setup();
        }

        private void Setup()
        {
            _refCommand = new Command("ref", "Reference a global http iteration")
            {
                CommandLineOptions.RefCommandOptions.ConfigFileArgument
            };
            CommandLineOptions.AddOptionsToCommand(_refCommand, typeof(CommandLineOptions.RefCommandOptions));
            _rootCliCommand.AddCommand(_refCommand);
        }

        public void SetHandler(CancellationToken cancellation)
        {
            _refCommand.SetHandler((configFile, roundName, iterationName) =>
            {
                try
                {
                    var plandto = ConfigurationService.FetchConfiguration<PlanDto>(configFile, _placeholderResolverService);
                    var globalIteration = plandto?.Iterations.FirstOrDefault(iteration => iteration.Name.Equals(iterationName, StringComparison.OrdinalIgnoreCase));
                    if (globalIteration != null)
                    {
                        var round = plandto?.Rounds.FirstOrDefault(r => r.Name.Equals(roundName, StringComparison.OrdinalIgnoreCase));
                        if (round !=null)
                        {
                            var iterationValidator = new IterationValidator(globalIteration);
                            if (iterationValidator.Validate(nameof(globalIteration.Name)))
                            {
                                round?.ReferencedIterations.Add(globalIteration.Name);
                            }
                            else
                            {
                                _logger.Log(_runtimeOperationIdProvider.OperationId, $"Invalid Iteration name {globalIteration.Name}", LPSLoggingLevel.Error);
                                iterationValidator.PrintValidationErrors(nameof(globalIteration.Name));
                            }
                        }
                        else {
                            _logger.Log(_runtimeOperationIdProvider.OperationId, $"Round '{roundName}' does not exist", LPSLoggingLevel.Error);
                        }
                    }
                    else {
                        _logger.Log(_runtimeOperationIdProvider.OperationId, $"Global iteration '{iterationName}' does not exist", LPSLoggingLevel.Error);
                    }
                    ConfigurationService.SaveConfiguration(configFile, plandto);
                }
                catch(Exception ex) {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"{ex.Message}\r\n{ex.InnerException?.Message}\r\n{ex.StackTrace}", LPSLoggingLevel.Error);

                }
            },
            CommandLineOptions.RefCommandOptions.ConfigFileArgument,
            CommandLineOptions.RefCommandOptions.RoundNameOption,
            CommandLineOptions.RefCommandOptions.IterationNameOption);
        }
    }
}
