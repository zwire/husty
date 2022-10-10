using System.Reactive.Linq;

namespace Husty.Extensions;

public static class ObservableEx
{

    public static IObservable<T[]> WithHistory<T>(this IObservable<T> src, int count)
    {
        if (count < 1)
            throw new ArgumentException("count must be >= 1");
        return src.Select(x => src.TakeLast(count + 1).ToEnumerable().ToArray());
    }

}
