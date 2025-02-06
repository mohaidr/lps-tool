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
using LPS.Domain.Domain.Common.Enums;
using LPS.Domain.Domain.Common.Validation;

namespace LPS.Domain
{

    public partial class HttpIteration
    {

        new public class Validator : CommandBaseValidator<HttpIteration, HttpIteration.SetupCommand>
        {
            public override SetupCommand Command => _command;
            public override HttpIteration Entity => _entity;
            ILogger _logger;
            IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            SetupCommand _command;
            HttpIteration _entity;
            public Validator(HttpIteration entity, SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
            {
                _logger = logger;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _command = command;
                _entity = entity;

                #region Validation Rules
                RuleFor(command => command.Name)
               .NotNull().WithMessage("The 'Name' must be a non-null value")
               .NotEmpty().WithMessage("The 'Name' must not be empty")
                .Matches("^[a-zA-Z0-9 _.-]+$")
               .WithMessage("The 'Name' does not accept special charachters")
               .Length(1, 60)
               .WithMessage("The 'Name' should be between 1 and 60 characters");

                RuleFor(command => command.StartupDelay).GreaterThanOrEqualTo(0).
                    WithMessage("The 'StartupDelay' must be greater than or equal to 0");

                RuleFor(command => command.Mode)
                .NotNull()
                .WithMessage("The accepted 'Mode' Values are (DCB,CRB,CB,R,D)");

                RuleFor(command => command.MaximizeThroughput)
               .NotNull()
               .WithMessage("The 'MaximizeThroughput' property must be a non-null value");

                RuleFor(command => command.RequestCount)
                .NotNull()
                .WithMessage("The 'Request Count' must be a non-null value and greater than 0")
                .GreaterThan(0).WithMessage("The 'Request Count' must be greater than 0")
                .When(command => command.Mode == IterationMode.CRB || command.Mode == IterationMode.R)
                .Null()
                .When(command => command.Mode != IterationMode.CRB && command.Mode != IterationMode.R, ApplyConditionTo.CurrentValidator)
                .GreaterThan(command => command.BatchSize)
                .WithMessage("The 'Request Count' Must Be Greater Than The BatchSize")
                .When(command => command.Mode == IterationMode.CRB && command.BatchSize.HasValue, ApplyConditionTo.CurrentValidator);

                RuleFor(command => command.Duration)
                .NotNull().WithMessage("The 'Duration' must be a non-null value and greater than 0")
                .GreaterThan(0).WithMessage("The 'Duration' must be greater than 0")
                .When(command => command.Mode == IterationMode.D || command.Mode == IterationMode.DCB)
                .Null()
                .When(command => command.Mode != IterationMode.D && command.Mode != IterationMode.DCB, ApplyConditionTo.CurrentValidator)
                .GreaterThan(command => command.CoolDownTime/1000)
                    .WithMessage("The 'Duration*1000' Must Be Greater Than The Cool Down Time")
                .When(command => command.Mode == IterationMode.DCB && command.CoolDownTime.HasValue, ApplyConditionTo.CurrentValidator);

                RuleFor(command => command.BatchSize)
                .NotNull().WithMessage("The 'Batch Size' must be a non-null value and greater than 0")
                .GreaterThan(0).WithMessage("The 'Batch Size' must be greater than 0")
                .When(command => command.Mode == IterationMode.DCB || command.Mode == IterationMode.CRB || command.Mode == IterationMode.CB)
                .Null()
                .When(command => command.Mode != IterationMode.DCB && command.Mode != IterationMode.CRB && command.Mode != IterationMode.CB, ApplyConditionTo.CurrentValidator)
                .LessThan(command => command.RequestCount)
                .WithMessage("The 'Batch Size' Must Be Less Than The Request Count")
                .When(command => command.Mode == IterationMode.CRB && command.RequestCount.HasValue, ApplyConditionTo.CurrentValidator);

                RuleFor(command => command.CoolDownTime)
                .NotNull().WithMessage("The 'Cool Down Time' must be a non-null value and greater than 0")
                .GreaterThan(0).WithMessage("The 'Cool Down Time' must be greater than 0")
                .When(command => command.Mode == IterationMode.DCB || command.Mode == IterationMode.CRB || command.Mode == IterationMode.CB)
                .Null()
                .When(command => command.Mode != IterationMode.DCB && command.Mode != IterationMode.CRB && command.Mode != IterationMode.CB, ApplyConditionTo.CurrentValidator)
                .LessThan(command => command.Duration*1000)
                .WithMessage("The 'CoolDownTime/1000' Must Be Less Than The Duration")
                .When(command => command.Mode == IterationMode.DCB && command.Duration.HasValue, ApplyConditionTo.CurrentValidator);
                #endregion

                if (entity.Id != default && command.Id.HasValue && entity.Id != command.Id)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "LPS Http Run: Entity Id Can't be Changed, The Id value will be ignored", LPSLoggingLevel.Warning);
                }
                command.IsValid = base.Validate();  
            }
        }
    }
}

