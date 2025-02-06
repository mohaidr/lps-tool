using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using LPS.Domain.LPSFlow.LPSHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSFlow
{
    public partial class Flow : IValidEntity, IDomainEntity, IBusinessEntity
    {
        protected ILogger _logger;
        protected IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        protected IWatchdog _watchdog;
        protected CancellationTokenSource _cts;

        public Flow(SetupCommand command, ILogger logger,
            IWatchdog watchdog,
            IRuntimeOperationIdProvider runtimeOperationIdProvider)
        {
            ArgumentNullException.ThrowIfNull(command);
            Id = Guid.NewGuid();
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _watchdog = watchdog;
            this.Setup(command);
        }

        public Guid Id { get; protected set; }

        public bool IsValid { get; protected set; }

        public ICollection<Request> Requests { get; protected set; }
    }
}
