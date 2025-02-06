using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Domain.Common.Interfaces
{
    internal interface IBuilder<T> where T : class
    {
        T Build();
    }
}
