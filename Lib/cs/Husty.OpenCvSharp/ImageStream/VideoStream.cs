using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;

namespace Husty.OpenCvSharp.ImageStream;

public sealed class VideoStream : IVideoStream<Mat>
{

    // ------ fields ------ //

    private bool _disposed;
    private int _positionIndex;
    private readonly VideoCapture _cap;
    private readonly ObjectPool<Mat> _pool;


    // ------ properties ------ //

    public int Fps { get; }

    public int Channels { get; }

    public Size FrameSize { get; }

    public int FrameCount { get; }

    public IObservable<Mat> ImageSequence { get; }

    public int CurrentPosition => _positionIndex;

    public bool IsEnd => _positionIndex >= FrameCount - 1;


    // ------ constructors ------ //

    public VideoStream(string src, IEnumerable<Properties> properties = null)
    {
        _cap = new(src);
        _pool = new(2, () => new Mat());
        if (properties is not null)
            foreach (var p in properties)
                _cap.Set(p.Key, p.Value);
        Fps = (int)_cap.Fps;
        Channels = (int)_cap.Get(VideoCaptureProperties.Channel);
        FrameSize = new(_cap.FrameWidth, _cap.FrameHeight);
        FrameCount = _cap.FrameCount;
        var connectable = Observable
            .Repeat(0, ThreadPoolScheduler.Instance)
            .TakeUntil(_ => _disposed)
            .Where(_ => !IsEnd)
            .Select(_ =>
            {
                if (IsEnd) return null;
                _cap.Set(VideoCaptureProperties.PosFrames, _positionIndex++);
                var frame = _pool.GetObject();
                Task.Delay(1000 / Fps).Wait();
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

    public void Seek(int position)
    {
        if (position > -1 && position < FrameCount) _positionIndex = position;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cap?.Dispose();
        _pool?.Dispose();
    }

}
