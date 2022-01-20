using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Microsoft Azure Kinect C# wrapper
    /// </summary>
    public sealed class Kinect : IImageStream<BgrXyzMat>
    {

        // ------ fields ------ //

        private readonly AlignBase _align;
        private readonly Device _device;
        private readonly Transformation _transformation;
        private readonly Mat _rotationMatrix;


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
            using var xRot = new Angle(-5.8, AngleType.Degree).ToRotationMatrix(Axis.X);
            using var yRot = new Angle(-1.3, AngleType.Degree).ToRotationMatrix(Axis.Y);
            using var zRot = new Angle(0, AngleType.Degree).ToRotationMatrix(Axis.Z);
            _rotationMatrix = zRot * yRot * xRot;
            _device = Device.Open();
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
                CameraFPS = FPS.FPS15
            }, align) { }


        // ------ public methods ------ //

        public BgrXyzMat Read()
        {
            GC.Collect();
            using var capture = _device.GetCapture();
            if (_align is AlignBase.Color)
            {
                using var colorFrame = capture.Color;
                using var depthFrame = _transformation.DepthImageToColorCamera(capture.Depth);
                using var pointCloudFrame = _transformation.DepthImageToPointCloud(depthFrame, CalibrationDeviceType.Color);
                var colorMat = colorFrame.ToColorMat();
                var pointCloudMat = pointCloudFrame.ToPointCloudMat();
                var frame = BgrXyzMat.Create(colorMat, pointCloudMat);
                HasFrame = true;
                return frame;
            }
            else
            {
                using var colorFrame = _transformation.ColorImageToDepthCamera(capture);
                using var pointCloudFrame = _transformation.DepthImageToPointCloud(capture.Depth);
                var colorMat = colorFrame.ToColorMat();
                var pointCloudMat = pointCloudFrame.ToPointCloudMat();
                var frame = BgrXyzMat.Create(colorMat, pointCloudMat).Rotate(_rotationMatrix);
                HasFrame = true;
                return frame;
            }
        }

        public IObservable<BgrXyzMat> GetStream()
        {
            return Observable
                .Repeat(0, ThreadPoolScheduler.Instance)
                .Select(_ => Read())
                .Publish().RefCount();
        }

        public void Dispose()
        {
            HasFrame = false;
            _device?.Dispose();
        }

    }

}
