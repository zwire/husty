using OpenCvSharp;
using Husty.Geometry;
using static System.Math;

namespace Husty.OpenCvSharp.Extensions;

public static class AngleExtensions
{

    public static Mat ToRotationMatrix(this Angle angle, Axis axis)
    {
        var r = angle.Radian;
        float[] array;
        if (axis is Axis.X)
        {
            array = new[]
            {
                1,             0,              0,
                0, (float)Cos(r), -(float)Sin(r),
                0, (float)Sin(r),  (float)Cos(r)
            };
        }
        else if (axis is Axis.Y)
        {
            array = new[]
            {
                 (float)Cos(r), 0, (float)Sin(r),
                0,              1,             0,
                -(float)Sin(r), 0, (float)Cos(r)
            };
        }
        else
        {
            array = new[]
            {
                (float)Cos(r), -(float)Sin(r), 0,
                (float)Sin(r),  (float)Cos(r), 0,
                            0,              0, 1
            };
        }
        return new(3, 3, MatType.CV_32F, array);
    }

}
