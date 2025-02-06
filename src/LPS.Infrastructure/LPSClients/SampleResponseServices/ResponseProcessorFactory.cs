// ResponseProcessingService.cs
using LPS.Domain.Common;
using LPS.Domain.Common.Interfaces;
using LPS.Infrastructure.Caching;
using LPS.Infrastructure.LPSClients.URLServices;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Infrastructure.LPSClients.SampleResponseServices
{
    public class ResponseProcessorFactory : IResponseProcessorFactory
    {
        private readonly ICacheService<string> _memoryCache;
        private readonly ILogger _logger;
        private readonly IRuntimeOperationIdProvider _runtimeOperationIdProvider;
        readonly IUrlSanitizationService _urlSanitizationService;

        public ResponseProcessorFactory(
            IRuntimeOperationIdProvider runtimeOperationIdProvider,
            ILogger logger,
            ICacheService<string> memoryCache,
            IUrlSanitizationService urlSanitizationService)
        {
            _logger = logger;
            _runtimeOperationIdProvider = runtimeOperationIdProvider;
            _memoryCache = memoryCache;
            _urlSanitizationService = urlSanitizationService;
        }

        public async Task<IResponseProcessor> CreateResponseProcessorAsync(HttpResponseMessage responseMessage,MimeType responseContentType, bool saveResponse, CancellationToken token)
        {

            if (!saveResponse)
            {
                // Return a no-op processor
                return new NoOpResponseProcessor();
            }

            // Create a processor that will handle response saving
            var processor = new FileResponseProcessor(
                responseMessage,
                _memoryCache,
                _logger,
                _runtimeOperationIdProvider, 
                _urlSanitizationService);

            await processor.InitializeAsync(responseContentType.ToFileExtension(), token);
            return processor;
        }
    }
}
