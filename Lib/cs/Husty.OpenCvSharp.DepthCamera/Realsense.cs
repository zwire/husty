using System.Reactive.Linq;
using System.Reactive.Concurrency;
using OpenCvSharp;
using Intel.RealSense;
using Stream = Intel.RealSense.Stream;

namespace Husty.OpenCvSharp.DepthCamera;

/// <summary>
/// Intel RealSense D415 - 455, L515 C# wrapper
/// </summary>
public sealed class Realsense : IDepthCamera
{

    // ------ fields ------ //

    private readonly Align _align;
    private readonly Pipeline _pipeline;
    //private readonly DecimationFilter _dfill;
    private readonly DisparityTransform _depthto;
    private readonly DisparityTransform _todepth;
    private readonly SpatialFilter _sfill;
    private readonly TemporalFilter _tfill;
    private readonly HoleFillingFilter _hfill;
    private readonly ObjectPool<Mat> _bgrPool;
    private readonly ObjectPool<Mat> _xyzPool;
    private readonly ObjectPool<BgrXyzMat> _bgrXyzPool;


    // ------ properties ------ //

    public int Fps { get; }

    public int Channels => 6;

    public Size FrameSize { get; }

    public bool HasFrame { private set; get; }


    // ------ constructors ------ //

    /// <summary>
    /// Open device
    /// </summary>
    /// <param name="size">Regulated by each device configuration</param>
    /// <param name="fps">1 - 50</param>
    public Realsense(Size size, AlignBase align = AlignBase.Color, int fps = 30,
        bool disparityTransform = true, bool spatialFilter = true, bool temporalFilter = true, bool holeFillingFilter = true)
    {
        _bgrPool = new(2, () => new Mat(size.Height, size.Width, MatType.CV_8UC3));
        _xyzPool = new(2, () => new Mat(size.Height, size.Width, MatType.CV_16UC3));
        _bgrXyzPool = new(2, () => new(
            new Mat(size.Height, size.Width, MatType.CV_8UC3),
            new Mat(size.Height, size.Width, MatType.CV_16UC3))
        );
        var width = size.Width;
        var height = size.Height;
        FrameSize = size;
        _pipeline = new Pipeline();
        var cfg = new Config();
        cfg.EnableStream(Stream.Depth, width, height, Format.Any, fps);
        cfg.EnableStream(Stream.Color, width, height, Format.Rgb8, fps);
        _pipeline.Start(cfg);
        _align = align switch
        {
            AlignBase.Color => new(Stream.Color),
            AlignBase.Depth => new(Stream.Depth),
            _ => null
        };
        if (disparityTransform)
        {
            _depthto = new(true);
            _todepth = new(false);
        }
        if (spatialFilter)
        {
            _sfill = new();
        }
        if (temporalFilter)
        {
            _tfill = new();
        }
        if (holeFillingFilter)
        {
            _hfill = new();
        }
        if (fps < 1) fps = 1;
        if (fps > 50) fps = 50;
        Fps = fps;
    }


    // ------ public methods ------ //

    public BgrXyzMat Read()
    {
        using var frames1 = _pipeline.WaitForFrames();
        using var frames2 = _align.Process(frames1);
        using var frames3 = frames2.AsFrameSet();
        using var color = frames3.ColorFrame.DisposeWith(frames3);
        using var depth1 = frames3.DepthFrame.DisposeWith(frames3);
        using var depth2 = _depthto?.Process(depth1) ?? depth1;
        using var depth3 = _sfill?.Process(depth2) ?? depth2;
        using var depth4 = _tfill?.Process(depth3) ?? depth3;
        using var depth5 = _todepth?.Process(depth4) ?? depth4;
        using var depth6 = _hfill?.Process(depth5) ?? depth5;
        var frame = _bgrXyzPool.GetObject();
        CopyColorPixels(color, frame.BGR);
        CopyPointCloudPixels(depth6, frame.XYZ, color.Width, color.Height);
        HasFrame = true;
        return frame;
    }

    public Mat ReadBgr()
    {
        using var frames1 = _pipeline.WaitForFrames();
        using var frames2 = _align.Process(frames1);
        using var frames3 = frames2.AsFrameSet();
        using var color = frames3.ColorFrame.DisposeWith(frames3);
        var frame = _bgrPool.GetObject();
        CopyColorPixels(color, frame);
        HasFrame = true;
        return frame;
    }

    public unsafe Mat ReadXyz()
    {
        using var frames1 = _pipeline.WaitForFrames();
        using var frames2 = _align.Process(frames1);
        using var frames3 = frames2.AsFrameSet();
        using var color = frames3.ColorFrame.DisposeWith(frames3);
        using var depth1 = frames3.DepthFrame.DisposeWith(frames3);
        using var depth2 = _depthto?.Process(depth1) ?? depth1;
        using var depth3 = _sfill?.Process(depth2) ?? depth2;
        using var depth4 = _tfill?.Process(depth3) ?? depth3;
        using var depth5 = _todepth?.Process(depth4) ?? depth4;
        using var depth6 = _hfill?.Process(depth5) ?? depth5;
        var frame = _xyzPool.GetObject();
        CopyPointCloudPixels(depth6, frame, color.Width, color.Height);
        HasFrame = true;
        return frame;
    }

    public IObservable<BgrXyzMat> GetStream()
    {
        return Observable
            .Repeat(0, ThreadPoolScheduler.Instance)
            .Select(_ => Read())
            .Where(x => !x.IsDisposed && !x.Empty())
            .Publish().RefCount();
    }

    public IObservable<Mat> GetBgrStream()
    {
        return Observable
            .Repeat(0, ThreadPoolScheduler.Instance)
            .Select(_ => ReadBgr())
            .Where(x => !x.IsDisposed && !x.Empty())
            .Publish().RefCount();
    }

    public IObservable<Mat> GetXyzStream()
    {
        return Observable
            .Repeat(0, ThreadPoolScheduler.Instance)
            .Select(_ => ReadXyz())
            .Where(x => !x.IsDisposed && !x.Empty())
            .Publish().RefCount();
    }

    public void Dispose()
    {
        HasFrame = false;
        _pipeline?.Stop();
        _pipeline?.Dispose();
        _bgrXyzPool?.Dispose();
        _bgrPool?.Dispose();
        _xyzPool?.Dispose();
    }


    // ------ private methods ------ //

    private unsafe static void CopyColorPixels(VideoFrame frame, Mat colorMat)
    {
        if (colorMat.IsDisposed || colorMat.Width != frame.Width || colorMat.Height != frame.Height || colorMat.Type() != MatType.CV_8UC3)
            return;
        frame.CopyTo(colorMat.Data);
        Cv2.CvtColor(colorMat, colorMat, ColorConversionCodes.RGB2BGR);
    }

    private unsafe static void CopyPointCloudPixels(Frame frame, Mat pointCloudMat, int width, int height)
    {
        if (pointCloudMat.IsDisposed || pointCloudMat.Width != width || pointCloudMat.Height != height || pointCloudMat.Type() != MatType.CV_16UC3)
            return;
        using var pdFrame0 = new PointCloud();
        using var pdFrame1 = pdFrame0.Process(frame);
        var pData = (float*)pdFrame1.Data;
        var pixels = (ushort*)pointCloudMat.Data;
        int index = 0;
        for (int i = 0; i < pointCloudMat.Width * pointCloudMat.Height; i++)
        {
            pixels[index] = (ushort)(pData[index++] * 1000);
            pixels[index] = (ushort)(pData[index++] * 1000);
            pixels[index] = (ushort)(pData[index++] * 1000);
        }
    }

}

