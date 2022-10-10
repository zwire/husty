using System.Collections.Concurrent;

namespace Husty;

public class ObjectPool<T> : IDisposable where T : new()
{

    // ------ fields ------ //

    private readonly int _capacity;
    private readonly ConcurrentQueue<T> _pool;


    // ------ constructors ------ //

    public ObjectPool(int capacity = 1, Func<T>? factory = null)
    {
        _capacity = capacity;
        _pool = new(Enumerable.Range(0, _capacity).Select(_ => factory is null ? new() : factory.Invoke()));
    }


    // ------ public methods ------ //

    public T GetObject()
    {
        if (!_pool.TryDequeue(out T obj))
            obj = new();
        if (_pool.Count < _capacity)
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
