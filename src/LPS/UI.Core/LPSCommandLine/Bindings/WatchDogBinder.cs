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
using LPS.Infrastructure.Watchdog;

namespace LPS.UI.Core.LPSCommandLine.Bindings
{
    public class WatchDogBinder : BinderBase<WatchdogOptions>
    {
        private Option<int?> _maxMemoryMB;
        private Option<int?> _maxCPUPercentage;
        private Option<int?> _coolDownMemoryMB;
        private Option<int?> _coolDownCPUPercentage;
        private Option<int?> _maxConcurrentConnectionsCountPerHostName;
        private Option<int?> _coolDownConcurrentConnectionsCountPerHostName;
        private Option<int?> _coolDownRetryTimeInSeconds;
        private Option<int?> _maxCoolingPeriod;
        private Option<int?> _resumeCoolingAfter;
        private Option<SuspensionMode?> _suspensionMode;



        public WatchDogBinder(
            Option<int?>? maxMemoryMB = null,
            Option<int?>? maxCPUPercentage = null,
            Option<int?>? coolDownMemoryMB = null,
            Option<int?>? coolDownCPUPercentage = null,
            Option<int?>? maxConcurrentConnectionsCountPerHostName = null,
            Option<int?>? coolDownConcurrentConnectionsCountPerHostName = null,
            Option<int?>? coolDownRetryTimeInSeconds = null,
            Option<int?>? maxCoolingPeriod = null,
            Option<int?>? resumeCoolingAfter = null,
            Option<SuspensionMode?>? suspensionMode = null)
        {
            _maxMemoryMB = maxMemoryMB ?? CommandLineOptions.LPSWatchdogCommandOptions.MaxMemoryMB;
            _maxCPUPercentage = maxCPUPercentage ?? CommandLineOptions.LPSWatchdogCommandOptions.MaxCPUPercentage;
            _coolDownMemoryMB = coolDownMemoryMB ?? CommandLineOptions.LPSWatchdogCommandOptions.CoolDownMemoryMB;
            _coolDownCPUPercentage = coolDownCPUPercentage ?? CommandLineOptions.LPSWatchdogCommandOptions.CoolDownCPUPercentage;
            _maxConcurrentConnectionsCountPerHostName = maxConcurrentConnectionsCountPerHostName ?? CommandLineOptions.LPSWatchdogCommandOptions.MaxConcurrentConnectionsCountPerHostName;
            _coolDownConcurrentConnectionsCountPerHostName = coolDownConcurrentConnectionsCountPerHostName ?? CommandLineOptions.LPSWatchdogCommandOptions.CoolDownConcurrentConnectionsCountPerHostName;
            _coolDownRetryTimeInSeconds = coolDownRetryTimeInSeconds ?? CommandLineOptions.LPSWatchdogCommandOptions.CoolDownRetryTimeInSeconds;
            _maxCoolingPeriod = maxCoolingPeriod ?? CommandLineOptions.LPSWatchdogCommandOptions.MaxCoolingPeriod;
            _resumeCoolingAfter = resumeCoolingAfter ?? CommandLineOptions.LPSWatchdogCommandOptions.ResumeCoolingAfter;
            _suspensionMode = suspensionMode ?? CommandLineOptions.LPSWatchdogCommandOptions.SuspensionMode;
        }

        protected override WatchdogOptions GetBoundValue(BindingContext bindingContext) =>
            new WatchdogOptions
            {
                MaxMemoryMB = bindingContext.ParseResult.GetValueForOption(_maxMemoryMB),
                MaxCPUPercentage = bindingContext.ParseResult.GetValueForOption(_maxCPUPercentage),
                MaxConcurrentConnectionsCountPerHostName = bindingContext.ParseResult.GetValueForOption(_maxConcurrentConnectionsCountPerHostName),
                CoolDownMemoryMB = bindingContext.ParseResult.GetValueForOption(_coolDownMemoryMB),
                CoolDownCPUPercentage = bindingContext.ParseResult.GetValueForOption(_coolDownCPUPercentage),
                CoolDownConcurrentConnectionsCountPerHostName = bindingContext.ParseResult.GetValueForOption(_coolDownConcurrentConnectionsCountPerHostName),
                CoolDownRetryTimeInSeconds = bindingContext.ParseResult.GetValueForOption(_coolDownRetryTimeInSeconds),
                SuspensionMode = bindingContext.ParseResult.GetValueForOption(_suspensionMode),
                MaxCoolingPeriod = bindingContext.ParseResult.GetValueForOption(_maxCoolingPeriod),
                ResumeCoolingAfter = bindingContext.ParseResult.GetValueForOption(_resumeCoolingAfter)
            };
    }
}
