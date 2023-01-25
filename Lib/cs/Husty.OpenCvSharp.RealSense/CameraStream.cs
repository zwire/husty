using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;
using Intel.RealSense;
using Husty.OpenCvSharp.ImageStream;
using Husty.OpenCvSharp.ThreeDimensionalImaging;
using Stream = Intel.RealSense.Stream;

namespace Husty.OpenCvSharp.RealSense;

public class CameraStream : IImageStream<BgrXyzImage>
{

    // ------ fields ------ //

    private readonly Align _align;
    private readonly Pipeline _pipeline;
    private readonly DisparityTransform _depthto;
    private readonly DisparityTransform _todepth;
    private readonly SpatialFilter _sfill;
    private readonly TemporalFilter _tfill;
    private readonly HoleFillingFilter _hfill;
    private readonly ObjectPool<BgrXyzImage> _pool;


    // ------ properties ------ //

    public int Fps { get; }

    public int Channels => 6;

    public Size FrameSize { get; }

    public bool HasFrame { private set; get; }


    // ------ constructors ------ //

    public CameraStream(Size size, MatchingBase align = MatchingBase.Color, int fps = 30,
        bool disparityTransform = true, bool spatialFilter = true, bool temporalFilter = true, bool holeFillingFilter = true)
    {
        _pool = new(2, () => new(
            new Mat(size.Height, size.Width, MatType.CV_8UC3),
            new Mat(size.Height, size.Width, MatType.CV_16UC1),
            new Mat(size.Height, size.Width, MatType.CV_16UC1),
            new Mat(size.Height, size.Width, MatType.CV_16UC1))
        );
        var width = size.Width;
        var height = size.Height;
        FrameSize = size;
        _pipeline = new Pipeline();
        var cfg = new Config();
        cfg.EnableStream(Stream.Depth, width, height, Format.Z16, fps);
        cfg.EnableStream(Stream.Color, width, height, Format.Bgr8, fps);
        if (!cfg.CanResolve(_pipeline))
            throw new Exception("cannot resolve configuration for the device");
        _pipeline.Start(cfg);
        _align = align switch
        {
            MatchingBase.Color => new(Stream.Color),
            MatchingBase.Depth => new(Stream.Depth),
            _ => throw new Exception()
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

    public BgrXyzImage Read()
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
        var frame = _pool.GetObject();
        CopyColorPixels(color, frame.Bgr);
        CopyPointCloudPixels(depth6, frame.X, frame.Y, frame.Z, color.Width, color.Height);
        HasFrame = true;
        return frame;
    }

    public IObservable<BgrXyzImage> GetStream()
    {
        return Observable
            .Repeat(0, ThreadPoolScheduler.Instance)
            .Select(_ => Read())
            .Where(x => !x.IsDisposed && !x.Empty())
            .Publish().RefCount();
    }

    public void Dispose()
    {
        HasFrame = false;
        _pipeline?.Stop();
        _pipeline?.Dispose();
        _pool?.Dispose();
    }


    // ------ private methods ------ //

    private unsafe static void CopyColorPixels(VideoFrame frame, Mat colorMat)
    {
        if (colorMat.IsDisposed || colorMat.Width != frame.Width || colorMat.Height != frame.Height || colorMat.Type() != MatType.CV_8UC3)
            return;
        frame.CopyTo(colorMat.Data);
    }

    private unsafe static void CopyPointCloudPixels(Frame frame, Mat xMat, Mat yMat, Mat zMat, int width, int height)
    {
        if (
            xMat.IsDisposed || xMat.Width != width || xMat.Height != height || xMat.Type() != MatType.CV_16UC1 ||
            yMat.IsDisposed || yMat.Width != width || yMat.Height != height || yMat.Type() != MatType.CV_16UC1 ||
            zMat.IsDisposed || zMat.Width != width || zMat.Height != height || zMat.Type() != MatType.CV_16UC1
        )
            return;
        using var pdFrame0 = new PointCloud();
        using var pdFrame1 = pdFrame0.Process(frame);
        var pData = (float*)pdFrame1.Data;
        var xp = (ushort*)xMat.Data;
        var yp = (ushort*)yMat.Data;
        var zp = (ushort*)zMat.Data;
        int index = 0;
        for (int i = 0; i < xMat.Width * xMat.Height; i++)
        {
            xp[i] = (ushort)(pData[index++] * 1000);
            yp[i] = (ushort)(pData[index++] * 1000);
            zp[i] = (ushort)(pData[index++] * 1000);
        }
    }

}