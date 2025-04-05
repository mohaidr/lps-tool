using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;

namespace LPS.Domain
{
    //TODO: Refactor this to base and subclasses 
    public partial class Round : IValidEntity, IDomainEntity, IBusinessEntity
    {

        private ILogger _logger;
        private Round()
        {
            Iterations = [];
            Tags = [];
        }

        IClientManager<HttpRequest,HttpResponse, IClientService<HttpRequest, HttpResponse>> _lpsClientManager;
        IClientConfiguration<HttpRequest> _lpsClientConfig;
        IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        IWatchdog _watchdog;
        IMetricsDataMonitor _lpsMetricsDataMonitor;
        ICommandStatusMonitor<IAsyncCommand<HttpIteration>, HttpIteration> _httpIterationExecutionCommandStatusMonitor;
        CancellationTokenSource _cts;
        public Round(SetupCommand command, 
            ILogger logger,
            IMetricsDataMonitor lpsMetricsDataMonitor,
            IRuntimeOperationIdProvider runtimeOperationIdProvider)
        {
            ArgumentNullException.ThrowIfNull(command);
            Iterations = new List<Iteration>();
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _logger = logger;
            _lpsMetricsDataMonitor = lpsMetricsDataMonitor;
            Id = Guid.NewGuid();
            this.Setup(command);
        }

        public Guid Id { get; protected set; }
        public string Name { get; private set; }
        public int StartupDelay { get; protected set; }
        public bool IsRedo { get; private set; }
        public bool? DelayClientCreationUntilIsNeeded { get; private set; }
        public bool? RunInParallel { get; private set; }
        public bool IsValid { get; private set; }
        public int NumberOfClients { get; private set; }
        public int? ArrivalDelay { get; private set; }
        private IList<Iteration> Iterations { get; set; }
        private IList<string> Tags { get; set; }
    }
}
