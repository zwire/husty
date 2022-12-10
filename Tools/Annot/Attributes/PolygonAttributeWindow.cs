using System.IO;
using OpenCvSharp;
using Husty.OpenCvSharp.DatasetFormat;
using Husty.OpenCvSharp.Extensions;

namespace Annot.Attributes;

public class PolygonAttributeWindow : WpfInteractiveCvWindowBase<int>
{

    // ------ fields ------ //

    private readonly List<Point> _points;


    // ------ constructors ------ //

    public PolygonAttributeWindow(
        IEnumerable<AnnotationData> ann,
        string imagePath,
        int labelIndex,
        int labelCount,
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
        labelCount,
        tolerance,
        standardLineWidth,
        boldLineWidth,
        getRatio,
        wheelSpeed
    )
    {
        _points = new();
        DrawOnce(f =>
        {
            var stWidth = GetActualStandardLineWidth();
            foreach (var (k, v) in Annotation.GetPolygonDatas(ImageId))
                DrawPolygons(f, v.Points.FirstOrDefault()!, LabelColors[v.Label], stWidth, true);
        });
    }


    // ------ public methods ------ //

    public override void ClickUp(Point point)
    {
        if (DrawMode)
        {
            _points.Add(point);
            DrawOnce(f =>
            {
                var stWidth = GetActualStandardLineWidth();
                var blWidth = GetActualBoldLineWidth();
                foreach (var (k, v) in Annotation.GetPolygonDatas(ImageId))
                    DrawPolygons(f, v.Points.FirstOrDefault()!, LabelColors[v.Label], stWidth, true);
                if (GetSelected() is SelectedObject obj)
                {
                    var data = Annotation.GetPolygonDatas(ImageId)[obj.Id];
                    DrawPolygons(f, data.Points.FirstOrDefault()!, LabelColors[data.Label], blWidth, true);
                }
                DrawPolygons(f, _points, LabelColors[LabelIndex], stWidth, false);
            });
        }
    }

    public override void Move(Point point)
    {
        DrawOnce(f =>
        {
            var stWidth = GetActualStandardLineWidth();
            var blWidth = GetActualBoldLineWidth();
            if (DrawMode)
            {
                var gw = GetActualGuideLineWidth();
                f.Line(new(point.X - Canvas.Width, point.Y), new(point.X + Canvas.Width, point.Y), LabelColors[LabelIndex], gw);
                f.Line(new(point.X, point.Y - Canvas.Height), new(point.X, point.Y + Canvas.Height), LabelColors[LabelIndex], gw);
                if (_points.Any())
                    f.Line(_points.LastOrDefault()!, point, LabelColors[LabelIndex], stWidth);
            }
            foreach (var (k, v) in Annotation.GetPolygonDatas(ImageId))
                DrawPolygons(f, v.Points.FirstOrDefault()!, LabelColors[v.Label], stWidth, true);
            if (GetSelected() is SelectedObject obj)
            {
                var data = Annotation.GetPolygonDatas(ImageId)[obj.Id];
                DrawPolygons(f, data.Points.FirstOrDefault()!, LabelColors[data.Label], blWidth, true);
            }
            DrawPolygons(f, _points, LabelColors[LabelIndex], stWidth, false);
            if (!DrawMode)
            {
                var nearest = Annotation
                    .GetPolygonDatas(ImageId)
                    .Select(d => (Data: d, Info: GetDistanceFromEachPoint(point, d.Value.Points.FirstOrDefault()!)))
                    .OrderBy(d => d.Info.Dist)
                    .FirstOrDefault();
                if (nearest.Data.Value.Points is not null)
                {
                    var tolerance = GetActualTolerence();
                    if (nearest.Info.Dist < tolerance)
                    {
                        f.Circle(nearest.Data.Value.Points.FirstOrDefault()![nearest.Info.Index], blWidth, LabelColors[nearest.Data.Value.Label], blWidth);
                    }
                }
            }
        });
    }

    public override void Drag(Point point)
    {
        if (GetSelected() is SelectedObject obj)
        {
            var data = Annotation.GetPolygonDatas(ImageId)[obj.Id];
            var pts = data.Points.FirstOrDefault()!;
            pts[obj.Value] = point;
            Annotation.SetPolygonData(ImageId, data.Label, new[] { pts }, obj.Id);
        }
        else if (!DrawMode)
        {
            base.Drag(point);
        }
        DrawOnce(f =>
        {
            var stWidth = GetActualStandardLineWidth();
            var blWidth = GetActualBoldLineWidth();
            foreach (var (k, v) in Annotation.GetPolygonDatas(ImageId))
                DrawPolygons(f, v.Points.FirstOrDefault()!, LabelColors[v.Label], stWidth, true);
            if (GetSelected() is SelectedObject obj)
            {
                var data = Annotation.GetPolygonDatas(ImageId)[obj.Id];
                DrawPolygons(f, data.Points.FirstOrDefault()!, LabelColors[data.Label], blWidth, true);
            }
            DrawPolygons(f, _points, LabelColors[LabelIndex], stWidth, false);
        });
    }

    public override void Cancel()
    {
        if (_points.Count > 2)
        {
            AddHistory(Annotation);
            Annotation.AddPolygonData(ImageId, LabelIndex, _points, out var id);
        }
        _points.Clear();
        DrawOnce(f =>
        {
            var stWidth = GetActualStandardLineWidth();
            foreach (var (k, v) in Annotation.GetPolygonDatas(ImageId))
                DrawPolygons(f, v.Points.FirstOrDefault()!, LabelColors[v.Label], stWidth, true);
        });
        SetDrawMode(false);
    }

    public override void DeleteLast()
    {
        var datas = Annotation.GetPolygonDatas(ImageId);
        if (datas.Count is 0) return;
        if (GetSelected()?.Id == datas.Count - 1)
            SetSelected(null);
        AddHistory(Annotation);
        Annotation.RemoveAnnotationData(datas.Last().Key);
        var stWidth = GetActualStandardLineWidth();
        DrawOnce(f =>
        {
            foreach (var (k, v) in Annotation.GetPolygonDatas(ImageId))
                DrawPolygons(f, v.Points.FirstOrDefault()!, LabelColors[v.Label], stWidth, true);
        });
        _points.Clear();
        SetDrawMode(false);
    }

    public override void DeleteSelected()
    {
        if (GetSelected() is SelectedObject obj)
        {
            SetSelected(null);
            AddHistory(Annotation);
            Annotation.RemoveAnnotationData(obj.Id);
            var stWidth = GetActualStandardLineWidth();
            DrawOnce(f =>
            {
                foreach (var (k, v) in Annotation.GetPolygonDatas(ImageId))
                    DrawPolygons(f, v.Points.FirstOrDefault()!, LabelColors[v.Label], stWidth, true);
            });
        }
        _points.Clear();
        SetDrawMode(false);
    }

    public override void Clear()
    {
        _points.Clear();
        AddHistory(Annotation);
        var ids = Annotation.GetPolygonDatas(ImageId).Keys;
        foreach (var id in ids)
            Annotation.RemoveAnnotationData(id);
        ClearCanvas();
    }


    // ------ protected methods ------ //

    protected override void DoClickDown(Point point)
    {
        SetSelected(null);
        if (!DrawMode && Annotation.GetBoxDatas(ImageId).Count > 0)
        {
            var nearest = Annotation
                .GetPolygonDatas(ImageId)
                .Select(d => (Data: d, Info: GetDistanceFromEachPoint(point, d.Value.Points.FirstOrDefault()!)))
                .OrderBy(d => d.Info.Dist)
                .FirstOrDefault();
            var tolerance = GetActualTolerence();
            if (nearest.Info.Dist < tolerance)
            {
                var label = nearest.Data.Value.Label;
                var points = nearest.Data.Value.Points.FirstOrDefault()!;
                SetSelected(new(nearest.Data.Key, nearest.Info.Index));
                AddHistory(Annotation);
            }
        }
    }


    // ------ private methods ------ //

    private static void DrawPolygons(Mat f, IEnumerable<Point> points, Scalar color, int width, bool closing = false)
    {
        foreach (var p in points)
            f.Circle(p, width, color, width);
        foreach (var (p, q) in GetPairs(points, closing))
            f.Line(p, q, color, width);
        using var back = f.Clone();
        if (points.Count() > 2)
            back.FillConvexPoly(points, color);
        Cv2.AddWeighted(f, 0.6, back, 0.4, 0, f);
    }

    private static (Point, Point)[] GetPairs(IEnumerable<Point> points, bool closing)
    {
        if (points.Count() < 2) return Array.Empty<(Point, Point)>();
        var first = points;
        var second = points.Skip(1);
        if (closing)
            return first.Zip(second).Append((second.LastOrDefault()!, first.FirstOrDefault()!)).ToArray();
        else
            return first.Zip(second).ToArray();
    }

    private static (int Index, int Dist) GetDistanceFromEachPoint(Point p, Point[] points)
    {
        var index = 0;
        var min = int.MaxValue;
        for (int i = 0; i < points.Length; i++)
        {
            var dist = p.DistanceTo(points[i]);
            if (min > dist)
            {
                min = (int)dist;
                index = i;
            }
        }
        return (index, min);
    }

}
