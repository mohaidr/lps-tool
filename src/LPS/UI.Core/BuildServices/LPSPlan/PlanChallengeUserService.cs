using System;
using System.Collections.Generic;
using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LPS.Domain;
using LPS.DTOs;
using LPS.UI.Common;
using LPS.UI.Core.LPSValidators;
using Spectre.Console;

namespace LPS.UI.Core.Build.Services
{
    internal class PlanChallengeUserService(bool skipOptionalFields, 
        PlanDto command,
        IBaseValidator<PlanDto> validator) : IChallengeUserService<PlanDto>
    {
        IBaseValidator<PlanDto> _validator = validator;
        readonly PlanDto _planDto = command;
        public PlanDto Dto => _planDto;
        public bool SkipOptionalFields => _skipOptionalFields;
        private readonly bool _skipOptionalFields = skipOptionalFields;

        public void Challenge()
        {
            if (!_skipOptionalFields)
            {
                ForceOptionalFields();
            }
            AnsiConsole.MarkupLine("[underline bold blue]Create a Plan:[/]");
            while (true)
            {
                if (!_validator.Validate(nameof(Dto.Name)) || !_validator.Validate(nameof(Dto.Rounds)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.Name));
                    _validator.PrintValidationErrors(nameof(Dto.Rounds));
                    _planDto.Name = AnsiConsole.Ask<string>("What's your [green]'Plan Name'[/]?");
                    continue;
                }

                RoundDto roundDto = new();
                RoundValidator validator = new(roundDto);
                RoundChallengeUserService lpsRoundUserService = new(SkipOptionalFields, roundDto, validator);
                lpsRoundUserService.Challenge();

                Dto.Rounds.Add(roundDto);

                AnsiConsole.MarkupLine("[bold]Type [blue]add[/] to add a new http Round or press [blue]enter[/] [/]");

                string? action = Console.ReadLine()?.Trim().ToLower();
                if (action == "add")
                {
                    continue;
                }
                break;
            }
        }

        public void ForceOptionalFields()
        {
            if (!_skipOptionalFields)
            {
            }
        }
    }
}
