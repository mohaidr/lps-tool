using LPS.Domain;
using LPS.UI.Core.Build.Services;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.CommandLine.Parsing;
using LPS.Domain.Domain.Common.Enums;
using LPS.DTOs;

namespace LPS.UI.Core.LPSCommandLine.Bindings
{
    public class CommandBinder : BinderBase<PlanDto>
    {
        private Option<string> _nameOption;
        private Option<string> _roundNameOption;
        private Option<string> _startupDelayOption;
        private Option<string> _httpIterationNameOption;
        private Option<string?> _requestCountOption;
        private Option<string> _maximizeThroughputOption;
        private Option<string?> _duration;
        private Option<string?> _coolDownTime;
        private Option<string?> _batchSize;
        private Option<string> _httpMethodOption;
        private Option<string> _downloadHtmlEmbeddedResourcesOption;
        private Option<string> _saveResponseOption;
        private Option<string?> _supportH2C;
        private Option<string> _httpversionOption;
        private Option<string> _urlOption;
        private Option<IList<string>> _headerOption;
        private Option<string> _payloadOption;
        readonly Option<string> _iterationModeOption;
        private Option<string> _numberOfClientsOption;
        private Option<string?> _arrivalDelayOption;
        private Option<string> _delayClientCreationOption;
        private Option<string> _runInParallerOption;

        public CommandBinder(
            Option<string>? nameOption = null,
            Option<string>? roundNameOption = null,
            Option<string>? startupDelayOption = null,
            Option<string>? numberOfClientsOption = null,
            Option<string?>? arrivalDelayOption = null,
            Option<string>? delayClientCreationOption = null,
            Option<string>? runInParallerOption = null,
            Option<string>? httpIterationNameOption = null,
            Option<string?>? requestCountOption = null,
            Option<string>? iterationModeOption = null,
            Option<string?>? duratiion = null,
            Option<string?>? coolDownTime = null,
            Option<string>? maximizeThroughput = null,
            Option<string?>? batchSizeOption = null,
            Option<string>? httpMethodOption = null,
            Option<string>? httpversionOption = null,
            Option<string>? urlOption = null,
            Option<IList<string>>? headerOption = null,
            Option<string>? payloadOption = null,
            Option<string>? downloadHtmlEmbeddedResourcesOption = null,
            Option<string>? saveResponseOption = null,
            Option<string?>? supportH2C = null)
        {
            _nameOption = nameOption ?? CommandLineOptions.LPSCommandOptions.PlanNameOption;
            _roundNameOption = roundNameOption ?? CommandLineOptions.LPSCommandOptions.RoundNameOption;
            _startupDelayOption = startupDelayOption?? CommandLineOptions.LPSCommandOptions.StartupDelayOption;
            _numberOfClientsOption = numberOfClientsOption ?? CommandLineOptions.LPSCommandOptions.NumberOfClientsOption;
            _arrivalDelayOption = arrivalDelayOption ?? CommandLineOptions.LPSCommandOptions.ArrivalDelayOption;
            _delayClientCreationOption = delayClientCreationOption ?? CommandLineOptions.LPSCommandOptions.DelayClientCreationOption;
            _runInParallerOption = runInParallerOption ?? CommandLineOptions.LPSCommandOptions.RunInParallelOption;
            _httpIterationNameOption = httpIterationNameOption ?? CommandLineOptions.LPSCommandOptions.IterationNameOption;
            _iterationModeOption = iterationModeOption ?? CommandLineOptions.LPSCommandOptions.IterationModeOption;
            _duration = duratiion ?? CommandLineOptions.LPSCommandOptions.Duration;
            _batchSize = batchSizeOption ?? CommandLineOptions.LPSCommandOptions.BatchSize;
            _coolDownTime = coolDownTime ?? CommandLineOptions.LPSCommandOptions.CoolDownTime;
            _requestCountOption = requestCountOption ?? CommandLineOptions.LPSCommandOptions.RequestCountOption;
            _maximizeThroughputOption = maximizeThroughput ?? CommandLineOptions.LPSCommandOptions.MaximizeThroughputOption;
            _httpMethodOption = httpMethodOption ?? CommandLineOptions.LPSCommandOptions.HttpMethodOption;
            _httpversionOption = httpversionOption ?? CommandLineOptions.LPSCommandOptions.HttpVersionOption;
            _urlOption = urlOption ?? CommandLineOptions.LPSCommandOptions.UrlOption;
            _headerOption = headerOption ?? CommandLineOptions.LPSCommandOptions.HeaderOption;
            _payloadOption = payloadOption ?? CommandLineOptions.LPSCommandOptions.PayloadOption;
            _downloadHtmlEmbeddedResourcesOption = downloadHtmlEmbeddedResourcesOption ?? CommandLineOptions.LPSCommandOptions.DownloadHtmlEmbeddedResources;
            _saveResponseOption = saveResponseOption ?? CommandLineOptions.LPSCommandOptions.SaveResponse;
            _supportH2C = supportH2C ?? CommandLineOptions.LPSCommandOptions.SupportH2C;
        }

        protected override PlanDto GetBoundValue(BindingContext bindingContext)
        {
            #pragma warning disable CS8601 // Possible null reference assignment.
            #pragma warning disable CS8604 // Possible null reference argument.
            return new PlanDto()
            {
                Name = bindingContext.ParseResult.GetValueForOption(_nameOption),
                Rounds = new List<RoundDto>()
                {
                    new RoundDto()
                    {
                        Name = bindingContext.ParseResult.GetValueForOption(_roundNameOption),
                        StartupDelay = bindingContext.ParseResult.GetValueForOption(_startupDelayOption),
                        NumberOfClients = bindingContext.ParseResult.GetValueForOption(_numberOfClientsOption),
                        ArrivalDelay = bindingContext.ParseResult.GetValueForOption(_arrivalDelayOption),
                        DelayClientCreationUntilIsNeeded = bindingContext.ParseResult.GetValueForOption(_delayClientCreationOption),
                        RunInParallel = bindingContext.ParseResult.GetValueForOption(_runInParallerOption),
                        Iterations = new List<HttpIterationDto>()
                        {
                            new()
                            {
                                Name = bindingContext.ParseResult.GetValueForOption(_httpIterationNameOption),
                                Mode = Enum.TryParse(bindingContext.ParseResult.GetValueForOption(_iterationModeOption), true, out IterationMode im) == true ? im.ToString(): string.Empty ,
                                RequestCount = bindingContext.ParseResult.GetValueForOption(_requestCountOption),
                                MaximizeThroughput = bindingContext.ParseResult.GetValueForOption(_maximizeThroughputOption),
                                Duration = bindingContext.ParseResult.GetValueForOption(_duration),
                                CoolDownTime = bindingContext.ParseResult.GetValueForOption(_coolDownTime),
                                BatchSize = bindingContext.ParseResult.GetValueForOption(_batchSize),
                                HttpRequest = new HttpRequestDto()
                                {
                                    HttpMethod = bindingContext.ParseResult.GetValueForOption(_httpMethodOption),
                                    HttpVersion = bindingContext.ParseResult.GetValueForOption(_httpversionOption),
                                    DownloadHtmlEmbeddedResources = bindingContext.ParseResult.GetValueForOption(_downloadHtmlEmbeddedResourcesOption),
                                    SaveResponse = bindingContext.ParseResult.GetValueForOption(_saveResponseOption),
                                    SupportH2C = bindingContext.ParseResult.GetValueForOption(_supportH2C),
                                    URL = bindingContext.ParseResult.GetValueForOption(_urlOption),
                                    Payload = !string.IsNullOrEmpty(bindingContext.ParseResult.GetValueForOption(_payloadOption)) ? new PayloadDto(){ Raw= InputPayloadService.Parse(bindingContext.ParseResult.GetValueForOption(_payloadOption)) } : new PayloadDto(){ Raw= string.Empty },
                                    HttpHeaders = InputHeaderService.Parse(bindingContext.ParseResult.GetValueForOption(_headerOption)),
                                },
                            }
                        }   
                    } 
                }
            };
            #pragma warning restore CS8604 // Possible null reference argument.
            #pragma warning restore CS8601 // Possible null reference assignment.
        }
    }
}
