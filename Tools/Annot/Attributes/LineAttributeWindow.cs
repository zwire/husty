using System.IO;
using OpenCvSharp;
using Husty.OpenCvSharp.DatasetFormat;

namespace Annot.Attributes;

internal class LineAttributeWindow : WpfInteractiveCvWindowBase<int>
{

    // ------ fields ------ //

    private Point? _firstPoint;


    // ------ constructors ------ //

    public LineAttributeWindow(
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
            foreach (var (k, v) in Annotation.GetLineData(ImageId))
                f.Line(v.P1, v.P2, LabelColors[v.Label], stWidth);
        });
    }


    // ------ public methods ------ //

    public override void ClickUp(Point point)
    {
        if (DrawMode)
        {
            if (_firstPoint is Point p)
            {
                AddHistory(Annotation);
                Annotation.AddLineData(ImageId, LabelIndex, p, point, out var id);
                _firstPoint = null;
            }
            else
            {
                _firstPoint = point;
            }
            DrawOnce(f =>
            {
                var stWidth = GetActualStandardLineWidth();
                var blWidth = GetActualBoldLineWidth();
                foreach (var (k, v) in Annotation.GetLineData(ImageId))
                    f.Line(v.P1, v.P2, LabelColors[v.Label], stWidth);
                if (GetSelected() is SelectedObject obj)
                {
                    var data = Annotation.GetLineData(ImageId)[obj.Id];
                    f.Line(data.P1, data.P2, LabelColors[data.Label], blWidth);
                }
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
                if (_firstPoint is Point p)
                    f.Line(p, point, LabelColors[LabelIndex], stWidth);
            }
            foreach (var (k, v) in Annotation.GetLineData(ImageId))
                f.Line(v.P1, v.P2, LabelColors[v.Label], stWidth);
            if (GetSelected() is SelectedObject obj)
            {
                var data = Annotation.GetLineData(ImageId)[obj.Id];
                f.Line(data.P1, data.P2, LabelColors[data.Label], blWidth);
            }
            if (!DrawMode)
            {
                var nearest = Annotation
                    .GetLineData(ImageId)
                    .Select(d => (Data: d, Info: GetDistanceFromEachPoint(point, new[] { d.Value.P1, d.Value.P2 })))
                    .OrderBy(d => d.Info.Dist)
                    .FirstOrDefault();
                var tolerance = GetActualTolerence();
                if (nearest.Info.Dist < tolerance)
                {
                    var p = nearest.Info.Index is 0 ? nearest.Data.Value.P1 : nearest.Data.Value.P2;
                    f.Circle(p, blWidth, LabelColors[nearest.Data.Value.Label], blWidth);
                }
            }
        });
    }

    public override void Drag(Point point)
    {
        if (GetSelected() is SelectedObject obj)
        {
            var data = Annotation.GetLineData(ImageId)[obj.Id];
            var p1 = data.P1;
            var p2 = data.P2;
            if (obj.Value is 0) p1 = point;
            else if (obj.Value is 1) p2 = point;
            Annotation.SetLineData(ImageId, data.Label, p1, p2, obj.Id);
        }
        else if (!DrawMode)
        {
            base.Drag(point);
        }
        DrawOnce(f =>
        {
            var stWidth = GetActualStandardLineWidth();
            var blWidth = GetActualBoldLineWidth();
            foreach (var (k, v) in Annotation.GetLineData(ImageId))
                f.Line(v.P1, v.P2, LabelColors[v.Label], stWidth);
            if (GetSelected() is SelectedObject obj)
            {
                var data = Annotation.GetLineData(ImageId)[obj.Id];
                f.Line(data.P1, data.P2, LabelColors[data.Label], blWidth);
            }
        });
    }

    public override void Cancel()
    {
        _firstPoint = null;
        DrawOnce(f =>
        {
            var stWidth = GetActualStandardLineWidth();
            foreach (var (k, v) in Annotation.GetLineData(ImageId))
                f.Line(v.P1, v.P2, LabelColors[v.Label], stWidth);
        });
        SetDrawMode(false);
    }

    public override void DeleteLast()
    {
        var data = Annotation.GetLineData(ImageId);
        if (data.Count is 0) return;
        if (GetSelected()?.Id == data.Count - 1)
            SetSelected(null);
        AddHistory(Annotation);
        Annotation.RemoveAnnotationData(data.Last().Key);
        var stWidth = GetActualStandardLineWidth();
        DrawOnce(f =>
        {
            foreach (var (k, v) in Annotation.GetLineData(ImageId))
                f.Line(v.P1, v.P2, LabelColors[v.Label], stWidth);
        });
        _firstPoint = null;
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
                foreach (var (k, v) in Annotation.GetLineData(ImageId))
                    f.Line(v.P1, v.P2, LabelColors[v.Label], stWidth);
            });
        }
        _firstPoint = null;
        SetDrawMode(false);
    }

    public override void Clear()
    {
        _firstPoint = null;
        AddHistory(Annotation);
        var ids = Annotation.GetLineData(ImageId).Keys;
        foreach (var id in ids)
            Annotation.RemoveAnnotationData(id);
        ClearCanvas();
    }


    // ------ protected methods ------ //

    protected override void DoClickDown(Point point)
    {
        SetSelected(null);
        var data = Annotation.GetLineData(ImageId);
        if (!DrawMode && data.Any())
        {
            var nearest = data
                .Select(d => (Data: d, Info: GetDistanceFromEachPoint(point, new[] { d.Value.P1, d.Value.P2 })))
                .OrderBy(d => d.Info.Dist)
                .FirstOrDefault();
            var tolerance = GetActualTolerence();
            if (nearest.Info.Dist < tolerance)
            {
                SetSelected(new(nearest.Data.Key, nearest.Info.Index));
                AddHistory(Annotation);
            }
        }
    }


    // ------ private methods ------ //

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
