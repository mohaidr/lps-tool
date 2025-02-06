using System;

namespace LPS.Domain.Common.Interfaces
{
    public interface IClientConfiguration<T> where T : IRequestEntity
    {
    }

    public interface ILPSHttpClientConfiguration<T>: IClientConfiguration<T> where T : IRequestEntity
    {
        public TimeSpan PooledConnectionLifetime { get; }
        public TimeSpan PooledConnectionIdleTimeout { get; }
        public int MaxConnectionsPerServer { get; }
        public TimeSpan Timeout { get; }
    }
}