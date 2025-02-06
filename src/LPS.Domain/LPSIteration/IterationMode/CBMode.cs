using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRun.IterationMode
{
    internal class CBMode : IIterationModeService
    {
        private HttpRequest.ExecuteCommand _command;
        private int _coolDownTime;
        private int _batchSize;
        private bool _maximizeThroughput;
        private IBatchProcessor<HttpRequest.ExecuteCommand, HttpRequest> _batchProcessor;

        private CBMode() { }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            var coolDownWatch = Stopwatch.StartNew();
            List<Task<int>> awaitableTasks = new List<Task<int>>();

            bool continueCondition() => !cancellationToken.IsCancellationRequested;
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

        public class Builder : IBuilder<CBMode>
        {
            private HttpRequest.ExecuteCommand _command;
            private int _coolDownTime;
            private int _batchSize;
            private bool _maximizeThroughput;
            private IBatchProcessor<HttpRequest.ExecuteCommand, HttpRequest> _batchProcessor;

            public Builder SetCommand(HttpRequest.ExecuteCommand command)
            {
                _command = command;
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

            public CBMode Build()
            {
                var cbMode = new CBMode
                {
                    _command = _command,
                    _coolDownTime = _coolDownTime,
                    _batchSize = _batchSize,
                    _maximizeThroughput = _maximizeThroughput,
                    _batchProcessor = _batchProcessor
                };
                return cbMode;
            }
        }
    }
}
