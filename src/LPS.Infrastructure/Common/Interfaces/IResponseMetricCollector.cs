using LPS.Domain;
using System.Threading.Tasks;

namespace LPS.Infrastructure.Common.Interfaces
{
    public interface IResponseMetricCollector : IMetricCollector
    {
        public IResponseMetricCollector Update(HttpResponse httpResponse);
        public Task<IResponseMetricCollector> UpdateAsync(HttpResponse httpResponse);
    }
}
