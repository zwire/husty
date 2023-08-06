using Husty.Geometry;
using OpenCvSharp;

namespace Husty.OpenCvSharp.Transform;

public sealed class HoughSingleLine
{

  // ------ Fields ------ //

  private readonly int _rhoCount;
  private readonly int _thetaCount;
  private readonly double _xMin;
  private readonly double _xMax;
  private readonly double _yMin;
  private readonly double _yMax;
  private readonly double[] _xArray;
  private readonly double[] _yArray;
  private readonly double[] _thetas;
  private readonly double[] _rhos;
  private readonly double[,,] _rhoTable;


  // ------ Constructors ------ //

  public HoughSingleLine(
      double thetaMin, double thetaMax, double rhoMin, double rhoMax,
      double xMin, double xMax, double yMin, double yMax,
      double thetaStep, double rhoStep,
      double xStep, double yStep
      )
  {

    // calculate required memory size of array and table by division
    var thetaRange = thetaMax - thetaMin;
    _thetaCount = (int)(thetaRange / thetaStep);
    var rhoRange = rhoMax - rhoMin;
    _rhoCount = (int)(rhoRange / rhoStep);
    var xRange = xMax - xMin;
    var width = (int)(xRange / xStep);
    var yRange = yMax - yMin;
    var height = (int)(yRange / yStep);
    _xMin = xMin;
    _xMax = xMax;
    _yMin = yMin;
    _yMax = yMax;

    // make size
    _xArray = new double[width];
    _yArray = new double[height];
    _thetas = new double[_thetaCount];
    _rhos = new double[_rhoCount];
    _rhoTable = new double[height, width, _thetaCount];

    // fill initial values of table
    for (int i = 0; i < _thetaCount; i++)
      _thetas[i] = thetaMin + i * thetaStep;
    for (int i = 0; i < _rhoCount; i++)
      _rhos[i] = rhoMin + i * rhoStep;
    var y = yMin;
    for (int h = 0; h < height; h++, y += yStep)
    {
      _yArray[h] = y + yStep;
      var x = xMin;
      for (int w = 0; w < width; w++, x += xStep)
      {
        _xArray[w] = x + xStep;
        for (int t = 0; t < _thetaCount; t++)
        {
          var rho = x * Math.Cos(_thetas[t]) + y * Math.Sin(_thetas[t]);
          if (rho > rhoMin && rho < rhoMax)
            _rhoTable[h, w, t] = rho;
        }
      }
    }

  }


  // ------ Methods ------ //

  public HoughCoordinateSystem Run(Point[] points)
  {

    // max value and its location to be updated
    var max = 0;
    var maxloc = (0, 0);
    var voteTable = new int[_thetaCount, _rhoCount];

    // make loop by input data
    for (int i = 0; i < points.Length; i++)
    {
      var x = points[i].X;
      var y = points[i].Y;
      if (x > _xMin && x < _xMax && y > _yMin && y < _yMax)
      {

        // search table
        int xIndex;
        int yIndex;
        for (xIndex = 0; xIndex < _xArray.Length; xIndex++)
          if (x < _xArray[xIndex]) break;
        for (yIndex = 0; yIndex < _yArray.Length; yIndex++)
          if (y < _yArray[yIndex]) break;
        for (int t = 0; t < _thetaCount; t++)
        {
          var rho = _rhoTable[yIndex, xIndex, t];
          if (rho is 0) continue;
          for (int r = 0; r < _rhos.Length; r++)
          {
            if (rho < _rhos[r])
            {
              voteTable[t, r]++;
              if (voteTable[t, r] > max)
              {
                max = voteTable[t, r];
                maxloc = (t, r);
              }
              break;
            }
          }
        }
      }
    }
    return new(Angle.FromRadian(_thetas[maxloc.Item1]), _rhos[maxloc.Item2]);

  }
}
