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
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Enums;
using LPS.Domain.Domain.Common.Interfaces;

namespace LPS.Domain
{

    public partial class HttpRequest
    {
        readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        public class ExecuteCommand(IClientService<HttpRequest,HttpResponse> httpClientService,
            ILogger logger,
            IWatchdog watchdog,
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            CancellationTokenSource cts) : IAsyncCommand<HttpRequest>, IStateSubject
        {
            private IClientService<HttpRequest, HttpResponse> _httpClientService { get; set; } = httpClientService;
            public IClientService<HttpRequest, HttpResponse> HttpClientService => _httpClientService;
            readonly ILogger _logger = logger;
            readonly IWatchdog _watchdog = watchdog;
            readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider;
            readonly CancellationTokenSource _cts = cts;
            private ExecutionStatus _executionStatus;
            private ExecutionStatus _aggregateStatus;
            public ExecutionStatus Status => _executionStatus;
            public ExecutionStatus AggregateStatus => _aggregateStatus;
            readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
            //TODO: This one method and the calsses uses it are tightly coupled (behavioral coupling)
            //and need to clean it up and use clear contracts as any change in the logic here will break
            //the system 
            async public Task ExecuteAsync(HttpRequest entity)
            {
                try
                {
                    if (entity == null)
                    {
                        _logger.Log(_runtimeOperationIdProvider.OperationId, "HttpRequest Entity Must Have a Value", LPSLoggingLevel.Error);
                        throw new ArgumentNullException(nameof(entity));
                    }
                    entity._logger = this._logger;
                    entity._watchdog = this._watchdog;
                    entity._runtimeOperationIdProvider = this._runtimeOperationIdProvider;
                    entity._cts = this._cts;
                    _executionStatus = ExecutionStatus.Ongoing;
                    await entity.ExecuteAsync(this);

                    if (!entity.HasFailed)
                        _executionStatus = ExecutionStatus.Completed;
                    else
                        _executionStatus = ExecutionStatus.Failed;
                }
                catch (OperationCanceledException) when (_cts.IsCancellationRequested)
                {
                    _executionStatus = ExecutionStatus.Cancelled;
                    throw;
                }
                catch
                {
                    _executionStatus = ExecutionStatus.Failed;
                }
                finally
                {
                    await _semaphoreSlim.WaitAsync();
                    if (_aggregateStatus < _executionStatus)
                    {
                        _aggregateStatus = _executionStatus;
                        NotifyObservers();
                    }
                    _semaphoreSlim.Release();
                }
            }

            private List<IStateObserver> _observers = new();

            public void RegisterObserver(IStateObserver observer)
            {
                _observers.Add(observer);
            }

            public void RemoveObserver(IStateObserver observer)
            {
                _observers.Remove(observer);
            }

            public void NotifyObservers()
            {
                foreach (var observer in _observers)
                {
                    observer.NotifyMe(_aggregateStatus);
                }
            }

        }

        async private Task ExecuteAsync(ExecuteCommand command)
        {
            if (this.IsValid)
            {
                string hostName = this.Url.HostName;
                await _watchdog.BalanceAsync(hostName, _cts.Token);
                try
                {
                    if (command.HttpClientService == null)
                    {
                        throw new InvalidOperationException("Http Client Is Not Defined");
                    }

                    var response = await command.HttpClientService.SendAsync(this, _cts.Token);
                    if (response.IsSuccessStatusCode)
                        this.HasFailed = false; // HasFailed is not valid property here, think of this as an entity you just fetch from DB to execute, so this has to change
                    else
                        this.HasFailed = true;
                }
                catch
                {
                    this.HasFailed = true;
                    throw;
                }
            }
        }
    }
}
