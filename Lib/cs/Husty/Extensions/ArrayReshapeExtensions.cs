using System;

namespace Husty
{
    public static class ArrayReshapeExtensions
    {

        // 1D array -> 2D array

        public static T[,] To2DArray<T>(this T[] array, int rows, int cols) where T : IConvertible, IComparable
        {
            if (array.Length != rows * cols) throw new ArgumentException();
            var array2 = new T[rows, cols];
            var index = 0;
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = array[index++];
            return array2;
        }

        public static T[][] To2DJaggedArray<T>(this T[] array, int rows, int cols) where T : IConvertible, IComparable
        {
            if (array.Length != rows * cols) throw new ArgumentException();
            var array2 = new T[rows][];
            var index = 0;
            for (int y = 0; y < rows; y++)
            {
                array2[y] = new T[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = array[index++];
                }
            }
            return array2;
        }

        
        // 2D array -> 1D array

        public static T[] To1DArray<T>(this T[,] array, bool transpose = false) where T : IConvertible, IComparable
        {
            var array2 = new T[array.Length];
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var index = 0;
            if (transpose)
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        array2[index++] = array[y, x];
            else
                for (int x = 0; x < cols; x++)
                    for (int y = 0; y < rows; y++)
                        array2[index++] = array[y, x];
            return array2;
        }

        public static T[] To1DArray<T>(this T[][] array, bool transpose = false) where T : IConvertible, IComparable
        {
            var rows = array.Length;
            var cols = array[0].Length;
            var array2 = new T[rows * cols];
            var index = 0;
            if (transpose)
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        array2[index++] = array[y][x];
            else
                for (int x = 0; x < cols; x++)
                    for (int y = 0; y < rows; y++)
                        array2[index++] = array[x][y];
            return array2;
        }


        // Transpose

        public static T[,] Transpose<T>(this T[,] array) where T : IConvertible, IComparable
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new T[cols, rows];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[x, y] = array[y, x];
            return array2;
        }

        public static T[][] Transpose<T>(this T[][] array) where T : IConvertible, IComparable
        {
            var rows = array.Length;
            var cols = array[0].Length;
            var array2 = new T[rows][];
            for (int y = 0; y < rows; y++)
            {
                if (array[y].Length != cols) throw new ArgumentException();
                array2[y] = new T[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[x][y] = array[y][x];
                }
            }
            return array2;
        }


        // 2D array <--> 2D jagged array

        public static T[][] To2DJaggedArray<T>(this T[,] array) where T : IConvertible, IComparable
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new T[rows][];
            for (int y = 0; y < rows; y++)
            {
                array2[y] = new T[cols];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = array[y, x];
                }
            }
            return array2;
        }

        public static T[,] To2DArray<T>(this T[][] array) where T : IConvertible, IComparable
        {
            var rows = array.Length;
            var cols = array[0].Length;
            var array2 = new T[rows, cols];
            for (int y = 0; y < rows; y++)
            {
                if (array[y].Length != cols) throw new ArgumentException();
                for (int x = 0; x < cols; x++)
                {
                    array2[y, x] = array[y][x];
                }
            }
            return array2;
        }


        // Concatinate

        public static T[,] AddRow<T>(this T[,] array, T[] row) where T : IConvertible, IComparable
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            if (row.Length != cols)
                throw new ArgumentException();
            var array2 = new T[rows + 1, cols];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = array[y, x];
            for (int x = 0; x < cols; x++)
                array2[rows, x] = row[x];
            return array2;
        }

        public static T[][] AddRow<T>(this T[][] array, T[] row) where T : IConvertible, IComparable
        {
            var rows = array.Length;
            var array2 = new T[rows + 1][];
            for (int y = 0; y < rows; y++)
                array2[y] = array[y];
            array2[rows] = row;
            return array2;
        }

        public static T[,] AddColumn<T>(this T[,] array, T[] col) where T : IConvertible, IComparable
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            if (col.Length != rows)
                throw new ArgumentException();
            var array2 = new T[rows, cols + 1];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = array[y, x];
            for (int y = 0; y < rows; y++)
                array2[y, cols] = col[y];
            return array2;
        }

        public static T[][] AddColumn<T>(this T[][] array, T[] col) where T : IConvertible, IComparable
        {
            var rows = array.Length;
            var cols = array[0].Length;
            if (col.Length != rows)
                throw new ArgumentException();
            var array2 = new T[rows][];
            for (int y = 0; y < rows; y++)
            {
                if (array[y].Length != cols) throw new ArgumentException();
                array2[y] = new T[cols + 1];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = array[y][x];
                }
            }
            for (int y = 0; y < rows; y++)
            {
                array2[y][cols] = col[y];
            }
            return array2;
        }


    }
}
