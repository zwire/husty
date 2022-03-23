using System;

namespace Husty.Extensions
{
    public static class MathExtensions
    {

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

    }
}
