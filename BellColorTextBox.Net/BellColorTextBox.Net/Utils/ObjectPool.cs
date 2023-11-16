using Bell.Data;

namespace Bell.Utils;

internal interface IReusable
{
    void Reset();
}

internal class ObjectPool<T>
    where T : IReusable, new()
{
    private const int MaxCapacity = 65536;
    
    private readonly Stack<T> _stack = new();
    
    internal T Get()
    {
        if (_stack.Count > 0)
            return _stack.Pop();
        return new T();
    }
    
    internal void Return(T item)
    {
        if (_stack.Count < MaxCapacity)
        {
            item.Reset();
            _stack.Push(item);
        }
    }
    
    internal void Return(List<T> items)
    {
        if (_stack.Count + items.Count < MaxCapacity)
        {
            foreach (T item in items)
            {
                item.Reset();
                _stack.Push(item);
            }
        }
    }
}