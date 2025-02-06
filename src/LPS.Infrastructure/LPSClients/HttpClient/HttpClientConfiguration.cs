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
        private readonly TimeSpan _pooledConnectionLifetime;
        private readonly TimeSpan _pooledConnectionIdleTimeout;
        private readonly int _maxConnectionsPerServer;
        private readonly TimeSpan _timeout;

        private HttpClientConfiguration()
        {
            _pooledConnectionLifetime = TimeSpan.FromSeconds(1500);
            _pooledConnectionIdleTimeout = TimeSpan.FromSeconds(300);
            _maxConnectionsPerServer = 1000;
            _timeout = TimeSpan.FromSeconds(240);

        }

        public HttpClientConfiguration(TimeSpan pooledConnectionLifetime, TimeSpan pooledConnectionIdleTimeout,
           int maxConnectionsPerServer, TimeSpan timeout)
        {
            _pooledConnectionLifetime = pooledConnectionLifetime;
            _pooledConnectionIdleTimeout = pooledConnectionIdleTimeout;
            _maxConnectionsPerServer = maxConnectionsPerServer;
            _timeout = timeout;
        }

        public static HttpClientConfiguration GetDefaultInstance()
        {
            return new HttpClientConfiguration();
        }

        public TimeSpan PooledConnectionLifetime { get { return _pooledConnectionLifetime; } }
        public TimeSpan PooledConnectionIdleTimeout { get { return _pooledConnectionIdleTimeout; } }
        public int MaxConnectionsPerServer { get { return _maxConnectionsPerServer; } }
        public TimeSpan Timeout { get { return _timeout; } }
    }
}
