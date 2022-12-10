using System.IO;
using OpenCvSharp;
using Husty.Geometry;
using Husty.OpenCvSharp.Extensions;
using Husty.OpenCvSharp.DatasetFormat;

namespace Annot.Attributes;

internal class BoxAttributeWindow : WpfInteractiveCvWindowBase<(Rect Box, int CornerIndex, Vec2i RelativeVec)>
{

    // ------ fields ------ //

    private Point? _firstPoint = null;


    // ------ constructors ------ //

    public BoxAttributeWindow(
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
        var stWidth = GetActualStandardLineWidth();
        DrawOnce(f =>
        {
            var datas = Annotation.GetBoxDatas(ImageId);
            foreach (var (k, v) in datas)
                f.Rectangle(v.Box, LabelColors[v.Label], stWidth);
        });
    }


    // ------ public methods ------ //

    public override void ClickUp(Point point)
    {
        if (DrawMode)
        {
            if (_firstPoint is Point p)
            {
                if (MakeBox(p, point) is Rect box)
                {
                    AddHistory(Annotation);
                    Annotation.AddBoxData(ImageId, LabelIndex, box, out var id);
                }
            }
            var stWidth = GetActualStandardLineWidth();
            var blWidth = GetActualBoldLineWidth();
            DrawOnce(f =>
            {
                foreach (var (k, v) in Annotation.GetBoxDatas(ImageId))
                    f.Rectangle(v.Box, LabelColors[v.Label], stWidth);

                if (GetSelected() is SelectedObject obj)
                {
                    var data = Annotation.GetBoxDatas(ImageId)[obj.Id];
                    f.Rectangle(data.Box, LabelColors[data.Label], blWidth);
                }
            });
            SetDrawMode(false);
        }
    }

    public override void Move(Point point)
    {
        DrawOnce(f =>
        {
            if (DrawMode)
            {
                var gw = GetActualGuideLineWidth();
                f.Line(new(point.X - Canvas.Width, point.Y), new(point.X + Canvas.Width, point.Y), LabelColors[LabelIndex], gw);
                f.Line(new(point.X, point.Y - Canvas.Height), new(point.X, point.Y + Canvas.Height), LabelColors[LabelIndex], gw);
            }
            var datas = Annotation
                .GetBoxDatas(ImageId)
                .OrderBy(d => GetDistanceFromEdge(point, d.Value.Box))
                .ToDictionary(x => x.Key, x => x.Value);
            var nearest = datas.FirstOrDefault().Value;
            var stWidth = GetActualStandardLineWidth();
            var blWidth = GetActualBoldLineWidth();
            foreach (var (k, v) in datas)
                f.Rectangle(v.Box, LabelColors[v.Label], stWidth);
            if (!DrawMode && GetDistanceFromEdge(point, nearest.Box) < GetActualTolerence())
                f.Rectangle(nearest.Box, LabelColors[nearest.Label], blWidth);
            if (GetSelected() is SelectedObject obj)
                f.Rectangle(datas[obj.Id].Box, LabelColors[datas[obj.Id].Label], blWidth);
        });
    }

    public override void Drag(Point point)
    {
        if (GetSelected() is SelectedObject obj)
        {
            int left = 0, top = 0, right = 0, bottom = 0, dx = 0, dy = 0;
            var data = Annotation.GetBoxDatas(ImageId)[obj.Id];
            var w = data.Box.Width;
            var h = data.Box.Height;
            if (obj.Value.CornerIndex is 0)
            {
                left = point.X - obj.Value.RelativeVec.Item0;
                top = point.Y - obj.Value.RelativeVec.Item1;
                right = left + w;
                bottom = top + h;
            }
            else if (obj.Value.CornerIndex is 1) // top left
            {
                left = point.X - obj.Value.RelativeVec.Item0;
                top = point.Y - obj.Value.RelativeVec.Item1;
                right = obj.Value.Box.Right;
                bottom = obj.Value.Box.Bottom;
            }
            else if (obj.Value.CornerIndex is 2) // top right
            {
                right = point.X;
                top = point.Y;
                left = obj.Value.Box.Left;
                bottom = obj.Value.Box.Bottom;
            }
            else if (obj.Value.CornerIndex is 3) // bottom left
            {
                left = point.X;
                bottom = point.Y;
                right = obj.Value.Box.Right;
                top = obj.Value.Box.Top;
            }
            else if (obj.Value.CornerIndex is 4) // bottom right
            {
                right = point.X;
                bottom = point.Y;
                left = obj.Value.Box.Left;
                top = obj.Value.Box.Top;
            }
            if (MakeBox(new(left, top), new(right, bottom)) is Rect box)
            {
                Annotation.SetBoxData(ImageId, data.Label, box, obj.Id);
            }
        }
        else if (!DrawMode)
        {
            base.Drag(point);
        }
        var stWidth = GetActualStandardLineWidth();
        var blWidth = GetActualBoldLineWidth();
        DrawOnce(f =>
        {
            var datas = Annotation.GetBoxDatas(ImageId);
            foreach (var (k, v) in datas)
                f.Rectangle(v.Box, LabelColors[v.Label], stWidth);
            if (DrawMode && _firstPoint is Point p)
                f.Rectangle(p, point, LabelColors[LabelIndex], stWidth);
            if (GetSelected() is SelectedObject obj)
                f.Rectangle(datas[obj.Id].Box, LabelColors[datas[obj.Id].Label], blWidth);
        });
    }

    public override void Cancel()
    {
        _firstPoint = null;
        SetSelected(null);
        var stWidth = GetActualStandardLineWidth();
        DrawOnce(f =>
        {
            foreach (var (k, v) in Annotation.GetBoxDatas(ImageId))
                f.Rectangle(v.Box, LabelColors[v.Label], stWidth);
        });
    }

    public override void DeleteLast()
    {
        var datas = Annotation.GetBoxDatas(ImageId);
        if (datas.Count is 0) return;
        if (GetSelected()?.Id == datas.Count - 1)
            SetSelected(null);
        AddHistory(Annotation);
        Annotation.RemoveAnnotationData(datas.Last().Key);
        var stWidth = GetActualStandardLineWidth();
        var blWidth = GetActualBoldLineWidth();
        DrawOnce(f =>
        {
            foreach (var (k, v) in datas)
                f.Rectangle(v.Box, LabelColors[v.Label], stWidth);
            if (GetSelected() is SelectedObject obj)
                f.Rectangle(datas[obj.Id].Box, LabelColors[datas[obj.Id].Label], blWidth);
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
                foreach (var (k, v) in Annotation.GetBoxDatas(ImageId))
                    f.Rectangle(v.Box, LabelColors[v.Label], stWidth);
            });
        }
        SetDrawMode(false);
    }

    public override void Clear()
    {
        SetSelected(null);
        _firstPoint = null;
        AddHistory(Annotation);
        var ids = Annotation.GetBoxDatas(ImageId).Keys;
        foreach (var id in ids)
            Annotation.RemoveAnnotationData(id);
        ClearCanvas();
    }


    // ------ protected methods ------ //

    protected override void DoClickDown(Point point)
    {
        SetSelected(null);
        _firstPoint = null;
        if (!DrawMode && Annotation.GetBoxDatas(ImageId).Count > 0)
        {
            var nearest = Annotation
                .GetBoxDatas(ImageId)
                .OrderBy(d => GetDistanceFromEdge(point, d.Value.Box))
                .FirstOrDefault();
            var tolerance = GetActualTolerence();
            if (GetDistanceFromEdge(point, nearest.Value.Box) < tolerance)
            {
                var box = nearest.Value.Box;
                // 0: None, 1: TopLeft, 2: TopRight, 3: BottomLeft, 4: BottomRight
                var corner = 0;
                if (new Point2D(box.Left, box.Top).DistanceTo(point.ToHustyPoint2D()) < tolerance)
                    corner = 1;
                else if (new Point2D(box.Right, box.Top).DistanceTo(point.ToHustyPoint2D()) < tolerance)
                    corner = 2;
                else if (new Point2D(box.Left, box.Bottom).DistanceTo(point.ToHustyPoint2D()) < tolerance)
                    corner = 3;
                else if (new Point2D(box.Right, box.Bottom).DistanceTo(point.ToHustyPoint2D()) < tolerance)
                    corner = 4;
                SetSelected(new(nearest.Key, (box, corner, new(point.X - box.Left, point.Y - box.Top))));
                AddHistory(Annotation);
            }
        }
        if (GetSelected() is null)
            _firstPoint = point;
    }


    // ------ private methods ------ //

    private static int GetDistanceFromEdge(Point p, Rect box)
    {
        if (p.X >= box.X && p.X <= box.X + box.Width)
        {
            return Math.Min(Math.Abs(p.Y - box.Y), Math.Abs(p.Y - box.Y - box.Height));
        }
        else if (p.Y >= box.Y && p.Y <= box.Y + box.Height)
        {
            return Math.Min(Math.Abs(p.X - box.X), Math.Abs(p.X - box.X - box.Width));
        }
        else
        {
            return (int)Math.Sqrt(new[]
            {
                Math.Pow(p.X - box.X, 2) + Math.Pow(p.Y - box.Y, 2),
                Math.Pow(p.X - box.X - box.Width, 2) + Math.Pow(p.Y - box.Y, 2),
                Math.Pow(p.X - box.X, 2) + Math.Pow(p.Y - box.Y - box.Height, 2),
                Math.Pow(p.X - box.X - box.Width, 2) + Math.Pow(p.Y - box.Y - box.Height, 2)
            }.Min());
        }
    }

    private Rect? MakeBox(Point p0, Point p1)
    {
        var left = Math.Min(p0.X, p1.X);
        var top = Math.Min(p0.Y, p1.Y);
        var right = Math.Max(p0.X, p1.X);
        var bottom = Math.Max(p0.Y, p1.Y);
        var w = right - left;
        var h = bottom - top;
        if (w > 0 && h > 0 && left >= 0 && top >= 0 && right < Canvas.Width && bottom < Canvas.Height)
        {
            return new Rect(left, top, right - left, bottom - top);
        }
        return null;
    }

}
