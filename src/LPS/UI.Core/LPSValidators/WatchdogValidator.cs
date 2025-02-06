using LPS.Domain;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FluentValidation;
using LPS.UI.Common;
using LPS.Infrastructure.Logger;
using System.IO;
using LPS.UI.Common.Options;

namespace LPS.UI.Core.LPSValidators
{
    internal class WatchdogValidator : AbstractValidator<WatchdogOptions>
    {
        public WatchdogValidator()
        {
            RuleFor(watchdog => watchdog.MaxCPUPercentage)
            .NotNull()
            .GreaterThan(0)
            .GreaterThan(watchdog => watchdog.CoolDownCPUPercentage)
            .WithMessage("'Max CPU Percentage' must be greater than the 'Cooldown CPU Percentage'");
            RuleFor(watchdog => watchdog.CoolDownCPUPercentage)
                .NotNull()
                .GreaterThan(0);
            RuleFor(watchdog => watchdog.MaxMemoryMB)
                .NotNull()
                .GreaterThan(0)
                .GreaterThan(watchdog => watchdog.CoolDownMemoryMB)
                .WithMessage("'Max Memory MB' must be greater than the 'Cooldown Memory MB'");
            RuleFor(watchdog => watchdog.CoolDownMemoryMB)
                .NotNull()
                .GreaterThan(0);
            RuleFor(command => command.MaxConcurrentConnectionsCountPerHostName)
                .NotNull()
                .GreaterThan(0)
                .GreaterThan(watchdog => watchdog.CoolDownConcurrentConnectionsCountPerHostName)
                .WithMessage("'Max Connections Count Per Host Name' must be greater than the 'Cooldown Connections Count Per Host Name'");
            RuleFor(command => command.CoolDownConcurrentConnectionsCountPerHostName)
            .NotNull()
            .GreaterThan(0);
            RuleFor(watchdog => watchdog.SuspensionMode)
                .IsInEnum();
            RuleFor(watchdog => watchdog.CoolDownRetryTimeInSeconds)
            .NotNull()
            .GreaterThan(0);
            RuleFor(command => command.MaxCoolingPeriod)
            .NotNull()
            .GreaterThan(0);
            RuleFor(command => command.ResumeCoolingAfter)
            .NotNull()
            .GreaterThan(0);

        }
    }
}
