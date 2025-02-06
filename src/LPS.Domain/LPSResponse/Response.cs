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
    public partial class Response : IValidEntity, IResponseEntity
    {
        protected ILogger _logger;
        protected IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        protected Response()
        {
            Id = Guid.NewGuid();
        }

        public Response(Response.SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
        {
            ArgumentNullException.ThrowIfNull(command);
            Id = Guid.NewGuid();
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
        }
        public Guid Id { get; protected set; }
        public bool IsValid { get; protected set; }
    }
}
