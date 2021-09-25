using System;

namespace Husty
{
    public static class ArrayOperatorExtensions
    {

        // 1D array

        public static T[] Plus<T>(this T[] array, double scalar) where T: IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (T)(object)((double)(object)array[i] + scalar);
            return array2;
        }

        public static T[] Minus<T>(this T[] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (T)(object)((double)(object)array[i] - scalar);
            return array2;
        }

        public static T[] Mul<T>(this T[] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (T)(object)((double)(object)array[i] * scalar);
            return array2;
        }

        public static T[] Div<T>(this T[] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0], out var _))
                throw new ArgumentException();
            var array2 = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
                array2[i] = (T)(object)((double)(object)array[i] / scalar);
            return array2;
        }


        // 2D array

        public static T[,] Plus<T>(this T[,] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new T[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (T)(object)((double)(object)array[y, x] + scalar);
            return array2;
        }

        public static T[,] Minus<T>(this T[,] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new T[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (T)(object)((double)(object)array[y, x] - scalar);
            return array2;
        }

        public static T[,] Mul<T>(this T[,] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new T[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (T)(object)((double)(object)array[y, x] * scalar);
            return array2;
        }

        public static T[,] Div<T>(this T[,] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new T[rows, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = (T)(object)((double)(object)array[y, x] / scalar);
            return array2;
        }


        // 2D jagged array

        public static T[][] Plus<T>(this T[][] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new T[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new T[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (T)(object)((double)(object)array[y][x] + scalar);
                }
            }
            return array2;
        }

        public static T[][] Minus<T>(this T[][] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new T[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new T[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (T)(object)((double)(object)array[y][x] - scalar);
                }
            }
            return array2;
        }

        public static T[][] Mul<T>(this T[][] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new T[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new T[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (T)(object)((double)(object)array[y][x] * scalar);
                }
            }
            return array2;
        }

        public static T[][] Div<T>(this T[][] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new T[rows][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new T[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = (T)(object)((double)(object)array[y][x] / scalar);
                }
            }
            return array2;
        }


        // 3D array

        public static T[,,] Plus<T>(this T[,,] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var depth = array.GetLength(2);
            var array2 = new T[rows, cols, depth];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[y, x, c] = (T)(object)((double)(object)array[y, x, c] + scalar);
            return array2;
        }

        public static T[,,] Minus<T>(this T[,,] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var depth = array.GetLength(2);
            var array2 = new T[rows, cols, depth];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[y, x, c] = (T)(object)((double)(object)array[y, x, c] - scalar);
            return array2;
        }

        public static T[,,] Mul<T>(this T[,,] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var depth = array.GetLength(2);
            var array2 = new T[rows, cols, depth];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[y, x, c] = (T)(object)((double)(object)array[y, x, c] * scalar);
            return array2;
        }

        public static T[,,] Div<T>(this T[,,] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0, 0, 0], out var _))
                throw new ArgumentException();
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var depth = array.GetLength(2);
            var array2 = new T[rows, cols, depth];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[y, x, c] = (T)(object)((double)(object)array[y, x, c] / scalar);
            return array2;
        }


        // 3D jagged array

        public static T[][][] Plus<T>(this T[][][] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new T[rows][][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new T[cols][];
                for (int x = 0; x < cols; x++)
                {
                    var depth = array[y][x].Length;
                    array2[y][x] = new T[depth];
                    for (int c = 0; c < depth; c++)
                    {
                        array2[y][x][c] = (T)(object)((double)(object)array[y][x][c] + scalar);
                    }
                }
            }
            return array2;
        }

        public static T[][][] Minus<T>(this T[][][] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new T[rows][][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new T[cols][];
                for (int x = 0; x < cols; x++)
                {
                    var depth = array[y][x].Length;
                    array2[y][x] = new T[depth];
                    for (int c = 0; c < depth; c++)
                    {
                        array2[y][x][c] = (T)(object)((double)(object)array[y][x][c] - scalar);
                    }
                }
            }
            return array2;
        }

        public static T[][][] Mul<T>(this T[][][] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new T[rows][][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new T[cols][];
                for (int x = 0; x < cols; x++)
                {
                    var depth = array[y][x].Length;
                    array2[y][x] = new T[depth];
                    for (int c = 0; c < depth; c++)
                    {
                        array2[y][x][c] = (T)(object)((double)(object)array[y][x][c] * scalar);
                    }
                }
            }
            return array2;
        }

        public static T[][][] Div<T>(this T[][][] array, double scalar) where T : IConvertible, IComparable
        {
            if (double.TryParse((string)(object)array[0][0][0], out var _))
                throw new ArgumentException();
            var rows = array.Length;
            var array2 = new T[rows][][];
            for (int y = 0; y < rows; y++)
            {
                var cols = array[y].Length;
                array2[y] = new T[cols][];
                for (int x = 0; x < cols; x++)
                {
                    var depth = array[y][x].Length;
                    array2[y][x] = new T[depth];
                    for (int c = 0; c < depth; c++)
                    {
                        array2[y][x][c] = (T)(object)((double)(object)array[y][x][c] / scalar);
                    }
                }
            }
            return array2;
        }

    }
}
