using System;

namespace Husty
{
    public static class ArrayExtensions
    {

        public static T ArgMax<T>(this T[] array, out int index) where T: IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var max = double.MinValue;
            index = 0;
            for (int i = 0; i < array.Length; i++)
            {
                var num = (double)(object)array[i];
                if (num > max)
                {
                    max = num;
                    index = i;
                }
            }
            return (T)(object)max;
        }

        public static T ArgMin<T>(this T[] array, out int index) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var min = double.MaxValue;
            index = 0;
            for (int i = 0; i < array.Length; i++)
            {
                var num = (double)(object)array[i];
                if (num < min)
                {
                    min = num;
                    index = i;
                }
            }
            return (T)(object)min;
        }

    }
}
