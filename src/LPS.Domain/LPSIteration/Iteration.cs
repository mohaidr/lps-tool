using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;

namespace LPS.Domain
{
    public enum IterationType
    { 
        Http,
        WebSocket //To Be Implemented
    }
    //This should be a Non-Entity Superclass
    public partial class Iteration : IValidEntity, IDomainEntity
    {
        protected ILogger _logger;
        protected IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        protected IWatchdog _watchdog;
        protected IMetricsDataMonitor _lpsMonitoringEnroller;
        protected CancellationTokenSource _cts;
        protected Iteration()
        {
            Id = Guid.NewGuid();
        }

        public Iteration(SetupCommand command, ILogger logger, 
            IRuntimeOperationIdProvider runtimeOperationIdProvider)
        {
            ArgumentNullException.ThrowIfNull(command);
            Id = Guid.NewGuid();
            _logger = logger;            
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            this.Setup(command);
        }

        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public bool IsValid { get; protected set; }
        public IterationType Type { get; protected set; }
    }
}
