using System;

namespace LPS.Domain.Common.Interfaces
{
    public interface IClientManager<T1, T2, T3> where T1 : IRequestEntity where T2 : IResponseEntity where T3: IClientService<T1, T2>
    {
        T3 CreateInstance(IClientConfiguration<T1> config);

        public void CreateAndQueueClient(IClientConfiguration<T1> config);

        public IClientService<T1, T2> DequeueClient();

        public IClientService<T1, T2> DequeueClient(IClientConfiguration<T1> config, bool byPassQueueIfEmpty);


    }

    public interface IHttpClientManager<T1, T2, T3> : IClientManager<T1, T2, T3> where T1 : IRequestEntity where T2 : IResponseEntity where T3 : IClientService<T1, T2>
    {
       
    }
}