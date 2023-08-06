using OpenCvSharp;

namespace Husty.OpenCvSharp.CameraCalibration;

public static class ExtrinsicCameraCalibrator
{

  public static ExtrinsicCameraParameters CalibrateWithGroundCoordinates(
      List<(Point2f, Point3f)> points,
      IntrinsicCameraParameters intrinsticParams)
  {
    if (points.Count is 0)
      throw new ArgumentException("Require: points count >= 1");
    var displayPoints = new List<Point2f>();
    var worldPoints = new List<Point3f>();
    foreach (var (display, world) in points)
    {
      displayPoints.Add(display);
      worldPoints.Add(world);
    }
    var distMat = intrinsticParams.DistortionCoeffs;
    var cameraMat = intrinsticParams.CameraMatrix;
    var rotationVec = new Mat(3, 1, MatType.CV_64F);
    var translationVec = new Mat(3, 1, MatType.CV_64F);
    Cv2.SolvePnP(
        InputArray.Create(worldPoints),
        InputArray.Create(displayPoints),
        cameraMat, distMat,
        rotationVec, translationVec,
        false, SolvePnPFlags.Iterative);
    var rotationMat = new Mat(3, 3, MatType.CV_64F);
    Cv2.Rodrigues(rotationVec, rotationMat);
    return new ExtrinsicCameraParameters(rotationMat, translationVec);
  }

  public unsafe static ExtrinsicCameraParameters CalibrateWithCorrespondencePoint3D(List<(Point3d, Point3d)> points)
  {
    if (points.Count is 0)
      throw new ArgumentException("Require: points count >= 1");
    using var referenceMat = new Mat(points.Count, 4, MatType.CV_64F, 0);
    using var targetMat = new Mat(points.Count, 4, MatType.CV_64F, 0);
    var rd = (double*)referenceMat.Data;
    var td = (double*)targetMat.Data;
    for (int i = 0; i < points.Count; i++)
    {
      rd[i * 4 + 0] = points[i].Item1.X;
      rd[i * 4 + 1] = points[i].Item1.Y;
      rd[i * 4 + 2] = points[i].Item1.Z;
      rd[i * 4 + 3] = 1;
      td[i * 4 + 0] = points[i].Item2.X;
      td[i * 4 + 1] = points[i].Item2.Y;
      td[i * 4 + 2] = points[i].Item2.Z;
      td[i * 4 + 3] = 1;
    }
    using var refPinv = referenceMat.Inv(DecompTypes.SVD);
    using var m = (Mat)(refPinv * targetMat);
    var md = (double*)m.Data;
    var rotMat = new Mat(3, 3, MatType.CV_64F, new[]
    {
            md[0], md[1], md[2],
            md[4], md[5], md[6],
            md[8], md[9], md[10]
        });
    var trsVec = new Mat(3, 1, MatType.CV_64F, 0);
    trsVec.At<double>(0, 0) = md[3];
    trsVec.At<double>(1, 0) = md[7];
    trsVec.At<double>(2, 0) = md[11];
    return new(rotMat, trsVec);
  }

}
