using System.Numerics;

namespace Husty.Extensions;

public static class ArrayCastEx
{

    // 1D array

    public static byte[] AsByteArray<T>(this T[] array) where T : INumber<T>
    {
        var array2 = new byte[array.Length];
        for (int i = 0; i < array.Length; i++)
            array2[i] = Convert.ToByte(array[i]);
        return array2;
    }

    public static short[] AsShortArray<T>(this T[] array) where T : INumber<T>
    {
        var array2 = new short[array.Length];
        for (int i = 0; i < array.Length; i++)
            array2[i] = Convert.ToInt16(array[i]);
        return array2;
    }

    public static int[] AsIntArray<T>(this T[] array) where T : INumber<T>
    {
        var array2 = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
            array2[i] = Convert.ToInt32(array[i]);
        return array2;
    }

    public static float[] AsFloatArray<T>(this T[] array) where T : INumber<T>
    {
        var array2 = new float[array.Length];
        for (int i = 0; i < array.Length; i++)
            array2[i] = Convert.ToSingle(array[i]);
        return array2;
    }

    public static double[] AsDoubleArray<T>(this T[] array) where T : INumber<T>
    {
        var array2 = new double[array.Length];
        for (int i = 0; i < array.Length; i++)
            array2[i] = Convert.ToDouble(array[i]);
        return array2;
    }


    // 2D array

    public static byte[,] AsByteArray<T>(this T[,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var array2 = new byte[rows, cols];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                array2[y, x] = Convert.ToByte(array[y, x]);
        return array2;
    }

    public static short[,] AsShortArray<T>(this T[,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var array2 = new short[rows, cols];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                array2[y, x] = Convert.ToInt16(array[y, x]);
        return array2;
    }

    public static int[,] AsIntArray<T>(this T[,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var array2 = new int[rows, cols];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                array2[y, x] = Convert.ToInt32(array[y, x]);
        return array2;
    }

    public static float[,] AsFloatArray<T>(this T[,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var array2 = new float[rows, cols];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                array2[y, x] = Convert.ToSingle(array[y, x]);
        return array2;
    }

    public static double[,] AsDoubleArray<T>(this T[,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var array2 = new double[rows, cols];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                array2[y, x] = Convert.ToDouble(array[y, x]);
        return array2;
    }


    // 2D jagged array

    public static byte[][] AsByteArray<T>(this T[][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new byte[rows][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new byte[cols];
            for (int x = 0; x < cols; x++)
                array2[y][x] = Convert.ToByte(array[y][x]);
        }
        return array2;
    }

    public static short[][] AsShortArray<T>(this T[][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new short[rows][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new short[cols];
            for (int x = 0; x < cols; x++)
                array2[y][x] = Convert.ToInt16(array[y][x]);
        }
        return array2;
    }

    public static int[][] AsIntArray<T>(this T[][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new int[rows][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new int[cols];
            for (int x = 0; x < cols; x++)
                array2[y][x] = Convert.ToInt32(array[y][x]);
        }
        return array2;
    }

    public static float[][] AsFloatArray<T>(this T[][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new float[rows][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new float[cols];
            for (int x = 0; x < cols; x++)
                array2[y][x] = Convert.ToSingle(array[y][x]);
        }
        return array2;
    }

    public static double[][] AsDoubleArray<T>(this T[][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new double[rows][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new double[cols];
            for (int x = 0; x < cols; x++)
                array2[y][x] = Convert.ToDouble(array[y][x]);
        }
        return array2;
    }


    // 3D array

    public static byte[,,] AsByteArray<T>(this T[,,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var depth = array.GetLength(2);
        var array2 = new byte[rows, cols, depth];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                for (int c = 0; c < depth; c++)
                    array2[y, x, c] = Convert.ToByte(array[y, x, c]);
        return array2;
    }

    public static short[,,] AsShortArray<T>(this T[,,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var depth = array.GetLength(2);
        var array2 = new short[rows, cols, depth];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                for (int c = 0; c < depth; c++)
                    array2[y, x, c] = Convert.ToInt16(array[y, x, c]);
        return array2;
    }

    public static int[,,] AsIntArray<T>(this T[,,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var depth = array.GetLength(2);
        var array2 = new int[rows, cols, depth];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                for (int c = 0; c < depth; c++)
                    array2[y, x, c] = Convert.ToInt32(array[y, x, c]);
        return array2;
    }

    public static float[,,] AsFloatArray<T>(this T[,,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var depth = array.GetLength(2);
        var array2 = new float[rows, cols, depth];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                for (int c = 0; c < depth; c++)
                    array2[y, x, c] = Convert.ToSingle(array[y, x, c]);
        return array2;
    }

    public static double[,,] AsDoubleArray<T>(this T[,,] array) where T : INumber<T>
    {
        var rows = array.GetLength(0);
        var cols = array.GetLength(1);
        var depth = array.GetLength(2);
        var array2 = new double[rows, cols, depth];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                for (int c = 0; c < depth; c++)
                    array2[y, x, c] = Convert.ToDouble(array[y, x, c]);
        return array2;
    }


    // 3D jagged array

    public static byte[][][] AsByteArray<T>(this T[][][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new byte[rows][][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new byte[cols][];
            for (int x = 0; x < cols; x++)
            {
                var depth = array[y][x].Length;
                array2[y][x] = new byte[depth];
                for (int c = 0; c < depth; c++)
                {
                    array2[y][x][c] = Convert.ToByte(array[y][x][c]);
                }
            }
        }
        return array2;
    }

    public static short[][][] AsShortArray<T>(this T[][][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new short[rows][][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new short[cols][];
            for (int x = 0; x < cols; x++)
            {
                var depth = array[y][x].Length;
                array2[y][x] = new short[depth];
                for (int c = 0; c < depth; c++)
                {
                    array2[y][x][c] = Convert.ToInt16(array[y][x][c]);
                }
            }
        }
        return array2;
    }

    public static int[][][] AsIntArray<T>(this T[][][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new int[rows][][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new int[cols][];
            for (int x = 0; x < cols; x++)
            {
                var depth = array[y][x].Length;
                array2[y][x] = new int[depth];
                for (int c = 0; c < depth; c++)
                {
                    array2[y][x][c] = Convert.ToInt32(array[y][x][c]);
                }
            }
        }
        return array2;
    }

    public static float[][][] AsFloatArray<T>(this T[][][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new float[rows][][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new float[cols][];
            for (int x = 0; x < cols; x++)
            {
                var depth = array[y][x].Length;
                array2[y][x] = new float[depth];
                for (int c = 0; c < depth; c++)
                {
                    array2[y][x][c] = Convert.ToSingle(array[y][x][c]);
                }
            }
        }
        return array2;
    }

    public static double[][][] AsDoubleArray<T>(this T[][][] array) where T : INumber<T>
    {
        var rows = array.Length;
        var array2 = new double[rows][][];
        for (int y = 0; y < rows; y++)
        {
            var cols = array[y].Length;
            array2[y] = new double[cols][];
            for (int x = 0; x < cols; x++)
            {
                var depth = array[y][x].Length;
                array2[y][x] = new double[depth];
                for (int c = 0; c < depth; c++)
                {
                    array2[y][x][c] = Convert.ToDouble(array[y][x][c]);
                }
            }
        }
        return array2;
    }

}
