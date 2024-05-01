using OpenCvSharp;

namespace Husty.OpenCvSharp.CameraCalibration;

public sealed class Chessboard
{

  private readonly Size _patternSize;
  private readonly float _squareSize;

  /// <summary> Initializes a new instance of the class. </summary>
  /// <param name="patternRows"> The number of inner corners per the chessboard row. </param>
  /// <param name="patternCols"> The number of inner corners per the chessboard column. </param>
  /// <param name="squareSize"> One side length of each square in the chessboard. </param>
  public Chessboard(int patternRows, int patternCols, float squareSize)
  {
    if ((patternRows % 2 is 0 && patternCols % 2 is 0) || (patternRows % 2 is not 0 && patternCols % 2 is not 0))
      throw new ArgumentException("Requires: comination of even & odd.");
    _patternSize = new(patternRows, patternCols);
    _squareSize = squareSize;
  }

  public Point3f[] GetWorldCorners()
  {
    var wc = new List<Point3f>();
    for (int y = 0; y < _patternSize.Height; y++)
      for (int x = 0; x < _patternSize.Width; x++)
        wc.Add(new Point3f(x * _squareSize, y * _squareSize, 0.0f));
    return wc.ToArray();
  }

  public Point2f[] FindCorners(Mat img, ChessboardFlags flags = ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage)
  {
    if (Cv2.FindChessboardCorners(img, _patternSize, out var corners, flags))
    {
      corners = Cv2.CornerSubPix(img, corners, new Size(11, 11), new Size(-1, -1), new TermCriteria(CriteriaTypes.Count | CriteriaTypes.Eps, 30, 0.1));
      return corners;
    }
    else
    {
      throw new Exception("Couldn't detect chessboard!");
    }
  }

  public void DrawCorners(Mat img, Point2f[] corners)
    => Cv2.DrawChessboardCorners(img, _patternSize, corners, true);

}
