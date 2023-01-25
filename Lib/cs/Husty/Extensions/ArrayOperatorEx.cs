using System.Numerics;

namespace Husty.Extensions;

public static class ArrayOperatorEx
{

    // 1D span * scalar

    public static Span<T> Plus<T>(this Span<T> span, T scalar) where T : INumber<T>
    {
        var span2 = new T[span.Length].AsSpan();
        for (int i = 0; i < span.Length; i++)
            span2[i] = span[i] + scalar;
        return span2;
    }

    public static Span<T> Minus<T>(this Span<T> span, T scalar) where T : INumber<T>
    {
        var span2 = new T[span.Length].AsSpan();
        for (int i = 0; i < span.Length; i++)
            span2[i] = span[i] - scalar;
        return span2;
    }

    public static Span<T> Mul<T>(this Span<T> span, T scalar) where T : INumber<T>
    {
        var span2 = new T[span.Length].AsSpan();
        for (int i = 0; i < span.Length; i++)
            span2[i] = span[i] * scalar;
        return span2;
    }

    public static Span<T> Div<T>(this Span<T> span, T scalar) where T : INumber<T>
    {
        var span2 = new T[span.Length].AsSpan();
        for (int i = 0; i < span.Length; i++)
            span2[i] = span[i] / scalar;
        return span2;
    }


    // 1D span * 1D span

    public static Span<T> Plus<T>(this Span<T> span1, Span<T> span2) where T : INumber<T>
    {
        if (span1.Length != span2.Length)
            throw new ArgumentException("must be span1.Length == span2.Length");
        var span3 = new Span<T>(new T[span1.Length]);
        for (int i = 0; i < span1.Length; i++)
            span3[i] = span1[i] + span2[i];
        return span3;
    }

    public static Span<T> Minus<T>(this Span<T> span1, Span<T> span2) where T : INumber<T>
    {
        if (span1.Length != span2.Length)
            throw new ArgumentException("must be span1.Length == span2.Length");
        var span3 = new Span<T>(new T[span1.Length]);
        for (int i = 0; i < span1.Length; i++)
            span3[i] = span1[i] - span2[i];
        return span3;
    }

    public static Span<T> Mul<T>(this Span<T> span1, Span<T> span2) where T : INumber<T>
    {
        if (span1.Length != span2.Length)
            throw new ArgumentException("must be span1.Length == span2.Length");
        var span3 = new Span<T>(new T[span1.Length]);
        for (int i = 0; i < span1.Length; i++)
            span3[i] = span1[i] * span2[i];
        return span3;
    }

    public static Span<T> Div<T>(this Span<T> span1, Span<T> span2) where T : INumber<T>
    {
        if (span1.Length != span2.Length)
            throw new ArgumentException("must be span1.Length == span2.Length");
        var span3 = new Span<T>(new T[span1.Length]);
        for (int i = 0; i < span1.Length; i++)
            span3[i] = span1[i] / span2[i];
        return span3;
    }

}
