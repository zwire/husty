using System.Reactive.Subjects;
using System.Text;
using Husty.OpenCvSharp.Extensions;
using OpenCvSharp;

namespace Husty.OpenCvSharp;

public abstract class InteractiveCvWindowBase : IInteractiveWindow
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


  // ------ properties ------ //

  public string Name => _name;

  public double Ratio => _ratio;

  public double WheelSpeed => WheelSpeed;

  public Size WindowSize => _windowSize;

  public Mat Canvas { private set; get; }

  public Rect ROI => _roi;

  public IObservable<string> KeyInvoked => _keySubject;


  // ------ constructors ------ //

  public InteractiveCvWindowBase(
      string name,
      Mat frame,
      double windowScale,
      double wheelSpeed,
      int timeout
  )
  {
    _name = name;
    _windowScale = windowScale;
    _wheelSpeed = wheelSpeed;
    Canvas = frame;
    _originalFrame = frame.Clone();
    _frame = frame.Clone();
    _pool1 = new(1, () => new(_windowSize, MatType.CV_8UC3));
    _pool2 = new(1, () => _frame.Clone());
    _windowSize = new(_frame.Width * windowScale, _frame.Height * windowScale);
    Cv2.Resize(_frame, _frame, _windowSize, 0, 0, InterpolationFlags.Cubic);
    _roi = new(0, 0, _frame.Width, _frame.Height);
    Cv2.ImShow(_name, _frame);
    Crop(_roi.GetCenter(), 1);
    _keySubject = new();
    while (true)
    {
      var key = Cv2.WaitKey(timeout);
      if (key is -1) return;
      var code = Encoding.ASCII.GetString(new[] { (byte)key });
      _keySubject.OnNext(code);
    }
  }


  // ------ public methods ------ //

  public virtual void Dispose()
  {
    _originalFrame.Dispose();
    _frame.Dispose();
    Canvas.Dispose();
  }

  public abstract void ClickDown(Point point);

  public abstract void ClickUp(Point point);

  public abstract void Move(Point point);

  public abstract void Drag(Point point);

  public abstract void Cancel();


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

  protected void Clear()
  {
    Canvas.Dispose();
    Canvas = _originalFrame.Clone();
  }


  // ------ protected methods ------ //

  protected void Show(Mat image)
  {
    var view1 = _pool1.GetObject();
    Cv2.Resize(image, view1, _windowSize, 0, 0, InterpolationFlags.Cubic);
    using var tmp = view1[_roi];
    var view2 = _pool2.GetObject();
    Cv2.Resize(tmp, view2, _frame.Size(), 0, 0, InterpolationFlags.Cubic);
    Cv2.ImShow(_name, view2);
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
    Cv2.SetMouseCallback(_name, (e, x, y, f, _) =>
    {
      x = (int)(x * _ratio + _roi.X);
      y = (int)(y * _ratio + _roi.Y);
      var p = new Point(x, y);
      var realP = new Point(p.X / _windowScale, p.Y / _windowScale);
      switch (e)
      {
        case MouseEventTypes.LButtonDown:
          _lDown = true;
          ClickDown(realP);
          break;
        case MouseEventTypes.LButtonUp:
          if (_lDown)
            ClickUp(realP);
          _lDown = false;
          break;
        case MouseEventTypes.RButtonDown:
          _lDown = false;
          Cancel();
          break;
        case MouseEventTypes.MouseMove:
          if (_lDown)
            Drag(realP);
          else
            Move(realP);
          break;
        case MouseEventTypes.MouseWheel:
          var up = Cv2.GetMouseWheelDelta(f) > 0;
          Crop(p, _ratio * (1 + _wheelSpeed * 0.08 * (up ? -1 : +1)));
          break;
      }
      Show(Canvas);
    });
  }

}
