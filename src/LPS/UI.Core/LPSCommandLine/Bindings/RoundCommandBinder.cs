using LPS.Domain;
using LPS.UI.Core.Build.Services;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.CommandLine.Parsing;
using LPS.DTOs;

namespace LPS.UI.Core.LPSCommandLine.Bindings
{
    public class RoundCommandBinder(
        Option<string>? roundNameOption = null,
        Option<string>? baseUrlOption = null,
        Option<string>? startupDelayOption = null,
        Option<string>? numberOfClientsOption = null,
        Option<string>? arrivalDelayOption = null,
        Option<string>? delayClientCreationOption = null,
        Option<string?>? runInParallerOption = null,
        Option<IList<string>>? tagOption = null) : BinderBase<RoundDto>
    {
        private readonly Option<string> _roundNameOption = roundNameOption ?? CommandLineOptions.LPSRoundCommandOptions.RoundNameOption;
        private readonly Option<string> _baseUrlOption = baseUrlOption ?? CommandLineOptions.LPSRoundCommandOptions.BaseUrlOption;
        private readonly Option<string> _startupDelayOption = startupDelayOption ?? CommandLineOptions.LPSRoundCommandOptions.StartupDelayOption;
        private readonly Option<string> _numberOfClientsOption = numberOfClientsOption ?? CommandLineOptions.LPSRoundCommandOptions.NumberOfClientsOption;
        private readonly Option<string> _arrivalDelayOption = arrivalDelayOption ?? CommandLineOptions.LPSRoundCommandOptions.ArrivalDelayOption;
        private readonly Option<string> _delayClientCreationOption = delayClientCreationOption ?? CommandLineOptions.LPSRoundCommandOptions.DelayClientCreation;
        private readonly Option<string?> _runInParallerOption = runInParallerOption ?? CommandLineOptions.LPSRoundCommandOptions.RunInParallel;
        private readonly Option<IList<string>>? _tagOption = tagOption ?? CommandLineOptions.LPSRoundCommandOptions.TagOption;
        #pragma warning disable CS8601 // Possible null reference assignment.
        protected override RoundDto GetBoundValue(BindingContext bindingContext) =>
            new()
            {
                Name = bindingContext.ParseResult.GetValueForOption(_roundNameOption),
                BaseUrl = bindingContext.ParseResult.GetValueForOption(_baseUrlOption),
                StartupDelay = bindingContext.ParseResult.GetValueForOption(_startupDelayOption),
                NumberOfClients = bindingContext.ParseResult.GetValueForOption(_numberOfClientsOption),
                ArrivalDelay = bindingContext.ParseResult.GetValueForOption(_arrivalDelayOption),
                DelayClientCreationUntilIsNeeded = bindingContext.ParseResult.GetValueForOption(_delayClientCreationOption),
                RunInParallel = bindingContext.ParseResult.GetValueForOption(_runInParallerOption),
                Tags = bindingContext.ParseResult.GetValueForOption(_tagOption),
            };
            #pragma warning restore CS8601 // Possible null reference assignment.
    }
}
