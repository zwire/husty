using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public static class HustyGeometryExtensions
    {

        public static Vec2d ToOpenCvSharpVec2d(this Vector2D v)
        {
            return new(v.X, v.Y);
        }

        public static Point2d ToOpenCvSharpPoint2d(this Point2D p)
        {
            return new(p.X, p.Y);
        }

    }

}
