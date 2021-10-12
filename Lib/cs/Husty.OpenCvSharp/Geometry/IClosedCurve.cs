using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public interface IClosedCurve
    {

        public Point2d[] GetEquallySpacedPoints(double interval);


    }
}
