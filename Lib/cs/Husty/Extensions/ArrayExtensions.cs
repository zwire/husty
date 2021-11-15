using System;
using System.Collections.Generic;
using System.Linq;

namespace Husty
{
    public static class ArrayExtensions
    {

        public static double ArgMax<T>(this IEnumerable<T> src, out int index) where T: struct, IConvertible, IComparable
        {
            if (double.TryParse((string)(object)src.FirstOrDefault(), out var _))
                throw new ArgumentException();
            var max = double.MinValue;
            index = 0;
            int i = 0;
            foreach (var s in src)
            {
                var num = (double)(object)s;
                if (num > max)
                {
                    max = num;
                    index = i;
                }
                i++;
            }
            return max;
        }

        public static double ArgMin<T>(this IEnumerable<T> src, out int index) where T : struct, IConvertible, IComparable
        {
            if (double.TryParse((string)(object)src.FirstOrDefault(), out var _))
                throw new ArgumentException();
            var min = double.MaxValue;
            index = 0;
            int i = 0;
            foreach (var s in src)
            {
                var num = (double)(object)s;
                if (num < min)
                {
                    min = num;
                    index = i;
                }
                i++;
            }
            return min;
        }

        public static double Median<T>(this IEnumerable<T> src) where T : struct, IComparable<T>, IEquatable<T>
        {
            if (!src.Any()) throw new InvalidOperationException("Cannot compute median for an empty set.");
            var doubleArray = src.Select(a => Convert.ToDouble(a)).OrderBy(x => x).ToArray();
            var len = doubleArray.Length;
            var odd = len % 2 is not 0;
            var median = odd ? doubleArray[len / 2] : (doubleArray[len / 2 - 1] + doubleArray[len / 2]) / 2.0;
            return median;
        }

        public static double Variance<T>(this IEnumerable<T> src) where T : struct, IComparable<T>, IEquatable<T>
        {
            if (!src.Any()) throw new InvalidOperationException("Cannot compute median for an empty set.");
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
}
