using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRun.IterationMode
{
    internal class DMode : IIterationModeService
    {
        private HttpRequest.ExecuteCommand _command;
        private int _duration;
        private IWatchdog _watchdog;
        private readonly string _hostName;
        private HttpRequest _request;

        private DMode(HttpRequest request)
        {
            _request = request;
            _hostName = _request.Url.HostName;
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                int numberOfSentRequests = 0;
                var stopwatch = Stopwatch.StartNew();
                while (stopwatch.Elapsed.TotalSeconds < _duration && !cancellationToken.IsCancellationRequested)
                {
                    await _watchdog.BalanceAsync(_hostName, cancellationToken);
                    await _command.ExecuteAsync(_request);
                    numberOfSentRequests++;
                }
                stopwatch.Stop();
                return numberOfSentRequests;
            }
            catch
            {
                throw;
            }
        }

        public class Builder : IBuilder<DMode>
        {
            private HttpRequest.ExecuteCommand _command;
            private int _duration;
            private IWatchdog _watchdog;
            private HttpRequest _request;

            public Builder SetCommand(HttpRequest.ExecuteCommand command)
            {
                _command = command;
                return this;
            }

            public Builder SetDuration(int duration)
            {
                _duration = duration;
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

            public DMode Build()
            {
                if (_request == null)
                    throw new InvalidOperationException("Request must be provided.");

                var dMode = new DMode(_request)
                {
                    _command = _command,
                    _duration = _duration,
                    _watchdog = _watchdog,
                    _request = _request
                };
                return dMode;
            }
        }
    }
}
