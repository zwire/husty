using System;
using System.Collections.Generic;
using System.Linq;

namespace Husty
{
    public static class MathEx
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

        public static int Factorial(this int k)
        {
            return k is 0 ? 1 : k * Factorial(k - 1);
        }
    }
}
