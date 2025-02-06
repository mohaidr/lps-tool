using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.Domain.Domain.Common.Interfaces
{
    public interface IStateSubject
    {
        void RegisterObserver(IStateObserver observer);
        void RemoveObserver(IStateObserver observer);
        void NotifyObservers();
    }
}
