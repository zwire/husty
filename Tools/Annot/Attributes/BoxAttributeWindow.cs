using System;
using System.IO;
using System.Linq;
using OpenCvSharp;
using Husty.Extensions;
using Husty.Geometry;
using Husty.OpenCvSharp.Extensions;
using Husty.OpenCvSharp.DatasetFormat;

namespace Annot.Attributes;

internal class BoxAttributeWindow : WpfInteractiveCvWindowBase
{

    // ------ fields ------ //

    private readonly int _standardLineWidth;
    private readonly int _boldLineWidth;
    private readonly int _tolerance;
    private Point? _firstPoint = null;
    private SelectedBoxObject? _selected = null;
    private int _cornerIndex; // 0: None, 1: TopLeft, 2: TopRight, 3: BottomLeft, 4: BottomRight
    private Point _prevDragPoint = new(0, 0);

    private record struct SelectedBoxObject(int Id, Rect Box, int Label, Vec2i RelativeVec);


    // ------ constructors ------ //

    public BoxAttributeWindow(
        AnnotationData ann,
        string imagePath,
        int labelIndex,
        int labelCount,
        int standardLineWidth,
        int boldLineWidth,
        int tolerance,
        double windowScale = 1,
        double wheelSpeed = 1
    ) : base(Path.GetFileName(imagePath), Cv2.ImRead(imagePath), ann, labelIndex, labelCount, windowScale, wheelSpeed)
    {
        _standardLineWidth = standardLineWidth;
        _boldLineWidth = boldLineWidth;
        _tolerance = tolerance;
    }


    // ------ public methods ------ //

    public override void ClickDown(Point point)
    {
        _prevDragPoint = point;
        _selected = null;
        _firstPoint = null;
        if (!DrawMode)
        {
            Annotation.GetBoxDatas(ImageId).ForEach(x =>
            {
                var label = x.Value.Label;
                var box = x.Value.Box;
                if (OnBox(point, box))
                {
                    _selected = new(x.Key, box, label, new(point.X - box.Left, point.Y - box.Top));
                    if (new Point2D(box.Left, box.Top).DistanceTo(point.ToHustyPoint2D()) < _tolerance)
                        _cornerIndex = 1;
                    else if (new Point2D(box.Right, box.Top).DistanceTo(point.ToHustyPoint2D()) < _tolerance)
                        _cornerIndex = 2;
                    else if (new Point2D(box.Left, box.Bottom).DistanceTo(point.ToHustyPoint2D()) < _tolerance)
                        _cornerIndex = 3;
                    else if (new Point2D(box.Right, box.Bottom).DistanceTo(point.ToHustyPoint2D()) < _tolerance)
                        _cornerIndex = 4;
                    else
                        _cornerIndex = 0;
                }
            });
        }
        if (_selected is null)
            _firstPoint = point;
    }

    public override void ClickUp(Point point)
    {
        if (DrawMode)
        {
            if (_firstPoint is Point p)
            {
                if (MakeBox(p, point) is Rect box)
                {
                    Annotation.AddBoxData(ImageId, LabelIndex, box, out var id);
                }
            }
            DrawOnce(f =>
            {
                foreach (var (k, v) in Annotation.GetBoxDatas(ImageId))
                    f.Rectangle(v.Box, LabelColors[v.Label], _standardLineWidth);
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
                f.Line(new(point.X - Canvas.Width, point.Y), new(point.X + Canvas.Width, point.Y), new(200, 200, 200), 2);
                f.Line(new(point.X, point.Y - Canvas.Height), new(point.X, point.Y + Canvas.Height), new(200, 200, 200), 2);
            }
            var datas = Annotation.GetBoxDatas(ImageId);
            foreach (var (k, v) in datas)
            {
                if (OnBox(point, v.Box))
                    f.Rectangle(v.Box, LabelColors[v.Label], _boldLineWidth);
                else
                    f.Rectangle(v.Box, LabelColors[v.Label], _standardLineWidth);
            }
            if (_selected is SelectedBoxObject obj)
                f.Rectangle(datas[obj.Id].Box, LabelColors[obj.Label], _boldLineWidth);
        });
    }

    public override void Drag(Point point)
    {
        var diffX = point.X - _prevDragPoint.X;
        var diffY = point.Y - _prevDragPoint.Y;
        _prevDragPoint = point;
        if (_selected is SelectedBoxObject obj)
        {
            int left = 0, top = 0, right = 0, bottom = 0, dx = 0, dy = 0;
            var datas = Annotation.GetBoxDatas(ImageId);
            var w = datas[obj.Id].Box.Width;
            var h = datas[obj.Id].Box.Height;
            if (_cornerIndex is 0)
            {
                left = point.X - obj.RelativeVec.Item0;
                top = point.Y - obj.RelativeVec.Item1;
                right = left + w;
                bottom = top + h;
            }
            else if (_cornerIndex is 1) // top left
            {
                left = point.X - obj.RelativeVec.Item0;
                top = point.Y - obj.RelativeVec.Item1;
                right = obj.Box.Right;
                bottom = obj.Box.Bottom;
            }
            else if (_cornerIndex is 2) // top right
            {
                right = point.X;
                top = point.Y;
                left = obj.Box.Left;
                bottom = obj.Box.Bottom;
            }
            else if (_cornerIndex is 3) // bottom left
            {
                left = point.X;
                bottom = point.Y;
                right = obj.Box.Right;
                top = obj.Box.Top;
            }
            else if (_cornerIndex is 4) // bottom right
            {
                right = point.X;
                bottom = point.Y;
                left = obj.Box.Left;
                top = obj.Box.Top;
            }
            if (MakeBox(new(left, top), new(right, bottom)) is Rect box)
                Annotation.SetBoxData(ImageId, datas[obj.Id].Label, box, obj.Id);
        }
        else if (!DrawMode)
        {
            MoveROI(-diffX, -diffY);
        }
        DrawOnce(f =>
        {
            var datas = Annotation.GetBoxDatas(ImageId);
            foreach (var (k, v) in datas)
                f.Rectangle(v.Box, LabelColors[v.Label], _standardLineWidth);
            if (DrawMode && _firstPoint is Point p)
                f.Rectangle(p, point, LabelColors[LabelIndex], _standardLineWidth);
            if (_selected is SelectedBoxObject obj)
                f.Rectangle(datas[obj.Id].Box, LabelColors[obj.Label], _boldLineWidth);
        });
    }

    public override void Cancel()
    {
        _firstPoint = null;
        _selected = null;
        DrawOnce(f =>
        {
            foreach (var (k, v) in Annotation.GetBoxDatas(ImageId))
                f.Rectangle(v.Box, LabelColors[v.Label], _standardLineWidth);
        });
    }

    public override void DeleteLast()
    {
        var datas = Annotation.GetBoxDatas(ImageId);
        if (datas.Count is 0) return;
        if (_selected?.Id == datas.Count - 1)
            _selected = null;
        Annotation.RemoveAnnotationData(datas.Last().Key);
        DrawOnce(f =>
        {
            foreach (var (k, v) in datas)
                f.Rectangle(v.Box, LabelColors[v.Label], _standardLineWidth);
            if (_selected is SelectedBoxObject obj)
                f.Rectangle(datas[obj.Id].Box, LabelColors[obj.Label], _boldLineWidth);
        });
        SetDrawMode(false);
    }

    public override void DeleteSelected()
    {
        if (_selected is SelectedBoxObject obj)
        {
            _selected = null;
            Annotation.RemoveAnnotationData(obj.Id);
            DrawOnce(f =>
            {
                foreach (var (k, v) in Annotation.GetBoxDatas(ImageId))
                    f.Rectangle(v.Box, LabelColors[v.Label], _standardLineWidth);
            });
        }
        SetDrawMode(false);
    }

    public override void Clear()
    {
        _selected = null;
        _firstPoint = null;
        var ids = Annotation.GetBoxDatas(ImageId).Keys;
        foreach (var id in ids)
            Annotation.RemoveAnnotationData(id);
        ClearCanvas();
    }


    // ------ private methods ------ //

    private bool OnBox(Point p, Rect box)
    {
        var left = box.Left - _tolerance;
        var top = box.Top - _tolerance;
        var right = box.Right + _tolerance;
        var bottom = box.Bottom + _tolerance;
        if (p.X < left || p.Y < top || p.X > right || p.Y > bottom) return false;
        left = box.Left + _tolerance;
        top = box.Top + _tolerance;
        right = box.Right - _tolerance;
        bottom = box.Bottom - _tolerance;
        if (p.X > left && p.Y > top && p.X < right && p.Y < bottom) return false;
        return true;
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
