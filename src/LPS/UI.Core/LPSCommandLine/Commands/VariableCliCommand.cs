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
using System.CommandLine;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class VariableCliCommand : ICliCommand
    {
        private readonly Command _rootCliCommand;
        private Command _variableCommand;
        public Command Command => _variableCommand;
        ILogger _logger;
        IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        IPlaceholderResolverService _placeholderResolverService;
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal VariableCliCommand(Command rootCliCommand, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider, IPlaceholderResolverService placeholderResolverService)
        {
            _rootCliCommand = rootCliCommand;
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _placeholderResolverService = placeholderResolverService;
            Setup();
        }

        private void Setup()
        {
            _variableCommand = new Command("variable", "Add global variable")
            {
                CommandLineOptions.VariableCommandOptions.ConfigFileArgument
            };
            CommandLineOptions.AddOptionsToCommand(_variableCommand, typeof(CommandLineOptions.VariableCommandOptions));
            _rootCliCommand.AddCommand(_variableCommand);
        }

        public void SetHandler(CancellationToken cancellation)
        {
            _variableCommand.SetHandler((configFile, variable, environments) =>
            {
                try
                {
                    // Fetch existing configuration
                    var planDto = ConfigurationService.FetchConfiguration<PlanDto>(configFile, _placeholderResolverService);
                    if (planDto != null)
                    {
                        // Validate the variable using VariableValidator
                        var variableDtoValidator = new VariableValidator();
                        if (!variableDtoValidator.Validate(variable).IsValid)
                        {
                            variableDtoValidator.ValidateAndThrow(variable);
                        }

                        // Initialize EnvironmentValidator
                        var environmentValidator = new EnvironmentValidator();

                        // Handle global variable addition
                        if (!environments.Any())
                        {
                            // Remove existing global variable if it exists
                            var existingGlobalVariable = planDto.Variables
                                .FirstOrDefault(v => v.Name.Equals(variable.Name, StringComparison.OrdinalIgnoreCase));
                            if (existingGlobalVariable != null)
                            {
                                planDto.Variables.Remove(existingGlobalVariable);
                            }

                            // Add the variable as a global variable
                            planDto.Variables.Add(variable);
                        }
                        else
                        {
                            // Handle environment-specific variables
                            foreach (var environmentName in environments)
                            {
                                var environment = planDto.Environments
                                    .FirstOrDefault(env => env.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase));

                                if (environment == null)
                                {
                                    // Create new environment if it does not exist
                                    environment = new EnvironmentDto
                                    {
                                        Name = environmentName,
                                        Variables = new List<VariableDto>()
                                    };

                                    // Validate the new environment
                                    environmentValidator.ValidateAndThrow(environment);

                                    planDto.Environments.Add(environment);
                                }

                                // Remove existing variable in the environment if it exists
                                var existingVariable = environment.Variables
                                    .FirstOrDefault(v => v.Name.Equals(variable.Name, StringComparison.OrdinalIgnoreCase));
                                if (existingVariable != null)
                                {
                                    environment.Variables.Remove(existingVariable);
                                }

                                // Add the variable to the environment
                                environment.Variables.Add(variable);

                                // Revalidate the environment after modification
                                environmentValidator.ValidateAndThrow(environment);
                            }
                        }

                        // Save updated configuration
                        ConfigurationService.SaveConfiguration(configFile, planDto);
                    }
                    else
                    {
                        _logger.Log(_runtimeOperationIdProvider.OperationId, "No plan defined for adding the variable to.", LPSLoggingLevel.Error);
                    }
                }
                catch (FluentValidation.ValidationException ex)
                {
                    // Handle FluentValidation-specific errors
                    foreach (var error in ex.Errors)
                    {
                        _logger.Log(_runtimeOperationIdProvider.OperationId, error.ErrorMessage, LPSLoggingLevel.Error);
                    }
                }
                catch (Exception ex)
                {
                    // Handle other errors
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"{ex.Message}\r\n{ex.InnerException?.Message}\r\n{ex.StackTrace}", LPSLoggingLevel.Error);
                }
            },
            CommandLineOptions.VariableCommandOptions.ConfigFileArgument,
            new VariableBinder(),
            CommandLineOptions.VariableCommandOptions.EnvironmentOption);
        }
    }
}
