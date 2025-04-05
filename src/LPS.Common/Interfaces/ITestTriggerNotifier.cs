
namespace LPS.Common.Interfaces
{
    public interface ITestTriggerNotifier
    {
        void RegisterObserver(ITestTriggerObserver observer);
        void UnregisterObserver(ITestTriggerObserver observer);
        void NotifyObservers();
    }
}