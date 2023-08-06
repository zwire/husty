using System.IO;
using Husty.Extensions;
using Husty.Geometry;
using Husty.OpenCvSharp.DatasetFormat;
using Husty.OpenCvSharp.Extensions;
using OpenCvSharp;

namespace Annot.Attributes;

internal class CircleAttributeWindow : WpfInteractiveCvWindowBase<object>
{

  // ------ fields ------ //

  private static int _circleSize = 10;
  private Point? _mousePoint;


  // ------ constructors ------ //

  public CircleAttributeWindow(
      IEnumerable<AnnotationData> ann,
      string imagePath,
      int labelIndex,
      int standardLineWidth,
      int boldLineWidth,
      int tolerance,
      Func<double> getRatio,
      double wheelSpeed
  ) : base(
      Path.GetFileName(imagePath),
      Cv2.ImRead(imagePath),
      ann,
      labelIndex,
      tolerance,
      standardLineWidth,
      boldLineWidth,
      getRatio,
      wheelSpeed
  )
  {
    DrawOnce(f =>
    {
      var stWidth = GetActualStandardLineWidth();
      foreach (var (k, v) in Annotation.GetPolygonData(ImageId))
      {
        var (p, d) = GetPolygonCenterAndRadius(v.Points.FirstOrDefault());
        DrawCircle(f, p, d, LabelColors[v.Label], 0.5);
      }
    });
  }


  // ------ public methods ------ //

  public override void ClickUp(Point point)
  {
    if (DrawMode)
    {
      AddHistory(Annotation);
      var circle = new Circle(point.ToHustyPoint2D(), _circleSize)
          .ApproximateAsPointsOnCircle(Angle.FromDegree(10))
          .Select(p => p.ToOpenCvSharpPoint2d().ToPoint())
          .ToArray();
      Annotation.AddPolygonData(ImageId, LabelIndex, circle, out var id);
      DrawOnce(f =>
      {
        foreach (var (k, v) in Annotation.GetPolygonData(ImageId))
        {
          var (p, d) = GetPolygonCenterAndRadius(v.Points.FirstOrDefault());
          DrawCircle(f, p, d, LabelColors[v.Label], 0.5);
        }
        if (GetSelected() is SelectedObject obj)
        {
          var data = Annotation.GetPolygonData(ImageId)[obj.Id];
          var (p, d) = GetPolygonCenterAndRadius(data.Points.FirstOrDefault());
          DrawCircle(f, p, d, LabelColors[data.Label], 0.8);
        }
      });
    }
  }

  public override void Move(Point point)
  {
    _mousePoint = point;
    DrawOnce(f =>
    {
      if (DrawMode)
      {
        var gw = GetActualGuideLineWidth();
        f.Line(new(point.X - Canvas.Width, point.Y), new(point.X + Canvas.Width, point.Y), LabelColors[LabelIndex], gw);
        f.Line(new(point.X, point.Y - Canvas.Height), new(point.X, point.Y + Canvas.Height), LabelColors[LabelIndex], gw);
        DrawCircle(f, point, _circleSize, LabelColors[LabelIndex], 0.5);
      }
      foreach (var (k, v) in Annotation.GetPolygonData(ImageId))
      {
        var (p, d) = GetPolygonCenterAndRadius(v.Points.FirstOrDefault());
        DrawCircle(f, p, d, LabelColors[v.Label], 0.5);
      }
      if (GetSelected() is SelectedObject obj)
      {
        var data = Annotation.GetPolygonData(ImageId)[obj.Id];
        var (p, d) = GetPolygonCenterAndRadius(data.Points.FirstOrDefault());
        DrawCircle(f, p, d, LabelColors[data.Label], 0.8);
      }
      if (!DrawMode)
      {
        var nearest = Annotation
                .GetPolygonData(ImageId)
                .Select(d => (d.Key, d.Value.Label, Value: GetPolygonCenterAndRadius(d.Value.Points.FirstOrDefault())))
                .Select(d => (Data: d, Dist: point.DistanceTo(d.Value.P)))
                .OrderBy(d => d.Dist)
                .FirstOrDefault();
        if (nearest.Dist < nearest.Data.Value.Radius)
        {
          DrawCircle(f, nearest.Data.Value.P, nearest.Data.Value.Radius, LabelColors[nearest.Data.Label], 0.8);
        }
      }
    });
  }

  public override void Drag(Point point)
  {
    _mousePoint = point;
    if (GetSelected() is SelectedObject obj)
    {
      var data = Annotation.GetPolygonData(ImageId)[obj.Id];
      SetSelected(new(obj.Id, null));
      var circle = new Circle(point.ToHustyPoint2D(), _circleSize)
          .ApproximateAsPointsOnCircle(Angle.FromDegree(10))
          .Select(p => p.ToOpenCvSharpPoint2d().ToPoint())
          .ToArray();
      Annotation.SetPolygonData(ImageId, data.Label, circle, obj.Id);
    }
    else if (!DrawMode)
    {
      base.Drag(point);
    }
    DrawOnce(f =>
    {
      foreach (var (k, v) in Annotation.GetPolygonData(ImageId))
      {
        var (p, d) = GetPolygonCenterAndRadius(v.Points.FirstOrDefault());
        DrawCircle(f, p, d, LabelColors[v.Label], 0.5);
      }
      if (GetSelected() is SelectedObject obj)
      {
        var data = Annotation.GetPolygonData(ImageId)[obj.Id];
        var (p, d) = GetPolygonCenterAndRadius(data.Points.FirstOrDefault());
        DrawCircle(f, p, d, LabelColors[data.Label], 0.8);
      }
    });
  }

  public override void Cancel()
  {
    DrawOnce(f =>
    {
      var stWidth = GetActualStandardLineWidth();
      foreach (var (k, v) in Annotation.GetPolygonData(ImageId))
      {
        var (p, d) = GetPolygonCenterAndRadius(v.Points.FirstOrDefault());
        DrawCircle(f, p, d, LabelColors[v.Label], 0.5);
      }
    });
    SetDrawMode(false);
  }

  public override void DeleteLast()
  {
    var datas = Annotation.GetPolygonData(ImageId);
    if (datas.Count is 0) return;
    if (GetSelected()?.Id == datas.Count - 1)
      SetSelected(null);
    AddHistory(Annotation);
    Annotation.RemoveAnnotationData(datas.Last().Key);
    DrawOnce(f =>
    {
      foreach (var (k, v) in Annotation.GetPolygonData(ImageId))
      {
        var (p, d) = GetPolygonCenterAndRadius(v.Points.FirstOrDefault());
        DrawCircle(f, p, d, LabelColors[v.Label], 0.5);
      }
    });
    SetDrawMode(false);
  }

  public override void DeleteSelected()
  {
    if (GetSelected() is SelectedObject obj)
    {
      SetSelected(null);
      AddHistory(Annotation);
      Annotation.RemoveAnnotationData(obj.Id);
      DrawOnce(f =>
      {
        foreach (var (k, v) in Annotation.GetPolygonData(ImageId))
        {
          var (p, d) = GetPolygonCenterAndRadius(v.Points.FirstOrDefault());
          DrawCircle(f, p, d, LabelColors[v.Label], 0.5);
        }
      });
    }
    SetDrawMode(false);
  }

  public override void Clear()
  {
    AddHistory(Annotation);
    var ids = Annotation.GetPolygonData(ImageId).Keys;
    foreach (var id in ids)
      Annotation.RemoveAnnotationData(id);
    ClearCanvas();
  }


  // ------ internal methods ------ //

  public override void AcceptOtherKeyInput(string key)
  {
    if (key.Contains("d1"))
    {
      _circleSize -= 2;
      _circleSize = _circleSize.OrAbove(1);
    }
    else if (key.Contains("d2"))
    {
      _circleSize += 2;
    }
    else
    {
      return;
    }
    if (DrawMode && _mousePoint is Point p)
      DrawOnce(f => DrawCircle(f, p, _circleSize, LabelColors[LabelIndex], 0.5));
  }


  // ------ protected methods ------ //

  protected override void DoClickDown(Point point)
  {
    SetSelected(null);
    var data = Annotation.GetPolygonData(ImageId);
    if (!DrawMode && data.Any())
    {
      var nearest = data
          .Select(d => (d.Key, d.Value.Label, Value: GetPolygonCenterAndRadius(d.Value.Points.FirstOrDefault())))
          .Select(d => (Data: d, Dist: point.DistanceTo(d.Value.P)))
          .OrderBy(d => d.Dist)
          .FirstOrDefault();
      if (nearest.Dist < nearest.Data.Value.Radius)
      {
        SetSelected(new(nearest.Data.Key, null));
        AddHistory(Annotation);
      }
    }
  }


  // ------ private methods ------ //

  private static void DrawCircle(Mat f, Point p, int radius, Scalar color, double density)
  {
    using var back = f.Clone();
    f.Circle(p, radius, color, -1);
    Cv2.AddWeighted(f, density, back, 1 - density, 0, f);
  }

  private static (Point P, int Radius) GetPolygonCenterAndRadius(IEnumerable<Point> points)
  {
    var avgX = points.Select(p => p.X).Average();
    var avgY = points.Select(p => p.Y).Average();
    var p = new Point(avgX, avgY);
    var dist = (int)p.DistanceTo(points.FirstOrDefault());
    return (p, dist);
  }

}
