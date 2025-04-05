using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients
{
    public class HttpClientConfiguration : ILPSHttpClientConfiguration<HttpRequest>
    {
        private HttpClientConfiguration()
        {
            PooledConnectionLifetime = TimeSpan.FromSeconds(1500);
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(300);
            MaxConnectionsPerServer = 1000;
            Timeout = TimeSpan.FromSeconds(240);

        }

        public HttpClientConfiguration(TimeSpan pooledConnectionLifetime, TimeSpan pooledConnectionIdleTimeout,
           int maxConnectionsPerServer, TimeSpan timeout)
        {
            PooledConnectionLifetime = pooledConnectionLifetime;
            PooledConnectionIdleTimeout = pooledConnectionIdleTimeout;
            MaxConnectionsPerServer = maxConnectionsPerServer;
            Timeout = timeout;
        }

        public static HttpClientConfiguration GetDefaultInstance()
        {
            return new HttpClientConfiguration();
        }

        public TimeSpan PooledConnectionLifetime { get; }
        public TimeSpan PooledConnectionIdleTimeout { get; }
        public int MaxConnectionsPerServer { get; }
        public TimeSpan Timeout { get; }
    }
}
