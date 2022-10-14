using System;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Reactive.Subjects;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Husty;
using Husty.Extensions;
using Husty.OpenCvSharp;

namespace Annot.Attributes;

public abstract class WpfInteractiveCvWindowBase : IInteractiveWindow
{

    // ------ fields ------ //

    private readonly string _name;
    private readonly double _wheelSpeed;
    private readonly double _windowScale;
    private readonly Mat _originalFrame;
    private readonly Size _windowSize;
    private readonly Mat _frame;
    private readonly ObjectPool<Mat> _pool1;
    private readonly ObjectPool<Mat> _pool2;
    private readonly Subject<string> _keySubject;
    private double _ratio = 1;
    private Rect _roi;
    private bool _lDown;
    private int _labelIndex;
    private bool _drawMode;
    private Scalar[] _colors;


    // ------ properties ------ //

    public string Name => _name;

    public double Ratio => _ratio;

    public double WheelSpeed => WheelSpeed;

    public Size WindowSize => _windowSize;

    public Mat Canvas { private set; get; }

    public Rect ROI => _roi;

    public IObservable<string> KeyInvoked => _keySubject;

    public int LabelIndex => _labelIndex;

    public Scalar[] LabelColors => _colors;

    public bool DrawMode => _drawMode;


    // ------ constructors ------ //

    public WpfInteractiveCvWindowBase(
        string name,
        Mat frame,
        int labelIndex,
        int labelCount,
        double windowScale,
        double wheelSpeed
    )
    {
        _name = name;
        _labelIndex = labelIndex;
        _windowScale = windowScale;
        _wheelSpeed = wheelSpeed;
        _colors = Enumerable
            .Range(0, labelCount)
            .Select(x => new Random(x))
            .Select(x => new Scalar(x.Next(155) + 100, x.Next(155) + 100, x.Next(155) + 100))
            .ToArray();
        Canvas = frame;
        _originalFrame = frame.Clone();
        _frame = frame.Clone();
        _pool1 = new(1, () => new(_windowSize, MatType.CV_8UC3));
        _pool2 = new(1, () => _frame.Clone());
        _windowSize = new(_frame.Width * windowScale, _frame.Height * windowScale);
        Cv2.Resize(_frame, _frame, _windowSize, 0, 0, InterpolationFlags.Cubic);
        _roi = new(0, 0, _frame.Width, _frame.Height);
        _keySubject = new();
    }


    // ------ public methods ------ //

    public virtual void Dispose()
    {
        _originalFrame.Dispose();
        _frame.Dispose();
        Canvas.Dispose();
    }

    public void SetLabelIndex(int index)
    {
        _labelIndex = index;
    }

    public void SetDrawMode(bool on)
    {
        _drawMode = on;
    }

    public BitmapSource GetViewImage()
    {
        var view1 = _pool1.GetObject();
        Cv2.Resize(Canvas, view1, _windowSize, 0, 0, InterpolationFlags.Cubic);
        using var tmp = view1[_roi];
        var view2 = _pool2.GetObject();
        Cv2.Resize(tmp, view2, _frame.Size(), 0, 0, InterpolationFlags.Cubic);
        return view2.ToBitmapSource();
    }

    public abstract void ClickDown(Point point);

    public abstract void ClickUp(Point point);

    public abstract void Move(Point point);

    public abstract void Drag(Point point);

    public abstract void Cancel();

    public abstract void DeleteLast();

    public abstract void DeleteSelected();

    public abstract void Clear();

    public abstract string GetLabelData();


    // ------ protected methods ------ //

    protected void DrawOnce(Action<Mat> action)
    {
        var f = _originalFrame.Clone();
        action(f);
        Canvas.Dispose();
        Canvas = f;
    }

    protected void DrawOnce(Func<Mat, Mat> func)
    {
        var f = func(_originalFrame.Clone());
        Canvas.Dispose();
        Canvas = f;
    }

    protected void Draw(Action<Mat> action)
    {
        action(Canvas);
    }

    protected void Draw(Func<Mat, Mat> func)
    {
        Canvas = func(Canvas);
    }

    protected void ClearCanvas()
    {
        Canvas.Dispose();
        Canvas = _originalFrame.Clone();
    }

    protected void MoveROI(int x, int y)
    {
        x += _roi.X;
        y += _roi.Y;
        var w = _roi.Width;
        var h = _roi.Height;
        if (x < 0)
            x = 0;
        if (y < 0)
            y = 0;
        if (x + w > _frame.Width - 1)
            x = _frame.Width - w;
        if (y + h > _frame.Height - 1)
            y = _frame.Height - h;
        _roi = new(x, y, w, h);
    }


    // ------ internal methods ------ //

    internal BitmapSource InputMouseWheel(System.Windows.Point p, bool up)
    {
        Crop(new(p.X, p.Y), _ratio * (1 + _wheelSpeed * 0.08 * (up ? -1 : +1)));
        return GetViewImage();
    }

    internal BitmapSource InputLeftMouseDown(System.Windows.Point p)
    {
        var realP = new Point((int)(p.X * _ratio + _roi.X / _windowScale), (int)(p.Y * _ratio + _roi.Y / _windowScale));
        _lDown = true;
        ClickDown(realP);
        return GetViewImage();
    }

    internal BitmapSource InputLeftMouseUp(System.Windows.Point p)
    {
        var realP = new Point((int)(p.X * _ratio + _roi.X / _windowScale), (int)(p.Y * _ratio + _roi.Y / _windowScale));
        realP.X = realP.X.InsideOf(0, Canvas.Width - 1);
        realP.Y = realP.Y.InsideOf(0, Canvas.Height - 1);
        if (_lDown)
            ClickUp(realP);
        _lDown = false;
        return GetViewImage();
    }

    internal BitmapSource InputRightMouseDown(System.Windows.Point p)
    {
        _lDown = false;
        Cancel();
        return GetViewImage();
    }

    internal BitmapSource InputMouseLeave(System.Windows.Point p)
    {
        _lDown = false;
        Cancel();
        return GetViewImage();
    }

    internal BitmapSource InputMouseMove(System.Windows.Point p)
    {
        var realP = new Point((int)(p.X * _ratio + _roi.X / _windowScale), (int)(p.Y * _ratio + _roi.Y / _windowScale));
        realP.X = realP.X.InsideOf(0, Canvas.Width - 1);
        realP.Y = realP.Y.InsideOf(0, Canvas.Height - 1);
        if (_lDown)
            Drag(realP);
        else
            Move(realP);
        return GetViewImage();
    }


    // ------ private methods ------ //

    private void Crop(Point center, double ratio)
    {
        var relX = (double)(center.X - _roi.X) / _roi.Width;
        var relY = (double)(center.Y - _roi.Y) / _roi.Height;
        var w = (int)(_frame.Width * ratio);
        var h = (int)(_frame.Height * ratio);
        var x = (int)(_roi.X + _frame.Width * relX * (_ratio - ratio));
        var y = (int)(_roi.Y + _frame.Width * relY * (_ratio - ratio));
        if (x < 0)
            x = 0;
        if (y < 0)
            y = 0;
        if (x + w > _frame.Width - 1)
            x = _frame.Width - w;
        if (y + h > _frame.Height - 1)
            y = _frame.Height - h;
        if (w < 50 || h < 50)
        {
            return;
        }
        if (w > _frame.Width || h > _frame.Height)
        {
            ratio = 1;
            x = 0;
            y = 0;
            w = _frame.Width;
            h = _frame.Height;
        }
        _ratio = ratio;
        _roi = new(x, y, w, h);
    }

}
