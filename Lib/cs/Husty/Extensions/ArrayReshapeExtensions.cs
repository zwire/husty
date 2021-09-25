using System;

namespace Husty
{
    public static class ArrayReshapeExtensions
    {

        // 1D array -> 2D array

        public static T[,] To2DArray<T>(this T[] array, int rows, int cols)
        {
            if (array.Length != rows * cols) throw new ArgumentException();
            var array2 = new T[rows, cols];
            var index = 0;
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[y, x] = array[index++];
            return array2;
        }

        public static T[][] To2DJaggedArray<T>(this T[] array, int rows, int cols)
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


        // 1D array -> 3D array

        public static T[,,] To3DArray<T>(this T[] array, int rows, int cols, int depth)
        {
            if (array.Length != rows * cols * depth) throw new ArgumentException();
            var array2 = new T[rows, cols, depth];
            var index = 0;
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[y, x, c] = array[index++];
            return array2;
        }

        public static T[][][] To3DJaggedArray<T>(this T[] array, int rows, int cols, int depth)
        {
            if (array.Length != rows * cols * depth) throw new ArgumentException();
            var array2 = new T[rows][][];
            var index = 0;
            for (int y = 0; y < rows; y++)
            {
                array2[y] = new T[cols][];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = new T[depth];
                    for (int c = 0; c < depth; c++)
                    {
                        array2[y][x][c] = array[index++];
                    }
                }
            }
            return array2;
        }


        // 2D array -> 1D array

        public static T[] To1DArray<T>(this T[,] array, bool transpose = false)
        {
            var array2 = new T[array.Length];
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var index = 0;
            if (!transpose)
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        array2[index++] = array[y, x];
            else
                for (int x = 0; x < cols; x++)
                    for (int y = 0; y < rows; y++)
                        array2[index++] = array[y, x];
            return array2;
        }

        public static T[] To1DArray<T>(this T[][] array, bool transpose = false)
        {
            var rows = array.Length;
            var cols = array[0].Length;
            var array2 = new T[rows * cols];
            var index = 0;
            if (!transpose)
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        array2[index++] = array[y][x];
            else
                for (int x = 0; x < cols; x++)
                    for (int y = 0; y < rows; y++)
                        array2[index++] = array[y][x];
            return array2;
        }


        // 3D array -> 1D array

        public static T[] To1DArray<T>(this T[,,] array)
        {
            var array2 = new T[array.Length];
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var depth = array.GetLength(2);
            var index = 0;
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[index++] = array[y, x, c];
            return array2;
        }

        public static T[] To1DArray<T>(this T[][][] array)
        {
            var rows = array.Length;
            var cols = array[0].Length;
            var depth = array[0][0].Length;
            var array2 = new T[rows * cols * depth];
            var index = 0;
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[index++] = array[y][x][c];
            return array2;
        }


        // 2D transpose

        public static T[,] Transpose<T>(this T[,] array)
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var array2 = new T[cols, rows];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    array2[x, y] = array[y, x];
            return array2;
        }

        public static T[][] Transpose<T>(this T[][] array)
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


        // 3D transpose like numpy

        public static T[,,] Transpose<T>(this T[,,] array, int dim0, int dim1, int dim2)
        {
            if (dim0 == dim1 || dim1 == dim2 || dim2 == dim0 ||
                dim0 < 0 || dim0 > 2 ||
                dim1 < 0 || dim1 > 2 ||
                dim2 < 0 || dim2 > 2)
                throw new ArgumentException();
            if (dim0 is 0)
            {
                if (dim1 is 1)                                          // 0, 1, 2
                {
                    return array;
                }
                else                                                    // 0, 2, 1
                {
                    var rows = array.GetLength(0);
                    var cols = array.GetLength(2);
                    var depth = array.GetLength(1);
                    var array2 = new T[rows, cols, depth];
                    for (int y = 0; y < rows; y++)
                        for (int x = 0; x < cols; x++)
                            for (int c = 0; c < depth; c++)
                                array2[y, x, c] = array[y, c, x];
                    return array2;
                }
            }
            else if (dim0 is 1)
            {
                var rows = array.GetLength(1);
                if (dim1 is 0)                                          // 1, 0, 2
                {
                    var cols = array.GetLength(0);
                    var depth = array.GetLength(2);
                    var array2 = new T[rows, cols, depth];
                    for (int y = 0; y < rows; y++)
                        for (int x = 0; x < cols; x++)
                            for (int c = 0; c < depth; c++)
                                array2[y, x, c] = array[x, y, c];
                    return array2;
                }
                else                                                    // 1, 2, 0
                {
                    var cols = array.GetLength(2);
                    var depth = array.GetLength(0);
                    var array2 = new T[rows, cols, depth];
                    for (int y = 0; y < rows; y++)
                        for (int x = 0; x < cols; x++)
                            for (int c = 0; c < depth; c++)
                                array2[y, x, c] = array[x, c, y];
                    return array2;
                }
            }
            else
            {
                var rows = array.GetLength(2);
                if (dim1 is 0)                                          // 2, 0, 1
                {
                    var cols = array.GetLength(0);
                    var depth = array.GetLength(1);
                    var array2 = new T[rows, cols, depth];
                    for (int y = 0; y < rows; y++)
                        for (int x = 0; x < cols; x++)
                            for (int c = 0; c < depth; c++)
                                array2[y, x, c] = array[c, y, x];
                    return array2;
                }
                else                                                    // 2, 1, 0
                {
                    var cols = array.GetLength(1);
                    var depth = array.GetLength(0);
                    var array2 = new T[rows, cols, depth];
                    for (int y = 0; y < rows; y++)
                        for (int x = 0; x < cols; x++)
                            for (int c = 0; c < depth; c++)
                                array2[y, x, c] = array[c, x, y];
                    return array2;
                }
            }
        }

        public static T[][][] Transpose<T>(this T[][][] array, int dim0, int dim1, int dim2)
        {
            if (dim0 == dim1 || dim1 == dim2 || dim2 == dim0 ||
                dim0 < 0 || dim0 > 2 ||
                dim1 < 0 || dim1 > 2 ||
                dim2 < 0 || dim2 > 2)
                throw new ArgumentException();
            if (dim0 is 0)
            {
                if (dim1 is 1)                                          // 0, 1, 2
                {
                    return array;
                }
                else                                                    // 0, 2, 1
                {
                    var rows = array.Length;
                    var cols = array[0][0].Length;
                    var depth = array[0].Length;
                    var array2 = new T[rows][][];
                    for (int y = 0; y < rows; y++)
                    {
                        array2[y] = new T[cols][];
                        for (int x = 0; x < cols; x++)
                        {
                            array2[y][x] = new T[depth];
                            for (int c = 0; c < depth; c++)
                            {
                                array2[y][x][c] = array[y][c][x];
                            }
                        }
                    }
                    return array2;
                }
            }
            else if (dim0 is 1)
            {
                var rows = array[0].Length;
                if (dim1 is 0)                                          // 1, 0, 2
                {
                    var cols = array.Length;
                    var depth = array[0][0].Length;
                    var array2 = new T[rows][][];
                    for (int y = 0; y < rows; y++)
                    {
                        array2[y] = new T[cols][];
                        for (int x = 0; x < cols; x++)
                        {
                            array2[y][x] = new T[depth];
                            for (int c = 0; c < depth; c++)
                            {
                                array2[y][x][c] = array[x][y][c];
                            }
                        }
                    }
                    return array2;
                }
                else                                                    // 1, 2, 0
                {
                    var cols = array[0][0].Length;
                    var depth = array.Length;
                    var array2 = new T[rows][][];
                    for (int y = 0; y < rows; y++)
                    {
                        array2[y] = new T[cols][];
                        for (int x = 0; x < cols; x++)
                        {
                            array2[y][x] = new T[depth];
                            for (int c = 0; c < depth; c++)
                            {
                                array2[y][x][c] = array[x][c][y];
                            }
                        }
                    }
                    return array2;
                }
            }
            else
            {
                var rows = array[0][0].Length;
                if (dim1 is 0)                                          // 2, 0, 1
                {
                    var cols = array.Length;
                    var depth = array[0].Length;
                    var array2 = new T[rows][][];
                    for (int y = 0; y < rows; y++)
                    {
                        array2[y] = new T[cols][];
                        for (int x = 0; x < cols; x++)
                        {
                            array2[y][x] = new T[depth];
                            for (int c = 0; c < depth; c++)
                            {
                                array2[y][x][c] = array[c][y][x];
                            }
                        }
                    }
                    return array2;
                }
                else                                                    // 2, 1, 0
                {
                    var cols = array[0].Length;
                    var depth = array.Length;
                    var array2 = new T[rows][][];
                    for (int y = 0; y < rows; y++)
                    {
                        array2[y] = new T[cols][];
                        for (int x = 0; x < cols; x++)
                        {
                            array2[y][x] = new T[depth];
                            for (int c = 0; c < depth; c++)
                            {
                                array2[y][x][c] = array[c][x][y];
                            }
                        }
                    }
                    return array2;
                }
            }
        }


        // 2D array <--> 2D jagged array

        public static T[][] To2DJaggedArray<T>(this T[,] array)
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

        public static T[,] To2DArray<T>(this T[][] array)
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


        // 3D array <--> 3D jagged array

        public static T[][][] To3DJaggedArray<T>(this T[,,] array)
        {
            var rows = array.GetLength(0);
            var cols = array.GetLength(1);
            var depth = array.GetLength(2);
            var array2 = new T[rows][][];
            for (int y = 0; y < rows; y++)
            {
                array2[y] = new T[cols][];
                for (int x = 0; x < cols; x++)
                {
                    array2[y][x] = new T[depth];
                    for (int c = 0; c < depth; c++)
                    {
                        array2[y][x][c] = array[y, x, c];
                    }
                }
            }
            return array2;
        }

        public static T[,,] To3DArray<T>(this T[][][] array)
        {
            var rows = array.Length;
            var cols = array[0].Length;
            var depth = array[0][0].Length;
            var array2 = new T[rows, cols, depth];
            for (int y = 0; y < rows; y++)
            {
                if (array[y].Length != cols) throw new ArgumentException();
                for (int x = 0; x < cols; x++)
                {
                    if (array[y][x].Length != depth) throw new ArgumentException();
                    for (int c = 0; c < depth; c++)
                    {
                        array2[y, x, c] = array[y][x][c];
                    }
                }
            }
            return array2;
        }


        // 2D concatinate

        public static T[,] AddRow<T>(this T[,] array, T[] row)
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

        public static T[][] AddRow<T>(this T[][] array, T[] row)
        {
            var rows = array.Length;
            var array2 = new T[rows + 1][];
            for (int y = 0; y < rows; y++)
                array2[y] = array[y];
            array2[rows] = row;
            return array2;
        }

        public static T[,] AddColumn<T>(this T[,] array, T[] col)
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

        public static T[][] AddColumn<T>(this T[][] array, T[] col)
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
