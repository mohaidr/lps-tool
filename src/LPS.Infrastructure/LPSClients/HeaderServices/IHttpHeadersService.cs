using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.HeaderServices
{
    public interface IHttpHeadersService
    {
        Task ApplyHeadersAsync(HttpRequestMessage message, string sessionId, Dictionary<string, string> HttpHeaders, CancellationToken token);
    }
}
