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
    internal class HttpClientValidator : AbstractValidator<HttpClientOptions>
    {
        public HttpClientValidator()
        {
            RuleFor(httpClient => httpClient.ClientTimeoutInSeconds)
                .NotNull().WithMessage("'Client Timeout In Second' must be a non-null value")
                .GreaterThan(0).WithMessage("'Client Timeout In Second' must be greater than 0");
            RuleFor(httpClient => httpClient.PooledConnectionLifeTimeInSeconds)
                .NotNull().WithMessage("'Pooled Connection Life Time In Seconds' must be a non-null value")
                .GreaterThan(0).WithMessage("'Pooled Connection Life Time In Seconds' must be greater than 0");
            RuleFor(httpClient => httpClient.PooledConnectionIdleTimeoutInSeconds)
                .NotNull().WithMessage("'Pooled Connection Idle Timeout In Seconds' must be a non-null value")
                .GreaterThan(0).WithMessage("'Pooled Connection Idle Timeout In Seconds' must be greater than 0");
            RuleFor(httpClient => httpClient.MaxConnectionsPerServer)
                .NotNull().WithMessage("'Max Connections Per Server' a non-null value")
                .GreaterThan(0).WithMessage("'Max Connections Per Server' must be greater than 0");
        }
    }
}
