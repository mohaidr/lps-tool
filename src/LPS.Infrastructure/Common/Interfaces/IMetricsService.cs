using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LPS.Domain.Common.Interfaces;
using LPS.Domain;
using LPS.Infrastructure.Monitoring.Metrics;
using System;
using LPS.Infrastructure.Logger;

namespace LPS.Infrastructure.Common.Interfaces
{
    public interface IMetricsService
    {
        ValueTask<bool> TryIncreaseConnectionsCountAsync(Guid requestId, CancellationToken token);
        ValueTask<bool> TryDecreaseConnectionsCountAsync(Guid requestId, bool isSuccessful, CancellationToken token);
        ValueTask<bool> TryUpdateResponseMetricsAsync(Guid requestId, HttpResponse.SetupCommand response, CancellationToken token);
        ValueTask<bool> TryUpdateDataSentAsync(Guid requestId, double dataSize, double uploadTime, CancellationToken token);
        ValueTask<bool> TryUpdateDataReceivedAsync(Guid requestId, double dataSize, double downloadTime, CancellationToken token);


    }
}
