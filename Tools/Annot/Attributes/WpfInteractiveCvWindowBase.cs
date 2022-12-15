using System.Windows.Media.Imaging;
using System.Reactive.Subjects;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Husty;
using Husty.Extensions;
using Husty.OpenCvSharp.DatasetFormat;

namespace Annot.Attributes;

internal abstract class WpfInteractiveCvWindowBase<T> : IWpfInteractiveWindow
{

    public record SelectedObject(int Id, T Value);

    // ------ fields ------ //

    private readonly string _name;
    private readonly List<AnnotationData> _ann;
    private readonly int _imageId;
    private readonly double _wheelSpeed;
    private readonly Mat _originalFrame;
    private readonly Size _windowSize;
    private readonly Mat _frame;
    private readonly ObjectPool<Mat> _pool1;
    private readonly ObjectPool<Mat> _pool2;
    private readonly Subject<string> _keySubject;
    private readonly int _tolerance;
    private readonly Func<double> _getRatio;
    private readonly int _standardLineWidth;
    private readonly int _boldLineWidth;
    private double _ratio = 1;
    private Rect _roi;
    private bool _lDown;
    private int _labelIndex;
    private bool _drawMode;
    private Scalar[] _colors;
    private Point _prevDragPoint = default;
    private Rect _prevDragRoi = default;
    private SelectedObject? _selected = null;


    // ------ properties ------ //

    public string Name => _name;

    public List<AnnotationData> History => _ann;

    public AnnotationData Annotation => _ann.LastOrDefault()!;

    public int ImageId => _imageId;

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
        IEnumerable<AnnotationData> ann,
        int labelIndex,
        int labelCount,
        int tolerance,
        int standardLineWidth,
        int boldLineWidth,
        Func<double> getRatio,
        double wheelSpeed
    )
    {
        _name = name;
        _ann = ann.ToList();
        _ann.LastOrDefault()!.TryGetImageId(name, out _imageId);
        _labelIndex = labelIndex;
        _tolerance = tolerance;
        _standardLineWidth = standardLineWidth;
        _boldLineWidth = boldLineWidth;
        _getRatio = getRatio;
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
        _windowSize = new(_frame.Width, _frame.Height);
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

    public void Back()
    {
        if (_ann.Count > 1)
            _ann.RemoveAt(_ann.Count - 1);
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

    public void ClickDown(Point point)
    {
        _prevDragPoint = new((point.X - _roi.X) / _ratio, (point.Y - _roi.Y) / _ratio);
        _prevDragRoi = _roi;
        DoClickDown(point);
    }

    public abstract void ClickUp(Point point);

    public abstract void Move(Point point);

    public abstract void Cancel();

    public abstract void DeleteLast();

    public abstract void DeleteSelected();

    public abstract void Clear();

    public virtual void Drag(Point point)
    {
        var diffX = (int)(((point.X - _roi.X) / _ratio - _prevDragPoint.X) * _ratio);
        var diffY = (int)(((point.Y - _roi.Y) / _ratio - _prevDragPoint.Y) * _ratio);
        var x = _prevDragRoi.X - diffX;
        var y = _prevDragRoi.Y - diffY;
        var w = _prevDragRoi.Width;
        var h = _prevDragRoi.Height;
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

    public BitmapSource InputMouseWheel(System.Windows.Point p, bool up)
    {
        if (!_lDown)
            Crop(new(p.X, p.Y), _ratio * (1 + _wheelSpeed * 0.08 * (up ? -1 : +1)));
        return GetViewImage();
    }

    public BitmapSource InputLeftMouseDown(System.Windows.Point p)
    {
        var realP = new Point((int)(p.X * _ratio + _roi.X), (int)(p.Y * _ratio + _roi.Y));
        _lDown = true;
        ClickDown(realP);
        return GetViewImage();
    }

    public BitmapSource InputLeftMouseUp(System.Windows.Point p)
    {
        var realP = new Point((int)(p.X * _ratio + _roi.X), (int)(p.Y * _ratio + _roi.Y));
        realP.X = realP.X.InsideOf(0, Canvas.Width - 1);
        realP.Y = realP.Y.InsideOf(0, Canvas.Height - 1);
        if (_lDown)
            ClickUp(realP);
        _lDown = false;
        return GetViewImage();
    }

    public BitmapSource InputRightMouseDown(System.Windows.Point p)
    {
        _lDown = false;
        Cancel();
        return GetViewImage();
    }

    public BitmapSource InputMouseLeave(System.Windows.Point p)
    {
        _lDown = false;
        Cancel();
        return GetViewImage();
    }

    public BitmapSource InputMouseMove(System.Windows.Point p)
    {
        var realP = new Point((int)(p.X * _ratio + _roi.X), (int)(p.Y * _ratio + _roi.Y));
        realP.X = realP.X.InsideOf(0, _frame.Width - 1);
        realP.Y = realP.Y.InsideOf(0, _frame.Height - 1);
        if (_lDown)
            Drag(realP);
        else
            Move(realP);
        return GetViewImage();
    }


    // ------ protected methods ------ //

    protected void AddHistory(AnnotationData ann)
    {
        _ann.Add(ann.Clone());
    }

    protected abstract void DoClickDown(Point point);

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

    protected int GetActualTolerence()
    {
        return (int)(_tolerance * _getRatio()).OrAbove(1);
    }

    protected int GetActualGuideLineWidth()
    {
        return (int)(2 * _getRatio()).OrAbove(1);
    }

    protected int GetActualStandardLineWidth()
    {
        return (int)(_standardLineWidth * _getRatio()).OrAbove(1);
    }

    protected int GetActualBoldLineWidth()
    {
        return (int)(_boldLineWidth * _getRatio()).OrAbove(1);
    }

    protected void SetSelected(SelectedObject? obj)
    {
        _selected = obj;
    }

    protected SelectedObject? GetSelected()
    {
        return _selected;
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
