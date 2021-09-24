using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public static class MatAndArrayExtensions
    {

        public unsafe static byte[] To1DByteArray(this Mat image)
        {
            var length = image.Rows * image.Cols * image.Channels();
            var d = image.DataPointer;
            var array = new byte[length];
            for (int i = 0; i < length; i++) array[i] = d[i];
            return array;
        }

        public unsafe static short[] To1DShortArray(this Mat image)
        {
            var length = image.Rows * image.Cols * image.Channels();
            var d = (short*)image.Data;
            var array = new short[length];
            for (int i = 0; i < length; i++) array[i] = d[i];
            return array;
        }

        public unsafe static int[] To1DIntArray(this Mat image)
        {
            var length = image.Rows * image.Cols * image.Channels();
            var d = (int*)image.Data;
            var array = new int[length];
            for (int i = 0; i < length; i++) array[i] = d[i];
            return array;
        }

        public unsafe static float[] To1DFloatArray(this Mat image)
        {
            var length = image.Rows * image.Cols * image.Channels();
            var d = (float*)image.Data;
            var array = new float[length];
            for (int i = 0; i < length; i++)  array[i] = d[i];
            return array;
        }

        public unsafe static double[] To1DDoubleArray(this Mat image)
        {
            var length = image.Rows * image.Cols * image.Channels();
            var d = (double*)image.Data;
            var array = new double[length];
            for (int i = 0; i < length; i++) array[i] = d[i];
            return array;
        }

        public unsafe static Mat ToMat(this byte[] array, int rows, int cols, int channels)
        {
            var image = new Mat(rows, cols, MatType.CV_8UC(channels));
            var length = rows * cols * channels;
            if (array.Length != length) throw new ArgumentException();
            var d = image.DataPointer;
            for (int i = 0; i < length; i++) d[i] = array[i];
            return image;
        }

        public unsafe static Mat ToMat(this short[] array, int rows, int cols, int channels)
        {
            var image = new Mat(rows, cols, MatType.CV_16SC(channels));
            var length = rows * cols * channels;
            if (array.Length != length) throw new ArgumentException();
            var d = (short*)image.Data;
            for (int i = 0; i < length; i++) d[i] = array[i];
            return image;
        }

        public unsafe static Mat ToMat(this int[] array, int rows, int cols, int channels)
        {
            var image = new Mat(rows, cols, MatType.CV_32SC(channels));
            var length = rows * cols * channels;
            if (array.Length != length) throw new ArgumentException();
            var d = (int*)image.Data;
            for (int i = 0; i < length; i++) d[i] = array[i];
            return image;
        }

        public unsafe static Mat ToMat(this float[] array, int rows, int cols, int channels)
        {
            var image = new Mat(rows, cols, MatType.CV_32FC(channels));
            var length = rows * cols * channels;
            if (array.Length != length) throw new ArgumentException();
            var d = (float*)image.Data;
            for (int i = 0; i < length; i++) d[i] = array[i];
            return image;
        }

        public unsafe static Mat ToMat(this double[] array, int rows, int cols, int channels)
        {
            var image = new Mat(rows, cols, MatType.CV_64FC(channels));
            var length = rows * cols * channels;
            if (array.Length != length) throw new ArgumentException();
            var d = (double*)image.Data;
            for (int i = 0; i < length; i++) d[i] = array[i];
            return image;
        }

    }
}
