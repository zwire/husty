namespace Husty.Extensions;

public static class ArrayReshapeEx
{

    // 1D array -> 2D array

    public static T[,] To2DArray<T>(this T[] array, int rows, int cols)
    {
        if (array.Length != rows * cols) 
            throw new ArgumentException("invalid size!");
        var span = array.AsSpan();
        var array2 = new T[rows, cols];
        var index = 0;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                array2[y, x] = span[index++];
        return array2;
    }

    public static T[][] To2DJaggedArray<T>(this T[] array, int rows, int cols)
    {
        if (array.Length != rows * cols)
            throw new ArgumentException("invalid size!");
        var span = array.AsSpan();
        var array2 = new T[rows][];
        var index = 0;
        for (int y = 0; y < rows; y++)
        {
            var span2 = new T[cols].AsSpan();
            for (int x = 0; x < cols; x++)
            {
                span2[x] = span[index++];
            }
            array2[y] = span2.ToArray();
        }
        return array2;
    }


    // 1D array -> 3D array

    public static T[,,] To3DArray<T>(this T[] array, int rows, int cols, int depth)
    {
        if (array.Length != rows * cols * depth)
            throw new ArgumentException("invalid size!");
        var span = array.AsSpan();
        var array2 = new T[rows, cols, depth];
        var index = 0;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                for (int c = 0; c < depth; c++)
                    array2[y, x, c] = span[index++];
        return array2;
    }

    public static T[][][] To3DJaggedArray<T>(this T[] array, int rows, int cols, int depth)
    {
        if (array.Length != rows * cols * depth)
            throw new ArgumentException("invalid size!");
        var span = array.AsSpan();
        var array2 = new T[rows][][];
        var index = 0;
        for (int y = 0; y < rows; y++)
        {
            array2[y] = new T[cols][];
            for (int x = 0; x < cols; x++)
            {
                var span2 = new T[depth].AsSpan();
                for (int c = 0; c < depth; c++)
                {
                    span2[c] = span[index++];
                }
                array2[y][x] = span2.ToArray();
            }
        }
        return array2;
    }


    // 2D array -> 1D array

    public static T[] To1DArray<T>(this T[,] array, bool transpose = false)
    {
        var span = new T[array.Length].AsSpan();
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var index = 0;
        if (!transpose)
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    span[index++] = array[y, x];
        else
            for (int x = 0; x < cols; x++)
                for (int y = 0; y < rows; y++)
                    span[index++] = array[y, x];
        return span.ToArray();
    }

    public static T[] To1DArray<T>(this T[][] array, bool transpose = false)
    {
        var rows = array.Length;
        var cols = array[0].Length;
        var span = new T[rows * cols].AsSpan();
        var index = 0;
        if (!transpose)
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    span[index++] = array[y][x];
        else
            for (int x = 0; x < cols; x++)
                for (int y = 0; y < rows; y++)
                    span[index++] = array[y][x];
        return span.ToArray();
    }


    // 3D array -> 1D array

    public static T[] To1DArray<T>(this T[,,] array)
    {
        var span = new T[array.Length].AsSpan();
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var depth = array.GetLength(2);
        var index = 0;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                for (int c = 0; c < depth; c++)
                    span[index++] = array[y, x, c];
        return span.ToArray();
    }

    public static T[] To1DArray<T>(this T[][][] array)
    {
        var rows = array.Length;
        var cols = array[0].Length;
        var depth = array[0][0].Length;
        var span = new T[rows * cols * depth].AsSpan();
        var index = 0;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                for (int c = 0; c < depth; c++)
                    span[index++] = array[y][x][c];
        return span.ToArray();
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
            if (array[y].Length != cols) 
                throw new ArgumentException("invalid array shape");
            var span2 = new T[cols].AsSpan();
            for (int x = 0; x < cols; x++)
            {
                span2[x] = array[y][x];
            }
            array2[y] = span2.ToArray();
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
            throw new ArgumentException("invalid dimension input");
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
            var cols = array.GetLength(0);
            var depth = array.GetLength(2);
            var array2 = new T[rows, cols, depth];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[y, x, c] = array[x, y, c];
            if (dim1 is 0)                                          // 1, 0, 2
            {
                return array2;
            }
            else                                                    // 1, 2, 0
            {
                cols = array.GetLength(2);
                depth = array.GetLength(0);
                var array3 = new T[rows, cols, depth];
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        for (int c = 0; c < depth; c++)
                            array3[y, x, c] = array2[y, c, x];
                return array3;
            }
        }
        else
        {
            var rows = array.GetLength(2);
            var cols = array.GetLength(1);
            var depth = array.GetLength(0);
            var array2 = new T[rows, cols, depth];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    for (int c = 0; c < depth; c++)
                        array2[y, x, c] = array[c, x, y];
            if (dim1 is not 0)                                      // 2, 1, 0
            {
                return array2;
            }
            else                                                    // 2, 0, 1
            {
                cols = array.GetLength(0);
                depth = array.GetLength(1);
                var array3 = new T[rows, cols, depth];
                for (int y = 0; y < rows; y++)
                    for (int x = 0; x < cols; x++)
                        for (int c = 0; c < depth; c++)
                            array3[y, x, c] = array2[y, c, x];
                return array3;
            }
        }
    }

    public static T[][][] Transpose<T>(this T[][][] array, int dim0, int dim1, int dim2)
    {
        if (dim0 == dim1 || dim1 == dim2 || dim2 == dim0 ||
            dim0 < 0 || dim0 > 2 ||
            dim1 < 0 || dim1 > 2 ||
            dim2 < 0 || dim2 > 2)
            throw new ArgumentException("invalid dimension input");
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
            if (array[y].Length != cols) 
                throw new ArgumentException("invalid array shape");
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
            if (array[y].Length != cols) 
                throw new ArgumentException("invalid array shape");
            for (int x = 0; x < cols; x++)
            {
                if (array[y][x].Length != depth) 
                    throw new ArgumentException("invalid array shape");
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
            throw new ArgumentException("invalid row length");
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
            throw new ArgumentException("invalid column length");
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
            throw new ArgumentException("invalid column length");
        var array2 = new T[rows][];
        for (int y = 0; y < rows; y++)
        {
            if (array[y].Length != cols) 
                throw new ArgumentException("invalid array shape");
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
