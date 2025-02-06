using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Infrastructure.Common;
using LPS.Infrastructure.Monitoring;
using LPS.UI.Common;
using LPS.UI.Common.Options;
using Microsoft.Extensions.Options;
using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using LPS.UI.Core.Services;
using static LPS.UI.Core.LPSCommandLine.CommandLineOptions;
using LPS.UI.Core.LPSValidators;
using FluentValidation.Results;
using LPS.UI.Common.Extensions;
using LPS.DTOs;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class CreateCliCommand : ICliCommand
    {
        readonly Command _rootCliCommand;
        readonly ILogger _logger;
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        IPlaceholderResolverService _placeholderResolverService;
        Command _createCommand;
        public Command Command => _createCommand;
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal CreateCliCommand(
            Command rootCLICommandLine,
            ILogger logger,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            IPlaceholderResolverService placeholderResolverService)
        {
            _rootCliCommand = rootCLICommandLine;
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _placeholderResolverService = placeholderResolverService;
            Setup();
        }

        private void Setup()
        {
            _createCommand = new Command("create", "Run existing test");

            // Add the positional argument directly to _runCommand
            _createCommand.AddArgument(LPSCreateCommandOptions.ConfigFileArgument);
            CommandLineOptions.AddOptionsToCommand(_createCommand, typeof(LPSCreateCommandOptions));

            _rootCliCommand.AddCommand(_createCommand);
        }

        public void SetHandler(CancellationToken cancellationToken)
        {
            _createCommand.SetHandler((string configFile, string name) =>
            {
                try
                {
                    PlanDto planDto = new() { Name = name };
                    ValidationResult results;
                    var planValidator = new PlanValidator(planDto);
                    results = planValidator.Validate();
                    if (!results.IsValid)
                    {
                        results.PrintValidationErrors();
                    }
                    else
                    {
                        if (File.Exists(configFile))
                        {
                            _logger.Log(_runtimeOperationIdProvider.OperationId, $"{configFile} File exists, fetching configuration.", LPSLoggingLevel.Information);
                            planDto = ConfigurationService.FetchConfiguration<PlanDto>(configFile, _placeholderResolverService) ?? new PlanDto() { Name = name };
                            planDto.Name = name;
                        }
                        else
                        {
                            _logger.Log(_runtimeOperationIdProvider.OperationId, $"{configFile} File does not exist, creating new plan.", LPSLoggingLevel.Information);
                            planDto = new PlanDto { Name = name };
                        }

                        ConfigurationService.SaveConfiguration(configFile, planDto);

                        _logger.Log(_runtimeOperationIdProvider.OperationId, $"Configuration file '{configFile}' updated with plan name '{name}'.", LPSLoggingLevel.Information);

                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"{ex.Message}\r\n{ex.InnerException?.Message}\r\n{ex.StackTrace}", LPSLoggingLevel.Error);
                }
            },
            LPSCreateCommandOptions.ConfigFileArgument,
            LPSCreateCommandOptions.PlanNameOption);
        }
    }
}
