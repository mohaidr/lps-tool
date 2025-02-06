using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRun.IterationMode
{
    internal class RMode : IIterationModeService
    {
        private HttpRequest.ExecuteCommand _command;
        private int _requestCount;
        private IWatchdog _watchdog;
        private readonly string _hostName;
        private HttpRequest _request;

        private RMode(HttpRequest request)
        {
            _request = request;
            _hostName = _request.Url.HostName;
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                int numberOfSentRequests = 0;
                for (int i = 0; i < _requestCount && !cancellationToken.IsCancellationRequested; i++)
                {
                    await _watchdog.BalanceAsync(_hostName, cancellationToken);
                    await _command.ExecuteAsync(_request);
                    numberOfSentRequests++;
                }
                return numberOfSentRequests;
            }
            catch 
            {
                throw;
            }
        }
        public class Builder : IBuilder<RMode>
        {
            private HttpRequest.ExecuteCommand _command;
            private int _requestCount;
            private IWatchdog _watchdog;
            private HttpRequest _request;

            public Builder SetCommand(HttpRequest.ExecuteCommand command)
            {
                _command = command;
                return this;
            }

            public Builder SetRequestCount(int requestCount)
            {
                _requestCount = requestCount;
                return this;
            }

            public Builder SetWatchdog(IWatchdog watchdog)
            {
                _watchdog = watchdog;
                return this;
            }

            public Builder SetRequest(HttpRequest request)
            {
                _request = request;
                return this;
            }

            public RMode Build()
            {
                // Validate required fields
                if (_request == null)
                    throw new InvalidOperationException("Request must be provided.");

                var rMode = new RMode(_request)
                {
                    _command = _command,
                    _requestCount = _requestCount,
                    _watchdog = _watchdog,
                    _request = _request
                };
                return rMode;
            }
        }
    }
}
