using Husty.Geometry;
using Husty.OpenCvSharp.Extensions;
using OpenCvSharp;

namespace Husty.OpenCvSharp.Transform;

public record PcaResult(Point2d Center, double Val1, double Val2, Angle Angle);

public static class Pca2D
{

  public static PcaResult Compute(Mat input)
  {
    using var mean = new Mat();
    using var eigenVec = new Mat();
    using var eigenVal = new Mat();
    Cv2.PCACompute(input, mean, eigenVec, eigenVal, 2);
    var center = new Point2d(mean.At<float>(0, 0), mean.At<float>(0, 1));
    var val1 = Math.Sqrt(eigenVal.At<float>(0, 0));
    var val2 = Math.Sqrt(eigenVal.At<float>(1, 0));
    var angle = Angle.FromRadian(Math.Atan2(eigenVec.At<float>(0, 1), eigenVec.At<float>(0, 0)));
    if (double.IsNaN(val1)) val1 = 0;
    if (double.IsNaN(val2)) val2 = 0;
    return new(center, val1, val2, angle);
  }

  public static PcaResult Compute(IEnumerable<Point2d> input)
  {
    using var mat = input.AsFloatMat();
    return Compute(mat);
  }

}
