using LPS.Domain.Common;
using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain
{

    public partial class HttpResponse :Response, IBusinessEntity
    {

        private HttpResponse()
        {
        }


        internal HttpResponse(ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
        {
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
        }

        public HttpResponse(HttpResponse.SetupCommand command, ILogger logger, IRuntimeOperationIdProvider runtimeOperationIdProvider)
        {
            ArgumentNullException.ThrowIfNull(command);
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            this.Setup(command);
        }

        public MimeType ContentType { get; private set; }
        public string LocationToResponse { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string StatusMessage { get; private set; }
        public Dictionary<string, string> ResponseContentHeaders { get; private set; }
        public Dictionary<string, string> ResponseHeaders { get; private set; }
        public bool IsSuccessStatusCode { get; private set; }
        //TODO:
        //- The design will change when we have a repository and DB where validation will be added to make sure that this value does not change once assigned
        //- Assign this through the setupcommand where the command will have the ID so we can fetch the entity from the DB and assign it to prevent creating orphan entities
        public HttpRequest HttpRequest { get; private set; }

        public TimeSpan TotalTime { get; private set; }
    }
}
