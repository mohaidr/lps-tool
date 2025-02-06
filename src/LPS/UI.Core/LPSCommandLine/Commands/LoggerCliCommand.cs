using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Logger;
using LPS.UI.Common;
using LPS.UI.Common.Extensions;
using LPS.UI.Common.Options;
using LPS.UI.Core.LPSCommandLine.Bindings;
using LPS.UI.Core.LPSValidators;
using LPS.UI.Core.Build.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class LoggerCliCommand : ICliCommand
    {
        private Command _rootLpsCliCommand;
        private Command _loggerCliCommand;
        public Command Command => _loggerCliCommand;
        IWritableOptions<FileLoggerOptions> _loggerOptions;
        ILogger _logger;
        IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        public LoggerCliCommand(Command rootLpsCliCommand, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider, IWritableOptions<FileLoggerOptions> loggerOptions) 
        {
            _rootLpsCliCommand = rootLpsCliCommand;
            _loggerOptions = loggerOptions;
            _logger = logger;
           _runtimeOperationIdProvider= runtimeOperationIdProvider;
            Setup();
        }
        private void Setup()
        {
            _loggerCliCommand = new Command("logger", "Configure the LPS logger");
            CommandLineOptions.AddOptionsToCommand(_loggerCliCommand, typeof(CommandLineOptions.LPSLoggerCommandOptions));
            _rootLpsCliCommand.AddCommand(_loggerCliCommand);
        }

        public void SetHandler(CancellationToken cancellationToken)
        {

            _loggerCliCommand.SetHandler((updateLoggerOptions) =>
            {
                var loggerValidator = new FileLoggerValidator();
                FileLoggerOptions fileLoggerOptions = new()
                {
                    // Combine the provided logger options by the command and what in the config section to validate the final object
                    LogFilePath = !string.IsNullOrWhiteSpace(updateLoggerOptions.LogFilePath) ? updateLoggerOptions.LogFilePath : _loggerOptions.Value.LogFilePath,
                    DisableFileLogging = updateLoggerOptions.DisableFileLogging ?? _loggerOptions.Value.DisableFileLogging,
                    LoggingLevel = updateLoggerOptions.LoggingLevel ?? _loggerOptions.Value.LoggingLevel,
                    ConsoleLogingLevel = updateLoggerOptions.ConsoleLogingLevel ?? _loggerOptions.Value.ConsoleLogingLevel,
                    EnableConsoleLogging = updateLoggerOptions.EnableConsoleLogging ?? _loggerOptions.Value.EnableConsoleLogging,
                    DisableConsoleErrorLogging = updateLoggerOptions.DisableConsoleErrorLogging ?? _loggerOptions.Value.DisableConsoleErrorLogging
                };
                var validationResults = loggerValidator.Validate(fileLoggerOptions);

                if (!validationResults.IsValid)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "You must update the below properties to have a valid logger configuration. Updating the LPSAppSettings:LPSFileLoggerConfiguration section with the provided arguements will create an invalid logger configuration. You may run 'lps logger -h' to explore the options", LPSLoggingLevel.Warning);
                    validationResults.PrintValidationErrors();
                }
                else
                {
                    _loggerOptions.Update(option =>
                    {
                        option.LogFilePath = fileLoggerOptions.LogFilePath;
                        option.DisableFileLogging = fileLoggerOptions.DisableFileLogging;
                        option.LoggingLevel = fileLoggerOptions.LoggingLevel;
                        option.ConsoleLogingLevel = fileLoggerOptions.ConsoleLogingLevel;
                        option.EnableConsoleLogging = fileLoggerOptions.EnableConsoleLogging;
                        option.DisableConsoleErrorLogging = fileLoggerOptions.DisableConsoleErrorLogging;
                    });
                }
            }, new LoggerBinder());
        }
    }
}
