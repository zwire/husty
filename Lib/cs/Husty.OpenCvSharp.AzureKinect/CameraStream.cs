using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp;
using Husty.OpenCvSharp.ImageStream;
using Husty.OpenCvSharp.ThreeDimensionalImaging;

namespace Husty.OpenCvSharp.AzureKinect;

public class CameraStream : IImageStream<BgrXyzImage>
{

    // ------ fields ------ //

    private readonly MatchingBase _align;
    private readonly Device _device;
    private readonly Transformation _transformation;
    private readonly ObjectPool<BgrXyzImage> _pool;


    // ------ properties ------ //

    public DeviceConfiguration Config { get; }

    public int Fps { get; }

    public int Channels { get; } = 6;

    public Size FrameSize { get; }

    public bool HasFrame { private set; get; }


    // ------ constructors ------ //

    public CameraStream(DeviceConfiguration config, int id = 0, MatchingBase align = MatchingBase.Color)
    {
        _align = align;
        _device = Device.Open(id);
        _device.StartCameras(config);
        _transformation = _device.GetCalibration().CreateTransformation();
        Config = config;
        var ccal = _device.GetCalibration().ColorCameraCalibration;
        var dcal = _device.GetCalibration().DepthCameraCalibration;
        FrameSize = _align switch
        {
            MatchingBase.Color => new(ccal.ResolutionWidth, ccal.ResolutionHeight),
            MatchingBase.Depth => new(dcal.ResolutionWidth, dcal.ResolutionHeight),
            _ => throw new Exception()
        };
        _pool = new(2, () => new(
            new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_8UC3),
            new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_16UC1),
            new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_16UC1),
            new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_16UC1))
        );
        Fps = config.CameraFPS switch
        {
            FPS.FPS5 => 5,
            FPS.FPS15 => 15,
            FPS.FPS30 => 30,
            _ => -1
        };
    }

    public CameraStream(int id = 0, MatchingBase align = MatchingBase.Color)
        : this(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.WFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        }, id, align)
    { }


    // ------ public methods ------ //

    public BgrXyzImage Read()
    {
        using var capture = _device.GetCapture();
        if (_align is MatchingBase.Color)
        {
            using var colorFrame = capture.Color;
            using var depthFrame = _transformation.DepthImageToColorCamera(capture.Depth);
            using var pointCloudFrame = _transformation.DepthImageToPointCloud(depthFrame, CalibrationDeviceType.Color);
            var frame = _pool.GetObject();
            CopyColorPixels(colorFrame, frame.Color);
            CopyPointCloudPixels(pointCloudFrame, frame.X, frame.Y, frame.Z);
            HasFrame = true;
            return frame;
        }
        else
        {
            using var colorFrame = _transformation.ColorImageToDepthCamera(capture);
            using var pointCloudFrame = _transformation.DepthImageToPointCloud(capture.Depth);
            var frame = _pool.GetObject();
            CopyColorPixels(colorFrame, frame.Color);
            CopyPointCloudPixels(pointCloudFrame, frame.X, frame.Y, frame.Z);
            HasFrame = true;
            return frame;
        }
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
        _device?.StopCameras();
        _device?.Dispose();
        _pool?.Dispose();
    }


    // ------ private methods ------ //

    private unsafe static void CopyColorPixels(Image colorFrame, Mat colorMat)
    {
        var w = colorFrame.WidthPixels;
        var h = colorFrame.HeightPixels;
        if (colorMat.IsDisposed || colorMat.Width != w || colorMat.Height != h || colorMat.Type() != MatType.CV_8UC3)
            return;
        var cAry = colorFrame.GetPixels<BGRA>().Span;
        var p = colorMat.DataPointer;
        int index = 0;
        for (int i = 0; i < cAry.Length; i++)
        {
            p[index++] = cAry[i].B;
            p[index++] = cAry[i].G;
            p[index++] = cAry[i].R;
        }
    }

    private unsafe static void CopyPointCloudPixels(Image pointCloudFrame, Mat xMat, Mat yMat, Mat zMat)
    {
        var w = pointCloudFrame.WidthPixels;
        var h = pointCloudFrame.HeightPixels;
        if (
            xMat.IsDisposed || xMat.Width != w || xMat.Height != h || xMat.Type() != MatType.CV_16UC1 ||
            yMat.IsDisposed || yMat.Width != w || yMat.Height != h || yMat.Type() != MatType.CV_16UC1 ||
            zMat.IsDisposed || zMat.Width != w || zMat.Height != h || zMat.Type() != MatType.CV_16UC1
        )
            return;
        var pdAry = pointCloudFrame.GetPixels<Short3>().Span;
        var xp = (ushort*)xMat.Data;
        var yp = (ushort*)yMat.Data;
        var zp = (ushort*)zMat.Data;
        for (int i = 0; i < pdAry.Length; i++)
        {
            xp[i] = (ushort)pdAry[i].X;
            yp[i] = (ushort)pdAry[i].Y;
            zp[i] = (ushort)pdAry[i].Z;
        }
    }
}