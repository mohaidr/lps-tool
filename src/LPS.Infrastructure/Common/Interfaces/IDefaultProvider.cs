namespace LPS.Infrastructure.Common.Interfaces
{
    public interface IDefaultProvider<T> where T : new()
    {
        T GetDefaultInstance();
    }

    public class DefaultProvider<T> : IDefaultProvider<T> where T : new()
    {
        public T GetDefaultInstance()
        {
            return new T();
        }
    }
}
