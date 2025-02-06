namespace LPS.Infrastructure.Common.Interfaces
{
    public interface IThroughputMetricCollector : IMetricCollector
    {
        public bool IncreaseConnectionsCount();
        public bool DecreseConnectionsCount(bool isSuccess);
    }
}
