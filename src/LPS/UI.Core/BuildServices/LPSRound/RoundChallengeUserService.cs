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
    internal class RoundChallengeUserService(bool skipOptionalFields, RoundDto command, IBaseValidator<RoundDto> validator) : IChallengeUserService<RoundDto>
    {
        readonly IBaseValidator<RoundDto> _validator = validator;
        readonly RoundDto _roundDto = command;
        public RoundDto Dto => _roundDto;
        public bool SkipOptionalFields  => _skipOptionalFields;
        private readonly bool _skipOptionalFields = skipOptionalFields;

        public void Challenge()
        {
            if (!_skipOptionalFields)
            {
                ForceOptionalFields();
            }
            AnsiConsole.MarkupLine("[underline bold blue]Create a Test Round:[/]");
            while (true)
            {
                if (!_validator.Validate(nameof(Dto.Name)) || !_validator.Validate(nameof(Dto.Iterations)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.Name));
                    _validator.PrintValidationErrors(nameof(Dto.Iterations));
                    _roundDto.Name = AnsiConsole.Ask<string>("What's your [green]'Round Name'[/]?");
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.BaseUrl))  && !Dto.BaseUrl.Equals("skip", StringComparison.Ordinal))
                {
                    _validator.PrintValidationErrors(nameof(Dto.BaseUrl));
                    var input = AnsiConsole.Ask<string>("Enter a valid BaseUrl Or type skip?");
                    _roundDto.BaseUrl = input.Equals("skip", StringComparison.Ordinal) ? string.Empty: input;
                    continue;
                }


                if (!_validator.Validate(nameof(Dto.StartupDelay)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.StartupDelay));
                    _roundDto.StartupDelay = AnsiConsole.Ask<int>("Would you like to add a [green]'Startup Delay (in seconds)'[/]? Enter 0 if not.").ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.NumberOfClients)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.NumberOfClients));
                    _roundDto.NumberOfClients = AnsiConsole.Ask<int>("How [green]'Many Clients'[/] should run your test?").ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.ArrivalDelay)))
                {
                    if (Dto.NumberOfClients == 1.ToString())
                    {
                        _roundDto.ArrivalDelay = 1.ToString();
                        continue;
                    }
                    _validator.PrintValidationErrors(nameof(Dto.ArrivalDelay));
                    _roundDto.ArrivalDelay = AnsiConsole.Ask<int>("What is the [green]'Client Arrival Interval'[/] in milliseconds (i.e., the delay between each client's arrival)?").ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.DelayClientCreationUntilIsNeeded)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.DelayClientCreationUntilIsNeeded));
                    _roundDto.DelayClientCreationUntilIsNeeded = AnsiConsole.Confirm("Do you want to [green]'Delay'[/] the client creation until is needed?").ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.RunInParallel)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.RunInParallel));
                    _roundDto.RunInParallel = AnsiConsole.Confirm("Do you want to run all your http iterations in [green]'Parallel'[/]?").ToString();
                    continue;
                }

                HttpIterationDto iterationCommand = new();
                IterationValidator validator = new(iterationCommand);
                IterationChallengeUserService iterationUserService = new(SkipOptionalFields, iterationCommand, _roundDto.BaseUrl, validator);
                iterationUserService.Challenge();

                Dto.Iterations.Add(iterationCommand);

                AnsiConsole.MarkupLine("[bold]Type [blue]add[/] to add a new http iteration or press [blue]enter[/] [/]");

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
                _roundDto.StartupDelay = "-1";
                _roundDto.DelayClientCreationUntilIsNeeded = "-1";
                _roundDto.RunInParallel = "-1";
                _roundDto.BaseUrl = "https://";
            }
        }
    }
}
