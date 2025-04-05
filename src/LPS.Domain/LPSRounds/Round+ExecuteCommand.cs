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

    public partial class Round
    {
        IHttpIterationSchedulerService _httpIterationSchedulerService;
        public class ExecuteCommand : IAsyncCommand<Round>
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
            async public Task ExecuteAsync(Round entity)
            {
                if (entity == null)
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, "Round Entity Must Have a Value", LPSLoggingLevel.Error);
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
                entity._httpIterationSchedulerService = new HttpIterationSchedulerService(_logger, _watchdog, _runtimeOperationIdProvider, _lpsMetricsDataMonitor, _httpIterationExecutionCommandStatusMonitor, _cts);
                await entity.ExecuteAsync(this);
            }

            //TODO:: When implementing IQueryable repository so you can run a subset of the defined Runs
            public IList<Guid> SelectedRuns { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        }
        async private Task ExecuteAsync(ExecuteCommand command)
        {
            if (this.IsValid && this.Iterations.Count > 0)
            {
                if (this.StartupDelay > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(this.StartupDelay), _cts.Token);
                }

                List<Task> awaitableTasks = new();
                #region Loggin Round Details
                awaitableTasks.Add(_logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Round Details", LPSLoggingLevel.Verbose, _cts.Token));
                awaitableTasks.Add(_logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Round Name:  {this.Name}", LPSLoggingLevel.Verbose, _cts.Token));
                awaitableTasks.Add(_logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Number Of Clients:  {this.NumberOfClients}", LPSLoggingLevel.Verbose, _cts.Token));
                awaitableTasks.Add(_logger.LogAsync(_runtimeOperationIdProvider.OperationId, $"Delay Client Creation:  {this.DelayClientCreationUntilIsNeeded}", LPSLoggingLevel.Verbose, _cts.Token));
                #endregion

                if (!this.DelayClientCreationUntilIsNeeded.Value)
                {
                    for (int i = 0; i < this.NumberOfClients; i++)
                    {
                        _lpsClientManager.CreateAndQueueClient(_lpsClientConfig);
                    }
                }

                for (int i = 0; i < this.NumberOfClients && !_cts.Token.IsCancellationRequested; i++)
                {
                    IClientService<HttpRequest, HttpResponse> httpClient;
                    if (!this.DelayClientCreationUntilIsNeeded.Value)
                    {
                        httpClient = _lpsClientManager.DequeueClient();
                    }
                    else
                    {
                        httpClient = _lpsClientManager.CreateInstance(_lpsClientConfig);
                    }
                    int delayTime = i * (this.ArrivalDelay?? 0);
                    awaitableTasks.Add(SchedualHttpIterationForExecution(httpClient, DateTime.Now.AddMilliseconds(delayTime)));
                }
                await Task.WhenAll([..awaitableTasks]);
            }
        }

        private async Task SchedualHttpIterationForExecution(IClientService<HttpRequest, HttpResponse> httpClient, DateTime executionTime)
        {
            List<Task> awaitableTasks = [];
            foreach (var httpIteration in this.Iterations.Where(iteration=> iteration.Type == IterationType.Http))
            {
                if (httpIteration == null || !httpIteration.IsValid)
                {
                    continue;
                }
                if (this.RunInParallel.HasValue && this.RunInParallel.Value)
                {
                    awaitableTasks.Add(_httpIterationSchedulerService.ScheduleHttpIterationExecutionAsync(executionTime, (HttpIteration)httpIteration, httpClient));
                }
                else
                {
                    await _httpIterationSchedulerService.ScheduleHttpIterationExecutionAsync(executionTime, (HttpIteration)httpIteration, httpClient);
                }
            }
            await Task.WhenAll(awaitableTasks);
        }
    }
}

