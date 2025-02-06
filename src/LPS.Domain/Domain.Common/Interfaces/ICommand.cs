using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Common.Interfaces
{
    public interface ICommand<TEntity> where TEntity: IDomainEntity
    {
        void Execute(TEntity entity);
    }
}
