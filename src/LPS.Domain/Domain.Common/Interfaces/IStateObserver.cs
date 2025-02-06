using LPS.Domain.Common.Interfaces;
using LPS.Domain.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Domain.Common.Interfaces
{
    public interface IStateObserver
    {
        void NotifyMe(ExecutionStatus status);
    }
}
