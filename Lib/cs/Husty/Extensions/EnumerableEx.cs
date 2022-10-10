namespace Husty.Extensions;

public static class EnumerableEx
{
    public static IEnumerable<T> Do<T>(this IEnumerable<T> src, Action<T> action)
    {
        return src.Select(x =>
        {
            action(x);
            return x;
        });
    }

    public static IEnumerable<T> Do<T>(this IEnumerable<T> src, Action<T, int> action)
    {
        return src.Select((x, i) =>
        {
            action(x, i);
            return x;
        });
    }

    public static void ForEach<T>(this IEnumerable<T> src, Action<T> action)
    {
        foreach (var item in src)
            action(item);
    }

    public static void ForEach<T>(this IEnumerable<T> src, Action<T, int> action)
    {
        var count = 0;
        foreach (var item in src)
            action(item, count++);
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> src)
    {
        return src.OrderBy(x => Guid.NewGuid());
    }

    public static (T[] First, T[] Second) Split<T>(this IEnumerable<T> src, double firstRate)
    {
        if (firstRate <= 0 || firstRate >= 1)
            throw new ArgumentOutOfRangeException(nameof(firstRate));
        var firstCount = (int)(src.Count() * firstRate);
        return (src.Take(firstCount).ToArray(), src.Skip(firstCount).ToArray());
    }

    public static IEnumerable<T[]> WithHistory<T>(this IEnumerable<T> src, int count)
    {
        if (count < 1)
            throw new ArgumentException("count must be >= 1");
        return src.Select((x, i) => src.Skip(i).Take(count + 1).ToArray()).Where(ary => ary.Length == count + 1);
    }

    public static T Mode<T>(this IEnumerable<T> src)
    {
        return src.GroupBy(x => x).OrderBy(x => x.Count()).Last().Key;
    }

    public static double Median<T>(this IEnumerable<T> src)
        where T : struct, IComparable<T>, IEquatable<T>
    {
        if (!src.Any()) throw new InvalidOperationException("cannot compute median for an empty set.");
        var doubleArray = src.Select(a => Convert.ToDouble(a)).OrderBy(x => x).ToArray();
        var len = doubleArray.Length;
        var odd = len % 2 is not 0;
        var median = odd ? doubleArray[len / 2] : (doubleArray[len / 2 - 1] + doubleArray[len / 2]) / 2.0;
        return median;
    }

    public static double Variance<T>(this IEnumerable<T> src)
        where T : struct, IComparable<T>, IEquatable<T>
    {
        if (!src.Any()) throw new InvalidOperationException("cannot compute median for an empty set.");
        var doubleArray = src.Select(a => Convert.ToDouble(a)).ToArray();
        var mean = doubleArray.Average();
        var sum2 = doubleArray.Select(a => a * a).Sum();
        var variance = sum2 / doubleArray.Length - mean * mean;
        return variance;
    }

    public static double StdDev<T>(this IEnumerable<T> src)
        where T : struct, IComparable<T>, IEquatable<T>
    {
        return Math.Sqrt(Variance(src));
    }
}
