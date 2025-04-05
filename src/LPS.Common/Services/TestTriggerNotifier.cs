using LPS.Common.Interfaces;

namespace LPS.Common.Services
{
    public class TestTriggerNotifier : ITestTriggerNotifier
    {
        private readonly List<ITestTriggerObserver> _observers = new();

        public void RegisterObserver(ITestTriggerObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void UnregisterObserver(ITestTriggerObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.OnTestTriggered();
                UnregisterObserver(observer);
            }
        }
    }
}