using LPS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.MessageServices
{
    public interface IMessageService
    {
        Task<(HttpRequestMessage HttpRequestMessage, long MessageSize)> BuildAsync(HttpRequest httpRequest, string sessionId, CancellationToken token = default);
    }
}
