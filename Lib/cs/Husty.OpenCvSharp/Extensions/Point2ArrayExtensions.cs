using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public static class Point2ArrayExtensions
    {

        public unsafe static Mat ToBinaryImage(this IEnumerable<Point> points, int rows, int cols, byte maxval = 255)
        {
            var mat = new Mat(rows, cols, MatType.CV_8U, 0);
            var d = mat.DataPointer;
            foreach (var p in points)
                d[p.Y * rows + p.X] = maxval;
            return mat;
        }

        public unsafe static Mat AsByteMat(this IEnumerable<Point> points)
        {
            var mat = new Mat(points.Count(), 2, MatType.CV_8U);
            var d = (byte*)mat.Data;
            var p = points.ToArray();
            for (int y = 0; y < mat.Rows; y++)
            {
                d[y * 2 + 0] = (byte)p[y].X;
                d[y * 2 + 1] = (byte)p[y].Y;
            }
            return mat;
        }

        public unsafe static Mat AsSByteMat(this IEnumerable<Point> points)
        {
            var mat = new Mat(points.Count(), 2, MatType.CV_8S);
            var d = (sbyte*)mat.Data;
            var p = points.ToArray();
            for (int y = 0; y < mat.Rows; y++)
            {
                d[y * 2 + 0] = (sbyte)p[y].X;
                d[y * 2 + 1] = (sbyte)p[y].Y;
            }
            return mat;
        }

        public unsafe static Mat AsUShortMat(this IEnumerable<Point> points)
        {
            var mat = new Mat(points.Count(), 2, MatType.CV_16U);
            var d = (ushort*)mat.Data;
            var p = points.ToArray();
            for (int y = 0; y < mat.Rows; y++)
            {
                d[y * 2 + 0] = (ushort)p[y].X;
                d[y * 2 + 1] = (ushort)p[y].Y;
            }
            return mat;
        }

        public unsafe static Mat AsShortMat(this IEnumerable<Point> points)
        {
            var mat = new Mat(points.Count(), 2, MatType.CV_16S);
            var d = (short*)mat.Data;
            var p = points.ToArray();
            for (int y = 0; y < mat.Rows; y++)
            {
                d[y * 2 + 0] = (short)p[y].X;
                d[y * 2 + 1] = (short)p[y].Y;
            }
            return mat;
        }

        public unsafe static Mat AsIntMat(this IEnumerable<Point> points)
        {
            var mat = new Mat(points.Count(), 2, MatType.CV_32S);
            var d = (int*)mat.Data;
            var p = points.ToArray();
            for (int y = 0; y < mat.Rows; y++)
            {
                d[y * 2 + 0] = p[y].X;
                d[y * 2 + 1] = p[y].Y;
            }
            return mat;
        }

        public unsafe static Mat AsFloatMat(this IEnumerable<Point> points)
        {
            var mat = new Mat(points.Count(), 2, MatType.CV_32F);
            var d = (float*)mat.Data;
            var p = points.ToArray();
            for (int y = 0; y < mat.Rows; y++)
            {
                d[y * 2 + 0] = p[y].X;
                d[y * 2 + 1] = p[y].Y;
            }
            return mat;
        }

        public unsafe static Mat AsDoubleMat(this IEnumerable<Point> points)
        {
            var mat = new Mat(points.Count(), 2, MatType.CV_64F);
            var d = (double*)mat.Data;
            var p = points.ToArray();
            for (int y = 0; y < mat.Rows; y++)
            {
                d[y * 2 + 0] = p[y].X;
                d[y * 2 + 1] = p[y].Y;
            }
            return mat;
        }

        public static Point Mean(this IEnumerable<Point> points)
        {
            var x = 0;
            var y = 0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
            }
            x /= points.Count();
            y /= points.Count();
            return new(x, y);
        }

        public static Point2f Mean(this IEnumerable<Point2f> points)
        {
            var x = 0f;
            var y = 0f;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
            }
            x /= (float)points.Count();
            y /= (float)points.Count();
            return new(x, y);
        }

        public static Point2d Mean(this IEnumerable<Point2d> points)
        {
            var x = 0.0;
            var y = 0.0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
            }
            x /= (double)points.Count();
            y /= (double)points.Count();
            return new(x, y);
        }

        public static Point[] ToPointArray(this IEnumerable<Point2f> points)
        {
            return points.Select(p => p.ToPoint()).ToArray();
        }

        public static Point[] ToPointArray(this IEnumerable<Point2d> points)
        {
            return points.Select(p => p.ToPoint()).ToArray();
        }

        public static Point2f[] ToPoint2fArray(this IEnumerable<Point> points)
        {
            return points.Select(p => p.ToPoint2f()).ToArray();
        }

        public static Point2f[] ToPoint2fArray(this IEnumerable<Point2d> points)
        {
            return points.Select(p => p.ToPoint2f()).ToArray();
        }

        public static Point2d[] ToPoint2dArray(this IEnumerable<Point> points)
        {
            return points.Select(p => p.ToPoint2d()).ToArray();
        }

        public static Point2d[] ToPoint2dArray(this IEnumerable<Point2f> points)
        {
            return points.Select(p => p.ToPoint2d()).ToArray();
        }

    }
}
