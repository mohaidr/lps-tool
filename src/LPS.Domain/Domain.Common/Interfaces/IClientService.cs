using System;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.Common.Interfaces
{
    public interface IClientService<TRequest, TResponse> where TRequest : IRequestEntity where TResponse : IResponseEntity
    {
        public string SessionId { get; }
        public string GuidId { get; }

        Task<TResponse> SendAsync(TRequest request, CancellationToken token = default);
    }
}