using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRun.IterationMode
{
    internal class DCBMode : IIterationModeService
    {
        private HttpRequest.ExecuteCommand _command;
        private int _duration;
        private int _coolDownTime;
        private int _batchSize;
        private bool _maximizeThroughput;
        private IBatchProcessor<HttpRequest.ExecuteCommand, HttpRequest> _batchProcessor;

        private DCBMode() { }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            List<Task<int>> awaitableTasks = [];

            var stopwatch = Stopwatch.StartNew();
            var coolDownWatch = Stopwatch.StartNew();

            bool continueCondition() => stopwatch.Elapsed.TotalSeconds < _duration && !cancellationToken.IsCancellationRequested;
            Func<bool> batchCondition = continueCondition;
            bool newBatch = true;
            while (continueCondition())
            {
                if (_maximizeThroughput)
                {
                    if (newBatch)
                    {
                        coolDownWatch.Restart();
                        await Task.Yield();
                        awaitableTasks.Add(_batchProcessor.SendBatchAsync(_command, _batchSize, batchCondition, cancellationToken));
                    }
                    newBatch = coolDownWatch.Elapsed.TotalMilliseconds >= _coolDownTime;
                }
                else
                {
                    coolDownWatch.Restart();
                    awaitableTasks.Add(_batchProcessor.SendBatchAsync(_command, _batchSize, batchCondition, cancellationToken));
                    if (continueCondition())
                        await Task.Delay((int)Math.Max(_coolDownTime, _coolDownTime - coolDownWatch.ElapsedMilliseconds), cancellationToken);
                }
            }

            coolDownWatch.Stop();
            stopwatch.Stop();

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

        public class Builder : IBuilder<DCBMode>
        {
            private HttpRequest.ExecuteCommand _command;
            private int _duration;
            private int _coolDownTime;
            private int _batchSize;
            private bool _maximizeThroughput;
            private IBatchProcessor<HttpRequest.ExecuteCommand, HttpRequest> _batchProcessor;

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

            public DCBMode Build()
            {
                var dcbMode = new DCBMode
                {
                    _command = _command,
                    _duration = _duration,
                    _coolDownTime = _coolDownTime,
                    _batchSize = _batchSize,
                    _maximizeThroughput = _maximizeThroughput,
                    _batchProcessor = _batchProcessor
                };
                return dcbMode;
            }
        }
    }
}
