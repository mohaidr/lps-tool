using LPS.Domain;
using LPS.DTOs;
using LPS.UI.Common;
using Spectre.Console;
using System;


namespace LPS.UI.Core.Build.Services
{
    internal class RequestChallengeUserService(bool skipOptionalFields, HttpRequestDto command, string baseUrl, IBaseValidator<HttpRequestDto> validator) : IChallengeUserService<HttpRequestDto>
    {
        readonly IBaseValidator<HttpRequestDto> _validator = validator;
        public bool SkipOptionalFields => _skipOptionalFields;
        private readonly bool _skipOptionalFields = skipOptionalFields;
        private readonly string _baseUrl= baseUrl;
        readonly HttpRequestDto _requestDto = command;
        public HttpRequestDto Dto => _requestDto;
        public void Challenge()
        {
            if (!_skipOptionalFields)
            {
                ForceOptionalFields();
            }

            while (true)
            {

                if (!_validator.Validate(nameof(Dto.HttpMethod)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.HttpMethod));
                    _requestDto.HttpMethod = AnsiConsole.Ask<string>("What is the [green]'Http Request Method'[/]?");
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.URL)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.URL));
                    string inpute= AnsiConsole.Ask<string>("What is the [green]'Http Request URL'[/]?");
                    if (!string.IsNullOrEmpty(_baseUrl) && !inpute.StartsWith("http://") && !inpute.StartsWith("https://"))
                    {
                        inpute = $"{_baseUrl}{inpute}";
                    }
                    _requestDto.URL = inpute;
                    continue;
                }
                if (!_validator.Validate(nameof(Dto.HttpVersion)))
                {
                    _validator.PrintValidationErrors(nameof(_requestDto.HttpVersion));
                    _requestDto.HttpVersion = AnsiConsole.Ask<string>("Which [green]'Http Version'[/] to use?"); ;
                    continue;
                }
                if (!_validator.Validate(nameof(Dto.SupportH2C)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.SupportH2C));
                    Dto.SupportH2C = AnsiConsole.Confirm("Would you like to [green]'Perform'[/] Http2 over cleartext?", false).ToString();
                    continue;
                }
                if (!_validator.Validate(nameof(Dto.SaveResponse)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.SaveResponse));
                    Dto.SaveResponse = AnsiConsole.Confirm("Would you like to [green]'Save'[/] the http responses?", false).ToString();
                    continue;
                }

                if (!_validator.Validate(nameof(Dto.DownloadHtmlEmbeddedResources)))
                {
                    _validator.PrintValidationErrors(nameof(Dto.DownloadHtmlEmbeddedResources));
                    Dto.DownloadHtmlEmbeddedResources = AnsiConsole.Confirm("If the server returns text/html, would you like to [green]'Download'[/] the html embedded resources?", false).ToString();
                    continue;
                }

                break;
            }

            AnsiConsole.MarkupLine("Add request headers as [blue](HeaderName: HeaderValue)[/] each on a line. When finished, type C and press enter");
            _requestDto.HttpHeaders = InputHeaderService.Challenge();


            if (_requestDto.HttpMethod.Equals("PUT", StringComparison.CurrentCultureIgnoreCase) || _requestDto.HttpMethod.Equals("POST", StringComparison.CurrentCultureIgnoreCase) || _requestDto.HttpMethod.Equals("PATCH", StringComparison.CurrentCultureIgnoreCase))
            {
                AnsiConsole.WriteLine("Add payload to your http request.\n - Enter Path:[Path] to read the payload from a path file\n - URL:[URL] to read the payload from a URL \n - Or just add your payload inline");
                _requestDto.Payload = new PayloadDto() { Raw= InputPayloadService.Challenge() } ;
            }
        }

        public void ForceOptionalFields()
        {
            if (!_skipOptionalFields)
            {
                _requestDto.HttpVersion = string.Empty;
                _requestDto.DownloadHtmlEmbeddedResources = "-1";
                _requestDto.SaveResponse = "-1";
            }
        }
    }
}
