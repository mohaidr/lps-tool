using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.DTOs
{
    public interface IDto<T> where T : IDto<T>
    {
        public void DeepCopy(out T dto);
    }
}
