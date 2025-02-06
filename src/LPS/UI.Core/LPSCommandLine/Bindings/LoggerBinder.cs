using LPS.Domain;
using LPS.UI.Core.Build.Services;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.CommandLine.Parsing;
using LPS.UI.Common.Options;
using LPS.Domain.Common.Interfaces;

namespace LPS.UI.Core.LPSCommandLine.Bindings
{
    public class LoggerBinder : BinderBase<FileLoggerOptions>
    {


        private Option<string> _logFilePathOption;
        private Option<bool?> _enableConsoleLoggingOption;
        private Option<bool?> _disableConsoleErrorLoggingOption;
        private Option<bool?> _disableFileLoggingOption;
        private Option<LPSLoggingLevel?> _loggingLevelOption;
        private Option<LPSLoggingLevel?> _consoleLoggingLevelOption;


        public LoggerBinder(
            Option<string>? logFilePathOption = null,
            Option<bool?>? disableFileLoggingOption = null,
            Option<bool?>? enableConsoleLoggingOption = null,
            Option<bool?>? disableConsoleErrorLoggingOption = null,
            Option<LPSLoggingLevel?>? loggingLevelOption = null,
            Option<LPSLoggingLevel?>? consoleLoggingLevelOption = null)
        {
            _logFilePathOption = logFilePathOption ?? CommandLineOptions.LPSLoggerCommandOptions.LogFilePathOption;
            _enableConsoleLoggingOption = enableConsoleLoggingOption ?? CommandLineOptions.LPSLoggerCommandOptions.EnableConsoleLoggingOption;
            _disableConsoleErrorLoggingOption = disableConsoleErrorLoggingOption ?? CommandLineOptions.LPSLoggerCommandOptions.DisableConsoleErrorLoggingOption;
            _disableFileLoggingOption = disableFileLoggingOption ?? CommandLineOptions.LPSLoggerCommandOptions.DisableFileLoggingOption;
            _loggingLevelOption = loggingLevelOption ?? CommandLineOptions.LPSLoggerCommandOptions.LoggingLevelOption;
            _consoleLoggingLevelOption = consoleLoggingLevelOption ?? CommandLineOptions.LPSLoggerCommandOptions.ConsoleLoggingLevelOption;
        }

        protected override FileLoggerOptions GetBoundValue(BindingContext bindingContext) =>
            new FileLoggerOptions
            {
                LogFilePath = bindingContext.ParseResult.GetValueForOption(_logFilePathOption),
                EnableConsoleLogging = bindingContext.ParseResult.GetValueForOption(_enableConsoleLoggingOption),
                DisableConsoleErrorLogging = bindingContext.ParseResult.GetValueForOption(_disableConsoleErrorLoggingOption),
                DisableFileLogging = bindingContext.ParseResult.GetValueForOption(_disableFileLoggingOption),
                LoggingLevel = bindingContext.ParseResult.GetValueForOption(_loggingLevelOption),
                ConsoleLogingLevel = bindingContext.ParseResult.GetValueForOption(_consoleLoggingLevelOption),
            };
    }
}
