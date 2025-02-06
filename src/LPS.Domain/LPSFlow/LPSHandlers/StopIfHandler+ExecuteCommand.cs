using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Enums;
using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSFlow.LPSHandlers
{
    public partial class StopIfHandler
    {
        public class ExecuteCommand : IAsyncCommand<StopIfHandler>
        {
            private ExecutionStatus _executionStatus;

            public ExecutionStatus Status => _executionStatus;
            ILogger _logger;
            IWatchdog _watchdog;
            IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            IMetricsDataMonitor _lpsMonitoringEnroller;
            CancellationTokenSource _cts;
            protected ExecuteCommand()
            {

            }
            public ExecuteCommand(
                ILogger logger,
                IWatchdog watchdog,
                IRuntimeOperationIdProvider runtimeOperationIdProvider,
                IMetricsDataMonitor lpsMonitoringEnroller,
                CancellationTokenSource cts)
            {
                _logger = logger;
                _watchdog = watchdog;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _lpsMonitoringEnroller = lpsMonitoringEnroller;
                _cts = cts;
            }
            public async Task ExecuteAsync(StopIfHandler entity)
            {
                if (entity == null)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "Flow Entity Must Have a Value", LPSLoggingLevel.Error);
                    throw new ArgumentNullException(nameof(entity));
                }
                await entity.ExecuteAsync(this);
            }
        }

        async public Task ExecuteAsync(ExecuteCommand command)
        {

        }
    }
}
