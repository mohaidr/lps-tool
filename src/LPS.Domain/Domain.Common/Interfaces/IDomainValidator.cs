using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Common.Interfaces
{
    public interface IDomainValidator<TEntity, TCommand> where TEntity: IDomainEntity where TCommand: ICommand<TEntity>
    {
        public TCommand Command { get; }
        public TEntity Entity { get; }

        bool Validate();
    }
}
