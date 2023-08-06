using Husty.OpenCvSharp.CameraCalibration;
using OpenCvSharp;

namespace Husty.OpenCvSharp.Transform;

public class PerspectiveTransformer
{

  // 
  // Display Coordinate ... x
  // World Coordinate ... X
  // Rotation Matrix ... R
  // Translation Vector ... T
  // Camera Matrix ... A
  // 
  // It need to calculate scale factor, now naming 's'.
  // 
  // s * x = A * (R * X + T)
  // s = (X + R^-1 * T) / (R^-1 * A^-1 * x)
  // 
  // This equation couldn't be solved generally,
  // but assuming flat ground (z = 0) ...
  // 
  // s = (R^-1 * T) / (R^-1 * A^-1 * x)
  // 
  // detail --> http://www.cyber.t.u-tokyo.ac.jp/~tani/class/mech_enshu/enshu2011mi2.pdf
  //

  private readonly Mat _A;
  private readonly Mat _T;
  private readonly Mat _R;
  private readonly Mat _AInv;
  private readonly Mat _RInv;
  private readonly Mat _Q;
  private readonly Mat _P;

  public PerspectiveTransformer(Mat cameraMatrix, ExtrinsicCameraParameters paramEx)
  {
    if (cameraMatrix.Rows is not 3 || cameraMatrix.Cols is not 3)
      throw new ArgumentException("Requires: 3x3 matrix.", nameof(cameraMatrix));
    _A = cameraMatrix;
    _T = paramEx.TranslationVector;
    _R = paramEx.RotationMatrix;
    _AInv = _A.Inv();
    _RInv = _R.Inv();
    _Q = -_RInv * _T;
    _P = _RInv * _AInv;
  }

  public Point2f[] ConvertToWorldCoordinate(params Point2f[] pts)
  {
    var p00 = _P.At<double>(0, 0);
    var p01 = _P.At<double>(0, 1);
    var p02 = _P.At<double>(0, 2);
    var p10 = _P.At<double>(1, 0);
    var p11 = _P.At<double>(1, 1);
    var p12 = _P.At<double>(1, 2);
    var p20 = _P.At<double>(2, 0);
    var p21 = _P.At<double>(2, 1);
    var p22 = _P.At<double>(2, 2);
    var q0 = _Q.At<double>(0);
    var q1 = _Q.At<double>(1);
    var q2 = _Q.At<double>(2);
    var results = new Point2f[pts.Length];
    Parallel.For(0, pts.Length, i =>
    {
      var s = -q2 / (p20 * pts[i].X + p21 * pts[i].Y + p22);
      var us = pts[i].X * s;
      var vs = pts[i].Y * s;
      var x = p00 * us + p01 * vs + p02 * s + q0;
      var y = p10 * us + p11 * vs + p12 * s + q1;
      results[i] = new((float)x, (float)y);
    });
    return results;
  }

  public Point2f[] ConvertToDisplayCoordinate(params Point2f[] pts)
  {
    var a00 = _A.At<double>(0, 0);
    var a02 = _A.At<double>(0, 2);
    var a11 = _A.At<double>(1, 1);
    var a12 = _A.At<double>(1, 2);
    var r00 = _R.At<double>(0, 0);
    var r01 = _R.At<double>(0, 1);
    var r10 = _R.At<double>(1, 0);
    var r11 = _R.At<double>(1, 1);
    var r20 = _R.At<double>(2, 0);
    var r21 = _R.At<double>(2, 1);
    var t0 = _T.At<double>(0);
    var t1 = _T.At<double>(1);
    var t2 = _T.At<double>(2);
    var results = new Point2f[pts.Length];
    Parallel.For(0, pts.Length, i =>
    {
      var s = r20 * pts[i].X + r21 * pts[i].Y + t2;
      var xs = (r00 * pts[i].X + r01 * pts[i].Y + t0) / s;
      var ys = (r10 * pts[i].X + r11 * pts[i].Y + t1) / s;
      var u = a00 * xs + a02;
      var v = a11 * ys + a12;
      results[i] = new((float)u, (float)v);
    });
    return results;
  }

}
