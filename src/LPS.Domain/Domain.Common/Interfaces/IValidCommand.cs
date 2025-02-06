using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Common.Interfaces
{
    public interface IValidCommand<TEntity> 
    {
        bool IsValid { get; set; }
        IDictionary<string, List<string>> ValidationErrors { get; set; }
    }
}
