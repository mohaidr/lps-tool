using LPS.Domain;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net;
using FluentValidation;
using FluentValidation.Results;
using LPS.UI.Common;
using System.Text;
using Microsoft.AspNetCore.Http;
using LPS.DTOs;
using System.CommandLine;
using LPS.Domain.LPSFlow.LPSHandlers;
using LPS.Domain.Common;
using LPS.Infrastructure.LPSClients.SessionManager;

namespace LPS.UI.Core.LPSValidators
{
    public class CaptureValidator : CommandBaseValidator<CaptureHandlerDto>
    {

        readonly CaptureHandlerDto _captureHandlerDto;
        public CaptureValidator(CaptureHandlerDto captureHandlerDto)
        {
            ArgumentNullException.ThrowIfNull(captureHandlerDto);
            _captureHandlerDto = captureHandlerDto;
            RuleFor(dto => dto.To)
                .NotNull().NotEmpty()
                .WithMessage("'Variable Name' must not be empty")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("'Variable Name' must only contain letters and numbers.");

            RuleFor(dto => dto.MakeGlobal)
                .NotNull()
                .WithMessage("The 'MakeGlobal' property must not be null.")
                .Must(makeGlobal =>
                {
                    // Allow valid boolean values or placeholders
                    return makeGlobal.StartsWith("$") || bool.TryParse(makeGlobal, out _);
                })
                .WithMessage("The 'MakeGlobal' property must be 'true', 'false', or a placeholder starting with '$'");

            RuleFor(dto => dto.As)
                .Must(@as =>
                {
                    @as ??= string.Empty;
                    return IVariableHolder.IsKnownSupportedFormat(MimeTypeExtensions.FromKeyword(@as))
                    || @as == string.Empty;
                }).WithMessage("The provided value for 'As' is not valid or supported.");
            
            RuleFor(dto => dto.Regex)
            .Must(regex => string.IsNullOrEmpty(regex) || IsValidRegex(regex))
            .WithMessage("Input must be either empty or a valid .NET regular expression.");
        }

        public override CaptureHandlerDto Dto { get { return _captureHandlerDto; } }

        private static bool IsValidRegex(string pattern)
        {
            try
            {
                // If the Regex object can be created without exceptions, the pattern is valid
                _ = new System.Text.RegularExpressions.Regex(pattern);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}

