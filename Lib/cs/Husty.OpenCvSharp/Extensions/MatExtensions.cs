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

    }
}
