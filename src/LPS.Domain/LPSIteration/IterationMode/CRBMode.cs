using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRun.IterationMode
{
    internal class CRBMode : IIterationModeService
    {
        private HttpRequest.ExecuteCommand _command;
        private int _requestCount;
        private int _coolDownTime;
        private int _batchSize;
        private bool _maximizeThroughput;
        private IBatchProcessor<HttpRequest.ExecuteCommand, HttpRequest> _batchProcessor;
        private CRBMode() { }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            List<Task<int>> awaitableTasks = [];
            var coolDownWatch = Stopwatch.StartNew();

            bool continueCondition() => _requestCount > 0 && !cancellationToken.IsCancellationRequested;
            Func<bool> batchCondition = ()=> !cancellationToken.IsCancellationRequested;
            bool newBatch = true;
            while (continueCondition())
            {
                int batchSize = _batchSize < _requestCount ? _batchSize : _requestCount;
                if (_maximizeThroughput)
                {
                    if (newBatch)
                    {
                        coolDownWatch.Restart();
                        await Task.Yield();
                        awaitableTasks.Add(_batchProcessor.SendBatchAsync(_command, batchSize, batchCondition, cancellationToken));
                        _requestCount -= batchSize;
                    }
                    newBatch = coolDownWatch.Elapsed.TotalMilliseconds >= _coolDownTime;
                }
                else
                {
                    coolDownWatch.Restart();
                    awaitableTasks.Add(_batchProcessor.SendBatchAsync(_command, batchSize, batchCondition, cancellationToken));
                    _requestCount -= batchSize;
                    if(continueCondition())
                        await Task.Delay((int)Math.Max(_coolDownTime, _coolDownTime - coolDownWatch.ElapsedMilliseconds), cancellationToken);
                }
            }

            coolDownWatch.Stop();
            try
            {
                var results = await Task.WhenAll(awaitableTasks);
                return results.Sum();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public class Builder : IBuilder<CRBMode>
        {
            private HttpRequest.ExecuteCommand _command;
            private int _requestCount;
            private int _coolDownTime;
            private int _batchSize;
            private bool _maximizeThroughput;
            private IBatchProcessor<HttpRequest.ExecuteCommand, HttpRequest> _batchProcessor;

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

            public Builder SetCoolDownTime(int coolDownTime)
            {
                _coolDownTime = coolDownTime;
                return this;
            }

            public Builder SetBatchSize(int batchSize)
            {
                _batchSize = batchSize;
                return this;
            }

            public Builder SetMaximizeThroughput(bool maximizeThroughput)
            {
                _maximizeThroughput = maximizeThroughput;
                return this;
            }

            public Builder SetBatchProcessor(IBatchProcessor<HttpRequest.ExecuteCommand, HttpRequest> batchProcessor)
            {
                _batchProcessor = batchProcessor;
                return this;
            }

            public CRBMode Build()
            {
                var crbMode = new CRBMode
                {
                    _command = _command,
                    _requestCount = _requestCount,
                    _coolDownTime = _coolDownTime,
                    _batchSize = _batchSize,
                    _maximizeThroughput = _maximizeThroughput,
                    _batchProcessor = _batchProcessor
                };
                return crbMode;
            }
        }
    }
}
