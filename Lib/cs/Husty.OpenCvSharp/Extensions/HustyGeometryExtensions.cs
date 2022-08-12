using OpenCvSharp;
using Husty.Geometry;

namespace Husty.OpenCvSharp.Extensions;

public static class HustyGeometryExtensions
{

    public static Point2D ToHustyPoint2D(this Point p)
    {
        return new(p.X, p.Y);
    }

    public static Point2D ToHustyPoint2D(this Point2f p)
    {
        return new(p.X, p.Y);
    }

    public static Point2D ToHustyPoint2D(this Point2d p)
    {
        return new(p.X, p.Y);
    }

    public static Point3D ToHustyPoint3D(this Point3i p)
    {
        return new(p.X, p.Y, p.Z);
    }

    public static Point3D ToHustyPoint3D(this Point3f p)
    {
        return new(p.X, p.Y, p.Z);
    }

    public static Point3D ToHustyPoint3D(this Point3d p)
    {
        return new(p.X, p.Y, p.Z);
    }

    public static Vec2d ToOpenCvSharpVec2d(this Vector2D v)
    {
        return new(v.X, v.Y);
    }

    public static Vec3d ToOpenCvSharpVec3d(this Vector3D v)
    {
        return new(v.X, v.Y, v.Z);
    }

    public static Point2d ToOpenCvSharpPoint2d(this Point2D p)
    {
        return new(p.X, p.Y);
    }

    public static Point3d ToOpenCvSharpPoint3d(this Point3D p)
    {
        return new(p.X, p.Y, p.Z);
    }

}

