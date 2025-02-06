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
    public class VariableBinder : BinderBase<VariableDto>
    {
        private static Option<string>? _nameOption;
        private static Option<string>? _valueOption;
        private static Option<string>? _asOption;
        private static Option<string>? _regexOption;



        public VariableBinder(Option<string>? nameOption = null,
         Option<string>? valueOPtion = null,
        Option<string>? asOption = null,
         Option<string>? regexOption = null)
        {
            _nameOption = nameOption?? VariableCommandOptions.NameOption;
            _valueOption = valueOPtion?? VariableCommandOptions.ValueOption;
            _asOption = asOption?? VariableCommandOptions.AsOption;
            _regexOption = regexOption?? VariableCommandOptions.RegexOption;
        }

        #pragma warning disable CS8601 // Possible null reference assignment.
        protected override VariableDto GetBoundValue(BindingContext bindingContext) =>
            new()
            {
                Name = bindingContext.ParseResult.GetValueForOption(_nameOption),
                Value = bindingContext.ParseResult.GetValueForOption(_valueOption),
                As = bindingContext.ParseResult.GetValueForOption(_asOption),
                Regex = bindingContext.ParseResult.GetValueForOption(_regexOption),
            };
        #pragma warning restore CS8601 // Possible null reference assignment.
    }
}
