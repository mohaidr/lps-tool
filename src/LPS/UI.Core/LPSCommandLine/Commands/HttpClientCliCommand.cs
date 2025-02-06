using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Logger;
using LPS.UI.Common;
using LPS.UI.Common.Extensions;
using LPS.UI.Common.Options;
using LPS.UI.Core.LPSCommandLine.Bindings;
using LPS.UI.Core.LPSValidators;
using LPS.UI.Core.Build.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.UI.Core.LPSCommandLine.Commands
{
    internal class HttpClientCliCommand : ICliCommand
    {
        private Command _rootLpsCliCommand;
        private Command _httpClientCommand;
        public Command Command => _httpClientCommand;
        IWritableOptions<HttpClientOptions> _clientOptions;
        ILogger _logger;
        IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public HttpClientCliCommand(Command rootLpsCliCommand, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider, IWritableOptions<HttpClientOptions> clientOptions)
        {
            _rootLpsCliCommand = rootLpsCliCommand;
            _clientOptions = clientOptions;
            _logger = logger;
           _runtimeOperationIdProvider= runtimeOperationIdProvider;
            Setup();
        }
        private void Setup()
        {
            _httpClientCommand = new Command("httpclient", "Configure the http client");
            CommandLineOptions.AddOptionsToCommand(_httpClientCommand, typeof(CommandLineOptions.LPSHttpClientCommandOptions));
            _rootLpsCliCommand.AddCommand(_httpClientCommand);
        }

        public void SetHandler(CancellationToken cancellationToken)
        {

            _httpClientCommand.SetHandler((updatedClientOptions) =>
            {
                var httpClientValidator = new HttpClientValidator();
                HttpClientOptions clientOptions = new()
                {
                    MaxConnectionsPerServer = updatedClientOptions.MaxConnectionsPerServer ?? _clientOptions.Value.MaxConnectionsPerServer,
                    PooledConnectionLifeTimeInSeconds = updatedClientOptions.PooledConnectionLifeTimeInSeconds ?? _clientOptions.Value.PooledConnectionLifeTimeInSeconds,
                    PooledConnectionIdleTimeoutInSeconds = updatedClientOptions.PooledConnectionIdleTimeoutInSeconds ?? _clientOptions.Value.PooledConnectionIdleTimeoutInSeconds,
                    ClientTimeoutInSeconds = updatedClientOptions.ClientTimeoutInSeconds ?? _clientOptions.Value.ClientTimeoutInSeconds
                };
                var validationResults = httpClientValidator.Validate(clientOptions);

                if (!validationResults.IsValid)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "You must update the below properties to have a valid http client configuration. Updating the LPSAppSettings:LPSHttpClientConfiguration section with the provided arguements will create an invalid http client configuration. You may run 'lps httpclient -h' to explore the options", LPSLoggingLevel.Warning);
                    validationResults.PrintValidationErrors();
                }
                else
                {
                    _clientOptions.Update(option =>
                    {
                        option.MaxConnectionsPerServer = clientOptions.MaxConnectionsPerServer;
                        option.PooledConnectionLifeTimeInSeconds = clientOptions.PooledConnectionLifeTimeInSeconds;
                        option.PooledConnectionIdleTimeoutInSeconds = clientOptions.PooledConnectionIdleTimeoutInSeconds;
                        option.ClientTimeoutInSeconds = clientOptions.ClientTimeoutInSeconds;
                    });
                }
            }, new HttpClientBinder());
        }
    }
}
