using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Validation;

namespace LPS.Domain
{

    public partial class HttpResponse
    {
        public new class Validator : CommandBaseValidator<HttpResponse, HttpResponse.SetupCommand>
        {
            ILogger _logger;
            IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            HttpResponse _entity;
            HttpResponse.SetupCommand _command;
            public override SetupCommand Command => _command;
            public override HttpResponse Entity => _entity;
            public Validator(HttpResponse entity, HttpResponse.SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
            {
                _logger = logger;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _entity = entity;
                _command = command;

                #region Validation Rules
                RuleFor(command => command.LocationToResponse)
                //TODO: This below is commented for now as you need to fetch the HttpRequest from the DB and that is not implemented yet
                //.NotEmpty().When(command=> _entity.HttpRequest.SaveResponse)
                // .WithMessage("'Location To Response' must not be empty.")
                .Must(BeAValidPath).WithMessage("'Location To Response' Path contains illegal characters or does not exist.");
                #endregion

                if (entity.Id != default && command.Id.HasValue && entity.Id != command.Id)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "LPS Http Response: Entity Id Can't be Changed, The Id value will be ignored", LPSLoggingLevel.Warning);
                }

                _command.IsValid = base.Validate();
            }



            private bool BeAValidPath(string location)
            {
                if (string.IsNullOrEmpty(location))
                    return true; // Empty location is considered valid, you can change this if needed

                // Check for invalid path characters
                if (location.Any(c => Path.GetInvalidPathChars().Contains(c)))
                {
                    return false;
                }

                // Check if the file exists
                if (!File.Exists(location))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
