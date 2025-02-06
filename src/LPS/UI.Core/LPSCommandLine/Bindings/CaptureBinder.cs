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
using LPS.DTOs;
using static LPS.UI.Core.LPSCommandLine.CommandLineOptions;

namespace LPS.UI.Core.LPSCommandLine.Bindings
{
    public class CaptureBinder : BinderBase<CaptureHandlerDto>
    {
        private static Option<string>? _toOption;
        private static Option<string>? _asOption;
        private static Option<string>? _regexOption;
        private static Option<string>? _makeGlobal;
        private static Option<IList<string>>? _headerOption;



        public CaptureBinder(Option<string>? nameOption = null,
         Option<string>? valueOPtion = null,
         Option<string>? asOption = null,
         Option<string>? regexOption = null,
         Option<string>? makeGlobal = null,
         Option<IList<string>>? headerOption = null)
        {
            _toOption = nameOption?? CaptureCommandOptions.ToOption;
            _asOption = asOption?? CaptureCommandOptions.AsOption;
            _regexOption = regexOption?? CaptureCommandOptions.RegexOption;
            _makeGlobal = makeGlobal ?? CaptureCommandOptions.MakeGlobal;
            _headerOption = headerOption ?? CaptureCommandOptions.HeaderOption;
        }

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8604 // Possible null reference argument.
        protected override CaptureHandlerDto GetBoundValue(BindingContext bindingContext)
        {
           return new()
            {
                To = bindingContext.ParseResult.GetValueForOption(_toOption),
                As = bindingContext.ParseResult.GetValueForOption(_asOption),
                Regex = bindingContext.ParseResult.GetValueForOption(_regexOption),
                MakeGlobal = bindingContext.ParseResult.GetValueForOption(_makeGlobal),
                Headers = bindingContext.ParseResult.GetValueForOption(_headerOption),
            };
        }
        #pragma warning restore CS8604 // Possible null reference argument.
        #pragma warning restore CS8601 // Possible null reference assignment.
    }
}
