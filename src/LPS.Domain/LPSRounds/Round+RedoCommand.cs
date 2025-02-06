using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static LPS.Domain.HttpIteration;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Enums;

namespace LPS.Domain
{

    public partial class Round
    {
        public class RedoCommand : IAsyncCommand<Round>
        {
            private ExecutionStatus _executionStatus;
            public ExecutionStatus Status => _executionStatus;
            async public Task ExecuteAsync(Round entity)
            {
                await entity.RedoAsync(this);
            }
        }

        async private Task RedoAsync(RedoCommand command)
        {
            if (this.IsValid)
            {
                this.IsRedo = true;
                await this.ExecuteAsync(new ExecuteCommand(_logger, _watchdog, _runtimeOperationIdProvider, _lpsClientManager, _lpsClientConfig, _httpIterationExecutionCommandStatusMonitor, _lpsMetricsDataMonitor, _cts));
            }
        }
    }
}

