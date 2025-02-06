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
    public partial class CaptureHandler : ISessionHandler
    {
        protected ILogger _logger;
        protected IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        protected CancellationTokenSource _cts;
        public Guid Id { get; protected set; }
        public string To { get; protected set; }
        public string As { get; protected set; }
        public bool? MakeGlobal { get; protected set; }
        public string Regex { get; protected set; }
        public IList<string> Headers { get; protected set; }

        public HandlerType HandlerType => HandlerType.StopIf;
        public bool IsValid
        {
            get; protected set;
        }

        private CaptureHandler(ILogger logger,
        IRuntimeOperationIdProvider runtimeOperationIdProvider)
        {
            Headers = [];
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
        }

        public CaptureHandler(SetupCommand command, ILogger logger,
        IRuntimeOperationIdProvider runtimeOperationIdProvider)
        {
            ArgumentNullException.ThrowIfNull(command);
            Id = Guid.NewGuid();
            Headers = [];
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            this.Setup(command);
        }

    }
}
