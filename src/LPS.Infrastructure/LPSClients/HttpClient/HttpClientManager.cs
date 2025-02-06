using LPS.Domain;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Caching;
using LPS.Infrastructure.Common.Interfaces;
using LPS.Infrastructure.LPSClients.GlobalVariableManager;
using LPS.Infrastructure.LPSClients.MessageServices;
using LPS.Infrastructure.LPSClients.PlaceHolderService;
using LPS.Infrastructure.LPSClients.ResponseService;
using LPS.Infrastructure.LPSClients.SessionManager;
using System.Collections.Generic;



namespace LPS.Infrastructure.LPSClients
{
    //Refactor to queue manager if more queue functionalities are needed
    public class HttpClientManager(ILogger logger, 
        IRuntimeOperationIdProvider runtimeOperationIdProvider, 
        ICacheService<string> memoryCache, 
        ISessionManager sessionManager,
        IMessageService messageService,
        IMetricsService metricsService,
        IResponseProcessingService responseProcessingService, 
        IVariableManager variableManager, IPlaceholderResolverService placeholderResolverService) : IHttpClientManager<HttpRequest, HttpResponse, IClientService<HttpRequest, HttpResponse>>
    {
        readonly ICacheService<string> _memoryCache = memoryCache;
        readonly ILogger _logger = logger;
        readonly Queue<IClientService<HttpRequest, HttpResponse>> _clientsQueue = new Queue<IClientService<HttpRequest, HttpResponse>>();
        readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider = runtimeOperationIdProvider;
        readonly ISessionManager _sessionManager = sessionManager;
        readonly IMessageService _messageService = messageService;
        readonly IMetricsService _metricsService = metricsService;
        readonly IResponseProcessingService _responseProcessingService = responseProcessingService;
        readonly IVariableManager _variableManager = variableManager;
        IPlaceholderResolverService _placeholderResolverService = placeholderResolverService;
        public IClientService<HttpRequest, HttpResponse> CreateInstance(IClientConfiguration<HttpRequest> config)
        {
            var client = new HttpClientService(config, _logger, _runtimeOperationIdProvider, _memoryCache, _sessionManager, _messageService, _metricsService, _responseProcessingService, _variableManager, _placeholderResolverService);
            _logger.Log(_runtimeOperationIdProvider.OperationId, $"Client with Id {client.SessionId} has been created", LPSLoggingLevel.Verbose);
            return client;
        }

        public void CreateAndQueueClient(IClientConfiguration<HttpRequest> config)
        {
            var client = new HttpClientService(config, _logger, _runtimeOperationIdProvider, _memoryCache, _sessionManager, _messageService, _metricsService, _responseProcessingService, _variableManager, _placeholderResolverService);
            _clientsQueue.Enqueue(client);
            _logger.Log(_runtimeOperationIdProvider.OperationId, $"Client with Id {client.SessionId} has been created and queued", LPSLoggingLevel.Verbose);
        }

        public IClientService<HttpRequest, HttpResponse> DequeueClient()
        {
            if (_clientsQueue.Count > 0)
            {
                var client = _clientsQueue.Dequeue();
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"Client with Id {client.SessionId} has been dequeued", LPSLoggingLevel.Verbose);
                return client;
            }
            else
            {
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"Client Queue is empty", LPSLoggingLevel.Warning);
                return null;
            }
        }

        public IClientService<HttpRequest, HttpResponse> DequeueClient(IClientConfiguration<HttpRequest> config, bool byPassQueueIfEmpty)
        {
            if (_clientsQueue.Count > 0)
            {
                var client = _clientsQueue.Dequeue();
                _logger.Log(_runtimeOperationIdProvider.OperationId, $"Client with Id {client.SessionId} was dequeued", LPSLoggingLevel.Information);
                return client;
            }
            else
            {
                if (byPassQueueIfEmpty)
                {
                    var client = new HttpClientService(config, _logger, _runtimeOperationIdProvider, _memoryCache, _sessionManager, _messageService, _metricsService, _responseProcessingService, _variableManager, _placeholderResolverService);
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"Queue was empty but a client with Id {client.SessionId} was created", LPSLoggingLevel.Information);
                    return client;
                }
                else
                {
                    _logger.Log(_runtimeOperationIdProvider.OperationId, $"Client Queue is empty", LPSLoggingLevel.Warning);
                    return null;
                }
            }
        }
    }
}
