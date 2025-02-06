using LPS.UI.Common;
using LPS.Domain;
using System;
using LPS.Domain.Common.Interfaces;
using LPS.UI.Core.LPSValidators;
using Spectre.Console;
using LPS.DTOs;

namespace LPS.UI.Core.Build.Services
{
    internal class ManualBuild : IBuilderService<PlanDto>
    {
        IBaseValidator<PlanDto> _validator;
        ILogger _logger;
        IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        IPlaceholderResolverService _placeholderResolverService;
        public ManualBuild(
            IBaseValidator<PlanDto > validator,
            ILogger logger,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            IPlaceholderResolverService placeholderResolverService)
        {
            _validator = validator;
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _placeholderResolverService = placeholderResolverService;
        }

        static bool _skipOptionalFields = true;
        
        //This must be refactored one domain is refactored
        public PlanDto Build(ref PlanDto planDto)
        {
            _skipOptionalFields = AnsiConsole.Confirm("Do you want to skip the optional fields?");

            new PlanChallengeUserService(_skipOptionalFields, planDto, _validator).Challenge();
            return planDto;
        }
    }
}
