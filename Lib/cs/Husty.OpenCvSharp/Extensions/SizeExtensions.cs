using OpenCvSharp;

namespace Husty.OpenCvSharp.Extensions
{
    public static class SizeExtensions
    {

        public static Size ToSize(this Size2f size)
        {
            return new(size.Width, size.Height);
        }

        public static Size ToSize(this Size2d size)
        {
            return new(size.Width, size.Height);
        }

        public static Size2f ToSize2f(this Size size)
        {
            return new(size.Width, size.Height);
        }

        public static Size2f ToSize2f(this Size2d size)
        {
            return new(size.Width, size.Height);
        }

        public static Size2d ToSize2d(this Size size)
        {
            return new(size.Width, size.Height);
        }

        public static Size2d ToSize2d(this Size2f size)
        {
            return new(size.Width, size.Height);
        }

        public static Size Scale(this Size size, double x, double y)
        {
            return new(size.Width * x, size.Height * y);
        }

        public static Size2f Scale(this Size2f size, double x, double y)
        {
            return new(size.Width * x, size.Height * y);
        }

        public static Size2d Scale(this Size2d size, double x, double y)
        {
            return new(size.Width * x, size.Height * y);
        }

        public static Size FromTuple((int, int) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }

        public static Size2f FromTuple((float, float) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }

        public static Size2d FromTuple((double, double) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }

        public static (int Width, int Height) ToTuple(this Size size)
        {
            return (size.Width, size.Height);
        }

        public static (float Width, float Height) ToTuple(this Size2f size)
        {
            return (size.Width, size.Height);
        }

        public static (double Width, double Height) ToTuple(this Size2d size)
        {
            return (size.Width, size.Height);
        }

    }
}
