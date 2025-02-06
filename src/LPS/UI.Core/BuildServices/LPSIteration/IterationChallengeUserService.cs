using LPS.Domain;
using LPS.Domain.Domain.Common.Enums;
using LPS.DTOs;
using LPS.UI.Common;
using LPS.UI.Core.LPSValidators;
using Spectre.Console;
namespace LPS.UI.Core.Build.Services
{
    internal class IterationChallengeUserService(bool skipOptionalFields, HttpIterationDto command, string baseUrl, IBaseValidator<HttpIterationDto> validator) : IChallengeUserService<HttpIterationDto>
    {
        IBaseValidator<HttpIterationDto> _validator = validator;
        readonly HttpIterationDto _iterationDto = command;
        private readonly bool _skipOptionalFields = skipOptionalFields;
        private string _baseUrl= baseUrl;
        public bool SkipOptionalFields => _skipOptionalFields;
        public HttpIterationDto Dto => _iterationDto;
        public void Challenge()
        {
            if (!_skipOptionalFields)
            {
                ForceOptionalFields();
            }
            AnsiConsole.MarkupLine("[underline bold blue]Add 'HTTP Iteration' to your round:[/]");
            while (true)
            {
                if (!_validator.Validate(nameof(Dto.Name)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.Name));

                    _iterationDto.Name = AnsiConsole.Ask<string>("What is the [green]'Name'[/] of your [green]'HTTP Iteration'[/]?");
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.StartupDelay)))
                {

                    AnsiConsole.MarkupLine("[blue]Use this option cautiously, such as when you want to mimic the navigation between web pages with delays, resembling how a human would browse between pages.[/]");
                    _validator.PrintValidationErrors(nameof(Dto.StartupDelay));
                    _iterationDto.StartupDelay = AnsiConsole.Ask<int>("Would you like to add a [green]'Startup Delay (in seconds)'[/]? Enter 0 if not.").ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.Mode)))
                {

                    AnsiConsole.MarkupLine("[blue]D[/] stands for duration. [blue]C[/] stands for Cool Down, [blue]R[/] stands for Request Count, [blue]B[/] stands for Batch Size");
                    _validator.PrintValidationErrors(nameof(Dto.Mode));
                    _iterationDto.Mode = AnsiConsole.Ask<IterationMode>("At which [green]'Mode'[/] the 'HTTP RUN' should be executed?").ToString();
                    continue;
                }


                if (!_validator.Validate(nameof(Dto.Duration)))
                {

                    _validator.PrintValidationErrors(nameof(Dto.Duration));
                    _iterationDto.Duration = AnsiConsole.Ask<int>("What is the [green]'Duration' (in seconds)[/] for which each client can send requests to your endpoint?").ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.RequestCount)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.RequestCount));
                    _iterationDto.RequestCount = AnsiConsole.Ask<int>("How [green]'Many Requests'[/] the client is supposed to send?").ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.BatchSize)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.BatchSize));
                    _iterationDto.BatchSize = AnsiConsole.Ask<int>("How [green]'Many Requests'[/] the client has to send in a [green]'Batch'[/]?").ToString();
                    continue;
                }
                if (!_validator.Validate(nameof(Dto.CoolDownTime)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.CoolDownTime));
                    _iterationDto.CoolDownTime = AnsiConsole.Ask<int>("For how [green]'Long' (in Milliseconds)[/] the client should pause before running the next batch?").ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.MaximizeThroughput)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.MaximizeThroughput));
                    _iterationDto.MaximizeThroughput = AnsiConsole.Confirm("Do you want to maximize the [green]throughput[/]? Maximizing the throughput will result in [yellow]higher CPU and Memory usage[/]!", false).ToString();
                    continue;
                }

                break;
            }
            RequestValidator validator = new(_iterationDto.HttpRequest);
            RequestChallengeUserService requestChallengeUserService = new(SkipOptionalFields, _iterationDto.HttpRequest, _baseUrl, validator);
            requestChallengeUserService.Challenge();
        }

        public void ForceOptionalFields()
        {
            if (!_skipOptionalFields)
            {
                _iterationDto.StartupDelay = "-1";
                _iterationDto.MaximizeThroughput = "-1";
            }
        }
    }
}
