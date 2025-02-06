using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.EmbeddedResourcesServices
{
    public interface IHtmlResourceDownloaderService
    {
        Task DownloadResourcesAsync(
            string baseUrl,
            Guid requestId,
            CancellationToken cancellationToken);
    }
}
