using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;

namespace Husty.OpenCvSharp.ImageStream;

public sealed class CameraStream : IImageStream<Mat>
{

  // ------ fields ------ //

  private bool _disposed;
  private readonly VideoCapture _cap;
  private readonly ObjectPool<Mat> _pool;


  // ------ properties ------ //

  public int Fps { get; }

  public int Channels { get; }

  public Size FrameSize { get; }

  public IObservable<Mat> ImageSequence { get; }


  // ------ constructors ------ //

  public CameraStream(int src, IEnumerable<Properties> properties = null)
  {
    _cap = new(src);
    _pool = new(2, () => new Mat());
    if (properties is not null)
      foreach (var p in properties)
        _cap.Set(p.Key, p.Value);
    Fps = (int)_cap.Fps;
    Channels = (int)_cap.Get(VideoCaptureProperties.Channel);
    FrameSize = new(_cap.FrameWidth, _cap.FrameHeight);
    var connectable = Observable
        .Repeat(0, new EventLoopScheduler())
        .TakeUntil(_ => _disposed)
        .Select(_ =>
        {
          var frame = _pool.GetObject();
          return _cap.Read(frame) ? frame : null;
        })
        .Where(x => x is not null)
        .Publish();
    connectable.Connect();
    ImageSequence = connectable;
  }


  // ------ public methods ------ //

  public Mat Read()
  {
    while (!_disposed)
      if (ImageSequence.FirstOrDefaultAsync().Wait() is Mat img) return img;
    return null;
  }

  public void Dispose()
  {
    if (_disposed) return;
    _disposed = true;
    _cap?.Dispose();
    _pool?.Dispose();
  }

}
