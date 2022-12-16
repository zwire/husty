using System.IO;
using OpenCvSharp;
using Husty.OpenCvSharp.DatasetFormat;

namespace Annot.Attributes;

internal class PointAttributeWindow : WpfInteractiveCvWindowBase<object>
{

    // ------ constructors ------ //

    public PointAttributeWindow(
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
            foreach (var (k, v) in Annotation.GetPointData(ImageId))
                f.Circle(v.Point, stWidth, LabelColors[v.Label], -1);
        });
    }


    // ------ public methods ------ //

    public override void ClickUp(Point point)
    {
        if (DrawMode)
        {
            AddHistory(Annotation);
            Annotation.AddPointData(ImageId, LabelIndex, point, out var id);
            DrawOnce(f =>
            {
                var stWidth = GetActualStandardLineWidth();
                var blWidth = GetActualBoldLineWidth();
                foreach (var (k, v) in Annotation.GetPointData(ImageId))
                    f.Circle(v.Point, stWidth, LabelColors[v.Label], stWidth);
                if (GetSelected() is SelectedObject obj)
                {
                    var data = Annotation.GetPointData(ImageId)[obj.Id];
                    f.Circle(data.Point, blWidth, LabelColors[data.Label], -1);
                }
                f.Circle(point, stWidth, LabelColors[LabelIndex], -1);
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
            }
            foreach (var (k, v) in Annotation.GetPointData(ImageId))
                f.Circle(v.Point, stWidth, LabelColors[v.Label], -1);
            if (GetSelected() is SelectedObject obj)
            {
                var data = Annotation.GetPointData(ImageId)[obj.Id];
                f.Circle(data.Point, blWidth, LabelColors[data.Label], -1);
            }
            if (!DrawMode)
            {
                var nearest = Annotation
                    .GetPointData(ImageId)
                    .Select(d => (Data: d, Dist: point.DistanceTo(d.Value.Point)))
                    .OrderBy(d => d.Dist)
                    .FirstOrDefault();
                var tolerance = GetActualTolerence();
                if (nearest.Dist < tolerance)
                {
                    f.Circle(nearest.Data.Value.Point, blWidth, LabelColors[nearest.Data.Value.Label], -1);
                }
            }
        });
    }

    public override void Drag(Point point)
    {
        if (GetSelected() is SelectedObject obj)
        {
            var data = Annotation.GetPointData(ImageId)[obj.Id];
            SetSelected(new(obj.Id, null));
            Annotation.SetPointData(ImageId, data.Label, point, obj.Id);
        }
        else if (!DrawMode)
        {
            base.Drag(point);
        }
        DrawOnce(f =>
        {
            var stWidth = GetActualStandardLineWidth();
            var blWidth = GetActualBoldLineWidth();
            foreach (var (k, v) in Annotation.GetPointData(ImageId))
                f.Circle(v.Point, stWidth, LabelColors[v.Label], -1);
            if (GetSelected() is SelectedObject obj)
            {
                var data = Annotation.GetPointData(ImageId)[obj.Id];
                f.Circle(data.Point, blWidth, LabelColors[data.Label], -1);
            }
        });
    }

    public override void Cancel()
    {
        DrawOnce(f =>
        {
            var stWidth = GetActualStandardLineWidth();
            foreach (var (k, v) in Annotation.GetPointData(ImageId))
                f.Circle(v.Point, stWidth, LabelColors[v.Label], -1);
        });
        SetDrawMode(false);
    }

    public override void DeleteLast()
    {
        var datas = Annotation.GetPointData(ImageId);
        if (datas.Count is 0) return;
        if (GetSelected()?.Id == datas.Count - 1)
            SetSelected(null);
        AddHistory(Annotation);
        Annotation.RemoveAnnotationData(datas.Last().Key);
        var stWidth = GetActualStandardLineWidth();
        DrawOnce(f =>
        {
            foreach (var (k, v) in Annotation.GetPointData(ImageId))
                f.Circle(v.Point, stWidth, LabelColors[v.Label], -1);
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
            var stWidth = GetActualStandardLineWidth();
            DrawOnce(f =>
            {
                foreach (var (k, v) in Annotation.GetPointData(ImageId))
                    f.Circle(v.Point, stWidth, LabelColors[v.Label], -1);
            });
        }
        SetDrawMode(false);
    }

    public override void Clear()
    {
        AddHistory(Annotation);
        var ids = Annotation.GetPointData(ImageId).Keys;
        foreach (var id in ids)
            Annotation.RemoveAnnotationData(id);
        ClearCanvas();
    }


    // ------ protected methods ------ //

    protected override void DoClickDown(Point point)
    {
        SetSelected(null);
        var data = Annotation.GetPointData(ImageId);
        if (!DrawMode && data.Any())
        {
            var nearest = data
                .Select(d => (Data: d, Dist: point.DistanceTo(d.Value.Point)))
                .OrderBy(d => d.Dist)
                .FirstOrDefault();
            var tolerance = GetActualTolerence();
            if (nearest.Dist < tolerance)
            {
                SetSelected(new(nearest.Data.Key, null));
                AddHistory(Annotation);
            }
        }
    }

}
