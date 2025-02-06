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
    //This should be a Non-Entity Superclass
    public partial class Request : IDomainEntity, IValidEntity, IRequestEntity
    {
        protected ILogger _logger;
        protected IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        protected IWatchdog _watchdog;
        protected CancellationTokenSource _cts;
        protected Request()
        {
            Id = Guid.NewGuid();
        }

        public Request(Request.SetupCommand command, ILogger logger,
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

        public bool HasFailed { get; protected set; }
    }

}
