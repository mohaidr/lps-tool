using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.LPSRun.LPSHttpIteration.Scheduler
{
    public interface IHttpIterationSchedulerService
    {
        Task ScheduleHttpIterationExecutionAsync(DateTime scheduledTime, HttpIteration httpIteration, IClientService<HttpRequest, HttpResponse> httpClient);
    }
}
