using System.Collections.Concurrent;

namespace Husty;

public class ObjectPool<T> : IDisposable where T : class
{

    // ------ fields ------ //

    private readonly int _capacity;
    private readonly ConcurrentQueue<T> _pool;


    // ------ constructors ------ //

    public ObjectPool(int capacity, Func<T>? factory = null)
    {
        _capacity = capacity;
        _pool = factory is null ? new() : new(Enumerable.Range(0, _capacity).Select(_ => factory.Invoke()));
    }


    // ------ public methods ------ //

    public T GetObject(Func<T?, T>? replacer = null)
    {
        T obj;
        if (_pool.Count < _capacity)
        {
            if (replacer is null) throw new ArgumentNullException(nameof(replacer));
            obj = replacer(null);
        }
        else
        {
            _pool.TryDequeue(out obj);
            obj = replacer?.Invoke(obj) ?? obj;
        }
        _pool.Enqueue(obj);
        return obj;
    }

    public void Dispose()
    {
        foreach (var obj in _pool)
            if (obj is IDisposable o)
                o.Dispose();
    }

}
