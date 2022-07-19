using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp.Extensions
{

    public static class MatExtensions
    {

        public static Type GetElementType(this Mat input)
        {
            return input.Type().ToElementType();
        }

        public unsafe static Mat Map<T>(this Mat mat, Func<T, T> func) where T : unmanaged, IComparable
        {
            if (mat.GetElementType() != typeof(T))
                throw new TypeLoadException(nameof(T));
            var clone = mat.Clone();
            var d = (T*)clone.Data;
            for (int i = 0; i < mat.Width * mat.Height; i++)
                d[i] = func(d[i]);
            return clone;
        }

        /// <summary>
        /// Get points array from binary image
        /// </summary>
        /// <param name="input">must be 1 channel</param>
        /// <returns>array of points</returns>
        public unsafe static Point[] GetNonZeroLocations(this Mat input)
        {
            
            if (input.Channels() is not 1)
                throw new ArgumentException("Require: channels == 1");

            var length = Cv2.CountNonZero(input);
            var w = input.Width;
            var h = input.Height;
            var type = input.GetElementType();
            if (type == typeof(byte))
                return Do((byte*)input.Data, (byte)0, w, h, length);
            else if (type == typeof(sbyte))
                return Do((sbyte*)input.Data, (sbyte)0, w, h, length);
            else if (type == typeof(ushort))
                return Do((ushort*)input.Data, (ushort)0, w, h, length);
            else if (type == typeof(short))
                return Do((short*)input.Data, (short)0, w, h, length);
            else if (type == typeof(int))
                return Do((int*)input.Data, 0, w, h, length);
            else if (type == typeof(float))
                return Do((float*)input.Data, 0, w, h, length);
            else if (type == typeof(double))
                return Do((double*)input.Data, 0, w, h, length);
            else
                throw new NotSupportedException();

            unsafe static Point[] Do<T>(T* data, T zero, int w, int h, int length) where T : unmanaged, IComparable
            {
                var count = 0;
                var p = new Point[length];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        if (data[y * w + x].CompareTo(zero) > 0)
                            p[count++] = new(x, y);
                return p;
            }

        }

        public static Vec2s ToVec2s(this Mat src)
        {
            if (src.Rows is 2 && src.Cols is 1)
                return new(src.At<short>(0, 0), src.At<short>(1, 0));
            else if (src.Rows is 1 && src.Cols is 2)
                return new(src.At<short>(0, 0), src.At<short>(0, 1));
            else
                throw new ArgumentException("Require: source matrix must be 2x1 or 1x2");
        }

        public static Vec2i ToVec2i(this Mat src)
        {
            if (src.Rows is 2 && src.Cols is 1)
                return new(src.At<int>(0, 0), src.At<int>(1, 0));
            else if (src.Rows is 1 && src.Cols is 2)
                return new(src.At<int>(0, 0), src.At<int>(0, 1));
            else
                throw new ArgumentException("Require: source matrix must be 2x1 or 1x2");
        }

        public static Vec2f ToVec2f(this Mat src)
        {
            if (src.Rows is 2 && src.Cols is 1)
                return new(src.At<float>(0, 0), src.At<float>(1, 0));
            else if (src.Rows is 1 && src.Cols is 2)
                return new(src.At<float>(0, 0), src.At<float>(0, 1));
            else
                throw new ArgumentException("Require: source matrix must be 2x1 or 1x2");
        }

        public static Vec2d ToVec2d(this Mat src)
        {
            if (src.Rows is 2 && src.Cols is 1)
                return new(src.At<double>(0, 0), src.At<double>(1, 0));
            else if (src.Rows is 1 && src.Cols is 2)
                return new(src.At<double>(0, 0), src.At<double>(0, 1));
            else
                throw new ArgumentException("Require: source matrix must be 2x1 or 1x2");
        }

        public static Vec3s ToVec3s(this Mat src)
        {
            if (src.Rows is 3 && src.Cols is 1)
                return new(src.At<short>(0, 0), src.At<short>(1, 0), src.At<short>(2, 0));
            else if (src.Rows is 1 && src.Cols is 3)
                return new(src.At<short>(0, 0), src.At<short>(0, 1), src.At<short>(0, 2));
            else
                throw new ArgumentException("Require: source matrix must be 3x1 or 1x3");
        }

        public static Vec3i ToVec3i(this Mat src)
        {
            if (src.Rows is 3 && src.Cols is 1)
                return new(src.At<int>(0, 0), src.At<int>(1, 0), src.At<int>(2, 0));
            else if (src.Rows is 1 && src.Cols is 3)
                return new(src.At<int>(0, 0), src.At<int>(0, 1), src.At<int>(0, 2));
            else
                throw new ArgumentException("Require: source matrix must be 3x1 or 1x3");
        }

        public static Vec3f ToVec3f(this Mat src)
        {
            if (src.Rows is 3 && src.Cols is 1)
                return new(src.At<float>(0, 0), src.At<float>(1, 0), src.At<float>(2, 0));
            else if (src.Rows is 1 && src.Cols is 3)
                return new(src.At<float>(0, 0), src.At<float>(0, 1), src.At<float>(0, 2));
            else
                throw new ArgumentException("Require: source matrix must be 3x1 or 1x3");
        }

        public static Vec3d ToVec3d(this Mat src)
        {
            if (src.Rows is 3 && src.Cols is 1)
                return new(src.At<double>(0, 0), src.At<double>(1, 0), src.At<double>(2, 0));
            else if (src.Rows is 1 && src.Cols is 3)
                return new(src.At<double>(0, 0), src.At<double>(0, 1), src.At<double>(0, 2));
            else
                throw new ArgumentException("Require: source matrix must be 3x1 or 1x3");
        }

    }

}
