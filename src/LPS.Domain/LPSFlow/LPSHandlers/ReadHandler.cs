using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.LPSFlow.LPSHandlers
{
    public partial class ReadHandler : ISessionHandler
    {
        public HandlerType HandlerType => HandlerType.StopIf;
        protected ILogger _logger;
        protected IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        protected IWatchdog _watchdog;
        protected CancellationTokenSource _cts;
        public Guid Id { get; protected set; }
        public bool IsValid
        {
            get; protected set;
        }


        public ReadHandler(SetupCommand command, ILogger logger,
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
    }
}
