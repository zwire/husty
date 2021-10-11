using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public static class GeometryFactory
    {

        public static Vec2i MakeVector(Point p1, Point p2)
        {
            return new(p2.X - p1.X, p2.Y - p1.Y);
        }

        public static Vec2f MakeVector(Point2f p1, Point2f p2)
        {
            return new(p2.X - p1.X, p2.Y - p1.Y);
        }

        public static Vec2d MakeVector(Point2d p1, Point2d p2)
        {
            return new(p2.X - p1.X, p2.Y - p1.Y);
        }

    }
}
