using LPS.Domain.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LPS.Domain.Common.Interfaces
{

    public interface IAsyncCommand<TEntity> where TEntity : IDomainEntity
    {
        public ExecutionStatus Status { get; }
        Task ExecuteAsync(TEntity entity);
    }
}
