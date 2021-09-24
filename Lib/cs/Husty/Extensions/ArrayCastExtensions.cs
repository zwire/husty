using System;

namespace Husty
{
    public static class ArrayCastExtensions
    {

        // 1D array

        public static byte[] AsByteArray<T>(this T[] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new byte[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (byte)(object)array[i];
            return array2;
        }

        public static short[] AsShortArray<T>(this T[] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new short[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (short)(object)array[i];
            return array2;
        }

        public static int[] AsIntArray<T>(this T[] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (int)(object)array[i];
            return array2;
        }

        public static float[] AsFloatArray<T>(this T[] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (float)(object)array[i];
            return array2;
        }

        public static double[] AsDoubleArray<T>(this T[] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (double)(object)array[i];
            return array2;
        }


        // 2D array

        public static byte[,] AsByteArray<T>(this T[,] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new byte[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (byte)(object)array[y, x];
            return array2;
        }

        public static short[,] AsShortArray<T>(this T[,] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new short[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (short)(object)array[y, x];
            return array2;
        }

        public static int[,] AsIntArray<T>(this T[,] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new int[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (int)(object)array[y, x];
            return array2;
        }

        public static float[,] AsFloatArray<T>(this T[,] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new float[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (float)(object)array[y, x];
            return array2;
        }

        public static double[,] AsDoubleArray<T>(this T[,] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new double[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (double)(object)array[y, x];
            return array2;
        }


        // 2D jagged array

        public static byte[][] AsByteArray<T>(this T[][] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new byte[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new byte[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (byte)(object)array[y][x];
                }
            }
            return array2;
        }

        public static short[][] AsShortArray<T>(this T[][] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new short[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new short[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (short)(object)array[y][x];
                }
            }
            return array2;
        }

        public static int[][] AsIntArray<T>(this T[][] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new int[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new int[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (int)(object)array[y][x];
                }
            }
            return array2;
        }

        public static float[][] AsFloatArray<T>(this T[][] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new float[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new float[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (float)(object)array[y][x];
                }
            }
            return array2;
        }

        public static double[][] AsDoubleArray<T>(this T[][] array) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new double[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new double[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (double)(object)array[y][x];
                }
            }
            return array2;
        }

    }
}
