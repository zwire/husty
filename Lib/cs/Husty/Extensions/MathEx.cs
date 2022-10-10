namespace Husty.Extensions;

public static class MathEx
{

    public static bool IsInterger<T>(this T value)
        where T : struct, IComparable<T>, IEquatable<T>
        => value is byte or sbyte or ushort or short or uint or int or ulong or long;

    public static bool IsDecimal<T>(this T value)
        where T : struct, IComparable<T>, IEquatable<T>
        => value is float or double;

    public static T OrAbove<T>(this T value, T min)
         where T : struct, IComparable<T>, IEquatable<T>
        => value.CompareTo(min) < 0 ? min : value;

    public static T OrBelow<T>(this T value, T max)
         where T : struct, IComparable<T>, IEquatable<T>
        => value.CompareTo(max) > 0 ? max : value;

    public static T InsideOf<T>(this T value, T min, T max)
         where T : struct, IComparable<T>, IEquatable<T>
    {
        if (min.CompareTo(max) > 0) throw new ArgumentException("Min > Max !");
        value = value.CompareTo(min) < 0 ? min : value;
        value = value.CompareTo(max) > 0 ? max : value;
        return value;
    }

    public static int Factorial(this int k)
    {
        return k is 0 ? 1 : k * Factorial(k - 1);
    }

}
