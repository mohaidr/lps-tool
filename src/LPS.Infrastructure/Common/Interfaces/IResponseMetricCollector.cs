using LPS.Domain;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Common.Interfaces
{
    public interface IResponseMetricCollector : IMetricCollector
    {
        public IResponseMetricCollector Update(HttpResponse.SetupCommand httpResponse);
        public Task<IResponseMetricCollector> UpdateAsync(HttpResponse.SetupCommand httpResponse);
    }
}
