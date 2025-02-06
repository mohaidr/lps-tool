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
    internal class FileLoggerValidator : AbstractValidator<FileLoggerOptions>
    {
        public FileLoggerValidator()
        {
            RuleFor(logger => logger.LogFilePath)
                .NotNull()
                .NotEmpty()
                .Matches(@"^(\/{0,1}(?!\/))[A-Za-z0-9\/\-_]+(\.([a-zA-Z]+))?$")
                .WithMessage("Invalid File Path");
            RuleFor(logger => logger.LoggingLevel)
                .NotNull()
                .IsInEnum();
            RuleFor(logger => logger.ConsoleLogingLevel)
                .NotNull()
                .IsInEnum();
            RuleFor(logger => logger.EnableConsoleLogging)
                .NotNull();
            RuleFor(logger => logger.DisableConsoleErrorLogging)
                .NotNull();
            RuleFor(logger => logger.DisableFileLogging)
                .NotNull();
        }
    }
}
