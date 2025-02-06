using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Enums;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Domain.LPSRun.LPSHttpIteration.Scheduler;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace LPS.Domain
{

    public partial class Plan
    {
        public class ExecuteCommand : IAsyncCommand<Plan>
        {
            readonly ILogger _logger;
            readonly IWatchdog _watchdog;
            readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
            readonly IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> _lpsClientManager;
            readonly IClientConfiguration<HttpRequest> _lpsClientConfig;
            readonly IMetricsDataMonitor _lpsMetricsDataMonitor;
            readonly ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationExecutionCommandStatusMonitor;
            readonly CancellationTokenSource _cts;
            protected ExecuteCommand()
            {
            }
            public ExecuteCommand(ILogger logger,
                IWatchdog watchdog,
                IRuntimeOperationIdProvider runtimeOperationIdProvider,
                IClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>> lpsClientManager,
                IClientConfiguration<HttpRequest> lpsClientConfig,
                ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> httpIterationExecutionCommandStatusMonitor,
                IMetricsDataMonitor lpsMetricsDataMonitor,
                CancellationTokenSource cts)
            {
                _logger = logger;
                _watchdog = watchdog;
                _runtimeOperationIdProvider = runtimeOperationIdProvider;
                _lpsClientManager = lpsClientManager;
                _lpsClientConfig = lpsClientConfig;
                _httpIterationExecutionCommandStatusMonitor = httpIterationExecutionCommandStatusMonitor;
                _lpsMetricsDataMonitor = lpsMetricsDataMonitor;
                _cts = cts;
            }
            private ExecutionStatus _executionStatus;
            public ExecutionStatus Status => _executionStatus;
            async public Task ExecuteAsync(Plan entity)
            {
                if (entity == null)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "Plan Entity Must Have a Value", LPSLoggingLevel.Error);
                    throw new ArgumentNullException(nameof(entity));
                }
                entity._logger = this._logger;
                entity._watchdog = this._watchdog;
                entity._runtimeOperationIdProvider = this._runtimeOperationIdProvider;
                entity._lpsClientConfig = this._lpsClientConfig;
                entity._lpsClientManager = this._lpsClientManager;
                entity._lpsMetricsDataMonitor = this._lpsMetricsDataMonitor;
                entity._httpIterationExecutionCommandStatusMonitor = this._httpIterationExecutionCommandStatusMonitor;
                entity._cts = this._cts;
                await entity.ExecuteAsync(this);
            }

            //TODO:: When implementing IQueryable repository so you can run a subset of the defined Rounds
            public IList<Guid> SelectedRounds { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        }
        async private Task ExecuteAsync(ExecuteCommand command)
        {
            if (this.IsValid && this.Rounds.Count > 0)
            {
                RegisterHttpIterationForMonitor(); // Optionally pre-register HTTP runs for monitoring to include them in the dashboard immediately, even with empty execution lists, rather than waiting for each run to start.

                List<Task> awaitableTasks = new();
                #region Loggin Round Details
                awaitableTasks.Add(_logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Plan Details", LPSLoggingLevel.Verbose, _cts.Token));
                awaitableTasks.Add(_logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Plan Name:  {this.Name}", LPSLoggingLevel.Verbose, _cts.Token));
                #endregion

                foreach (var round in Rounds)
                {
                    var roundExecCommand = new Round.ExecuteCommand(_logger,
                        _watchdog,
                        _runtimeOperationIdProvider,
                        _lpsClientManager,
                        _lpsClientConfig,
                        _httpIterationExecutionCommandStatusMonitor,
                        _lpsMetricsDataMonitor,
                        _cts);
                    await roundExecCommand.ExecuteAsync(round);
                }
            }
        }

        private void RegisterHttpIterationForMonitor()
        {
            foreach (var round in Rounds)
            {
                foreach (var iteration in round.GetReadOnlyIterations())
                {
                    if (iteration.Type == IterationType.Http)
                    {
                        _lpsMetricsDataMonitor.TryRegister(round.Name, (HttpIteration)iteration);
                    }
                }
            }
        }
    }
}

