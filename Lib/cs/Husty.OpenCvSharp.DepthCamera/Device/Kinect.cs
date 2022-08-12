using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp;
using Husty.Geometry;
using Husty.OpenCvSharp.Extensions;

namespace Husty.OpenCvSharp.DepthCamera.Device;

public enum AlignBase { Color, Depth }

/// <summary>
/// Microsoft Azure Kinect C# wrapper
/// </summary>
public sealed class Kinect : IDepthCamera
{

    // ------ fields ------ //

    private readonly AlignBase _align;
    private readonly Microsoft.Azure.Kinect.Sensor.Device _device;
    private readonly Transformation _transformation;
    private readonly Mat _rotationMatrix;
    private readonly ObjectPool<Mat> _bgrPool;
    private readonly ObjectPool<Mat> _xyzPool;
    private readonly ObjectPool<BgrXyzMat> _bgrXyzPool;


    // ------ properties ------ //

    /// <summary>
    /// For device setup (resolution, fps, matching mode etc.)
    /// </summary>
    public DeviceConfiguration Config { get; }

    public int Fps { get; }

    public int Channels { get; } = 6;

    public Size FrameSize { get; }

    public bool HasFrame { private set; get; }


    // ------ constructors ------ //

    /// <summary>
    /// Open device
    /// </summary>
    /// <param name="config">User settings</param>
    public Kinect(DeviceConfiguration config, AlignBase align = AlignBase.Color)
    {
        _align = align;
        using var xRot = Angle.FromDegree(-5.8).ToRotationMatrix(Axis.X);
        using var yRot = Angle.FromDegree(-1.3).ToRotationMatrix(Axis.Y);
        using var zRot = Angle.FromDegree(0).ToRotationMatrix(Axis.Z);
        _rotationMatrix = zRot * yRot * xRot;
        _device = Microsoft.Azure.Kinect.Sensor.Device.Open();
        _device.StartCameras(config);
        _transformation = _device.GetCalibration().CreateTransformation();
        Config = config;
        var ccal = _device.GetCalibration().ColorCameraCalibration;
        var dcal = _device.GetCalibration().DepthCameraCalibration;
        FrameSize = _align switch
        {
            AlignBase.Color => new(ccal.ResolutionWidth, ccal.ResolutionHeight),
            AlignBase.Depth => new(dcal.ResolutionWidth, dcal.ResolutionHeight),
            _               => throw new Exception()
        };
        _bgrPool = new(2, () => new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_8UC3));
        _xyzPool = new(2, () => new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_16UC3));
        _bgrXyzPool = new(2, () => new(
            new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_8UC3), 
            new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_16UC3))
        );
        Fps = config.CameraFPS switch
        {
            FPS.FPS5    => 5,
            FPS.FPS15   => 15,
            FPS.FPS30   => 30,
            _           => -1
        };
    }

    /// <summary>
    /// Open device (default)
    /// </summary>
    public Kinect(AlignBase align = AlignBase.Color)
        : this(new DeviceConfiguration
        {
            ColorFormat = ImageFormat.ColorBGRA32,
            ColorResolution = ColorResolution.R720p,
            DepthMode = DepthMode.NFOV_2x2Binned,
            SynchronizedImagesOnly = true,
            CameraFPS = FPS.FPS30
        }, align) { }


    // ------ public methods ------ //

    public BgrXyzMat Read()
    {
        using var capture = _device.GetCapture();
        if (_align is AlignBase.Color)
        {
            using var colorFrame = capture.Color;
            using var depthFrame = _transformation.DepthImageToColorCamera(capture.Depth);
            using var pointCloudFrame = _transformation.DepthImageToPointCloud(depthFrame, CalibrationDeviceType.Color);
            var frame = _bgrXyzPool.GetObject();
            CopyColorPixels(colorFrame, frame.BGR);
            CopyPointCloudPixels(pointCloudFrame, frame.XYZ);
            HasFrame = true;
            return frame;
        }
        else
        {
            using var colorFrame = _transformation.ColorImageToDepthCamera(capture);
            using var pointCloudFrame = _transformation.DepthImageToPointCloud(capture.Depth);
            var frame = _bgrXyzPool.GetObject();
            CopyColorPixels(colorFrame, frame.BGR);
            CopyPointCloudPixels(pointCloudFrame, frame.XYZ);
            HasFrame = true;
            return frame.Rotate(_rotationMatrix);
        }
    }

    public Mat ReadBgr()
    {
        using var capture = _device.GetCapture();
        using var colorFrame = capture.Color;
        var frame = _bgrPool.GetObject();
        CopyColorPixels(colorFrame, frame);
        HasFrame = true;
        return frame;
    }

    public unsafe Mat ReadXyz()
    {
        using var capture = _device.GetCapture();
        if (_align is AlignBase.Color)
        {
            using var depthFrame = _transformation.DepthImageToColorCamera(capture.Depth);
            using var pointCloudFrame = _transformation.DepthImageToPointCloud(depthFrame, CalibrationDeviceType.Color);
            var frame = _xyzPool.GetObject();
            CopyPointCloudPixels(pointCloudFrame, frame);
            HasFrame = true;
            return frame;
        }
        else
        {
            using var pointCloudFrame = _transformation.DepthImageToPointCloud(capture.Depth);
            var frame = _xyzPool.GetObject();
            CopyPointCloudPixels(pointCloudFrame, frame);
            HasFrame = true;
            var d = (float*)_rotationMatrix.Data;
            var s = (short*)frame.Data;
            for (int i = 0; i < frame.Rows * frame.Cols * 3; i += 3)
            {
                var x = s[i + 0];
                var y = s[i + 1];
                var z = s[i + 2];
                s[i + 0] = (short)(d[0] * x + d[1] * y + d[2] * z);
                s[i + 1] = (short)(d[3] * x + d[4] * y + d[5] * z);
                s[i + 2] = (short)(d[6] * x + d[7] * y + d[8] * z);
            }
            return frame;
        }
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
        _device?.StopCameras();
        _device?.Dispose();
        _bgrXyzPool?.Dispose();
        _bgrPool?.Dispose();
        _xyzPool?.Dispose();
    }


    // ------ private methods ------ //

    private unsafe static void CopyColorPixels(Image colorFrame, Mat colorMat)
    {
        var w = colorFrame.WidthPixels;
        var h = colorFrame.HeightPixels;
        if (colorMat.IsDisposed || colorMat.Width != w || colorMat.Height != h || colorMat.Type() != MatType.CV_8UC3)
            return;
        var m = colorFrame.Memory;
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

    private unsafe static void CopyPointCloudPixels(Image pointCloudFrame, Mat pointCloudMat)
    {
        var w = pointCloudFrame.WidthPixels;
        var h = pointCloudFrame.HeightPixels;
        if (pointCloudMat.IsDisposed || pointCloudMat.Width != w || pointCloudMat.Height != h || pointCloudMat.Type() != MatType.CV_16UC3)
            return;
        var pdAry = pointCloudFrame.GetPixels<Short3>().Span;
        var p = (ushort*)pointCloudMat.Data;
        int index = 0;
        for (int i = 0; i < pdAry.Length; i++)
        {
            p[index++] = (ushort)pdAry[i].X;
            p[index++] = (ushort)pdAry[i].Y;
            p[index++] = (ushort)pdAry[i].Z;
        }
    }

}

