using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Azure.Kinect.Sensor;
using Reactive.Bindings;
using OpenCvSharp;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Microsoft Azure Kinect C# wrapper
    /// </summary>
    public class Kinect : IImageStream<BgrXyzMat>
    {

        // ------- Fields ------- //

        private readonly AlignBase _align;
        private readonly Device _device;
        private readonly Transformation _transformation;
        private readonly float _pitchRad;
        private readonly float _yawRad;
        private readonly float _rollRad;


        // ------- Properties ------- //

        /// <summary>
        /// For device setup (resolution, fps, matching mode etc.)
        /// </summary>
        public DeviceConfiguration Config { get; }

        public int Fps { get; }

        public int Channels => 6;

        public Size FrameSize { get; }

        public bool HasFrame { private set; get; }

        public ReadOnlyReactivePropertySlim<BgrXyzMat> ReactiveFrame { private set; get; }


        // ------- Constructor ------- //

        /// <summary>
        /// Open device
        /// </summary>
        /// <param name="config">User settings</param>
        public Kinect(DeviceConfiguration config, AlignBase align = AlignBase.Color, float pitchDeg = -5.8f, float yawDeg = -1.3f, float rollDeg = 0f)
        {
            _align = align;
            _pitchRad = (float)(pitchDeg * Math.PI / 180);
            _yawRad = (float)(yawDeg * Math.PI / 180);
            _rollRad = (float)(rollDeg * Math.PI / 180);
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
            ReactiveFrame = Observable
                .Repeat(0, ThreadPoolScheduler.Instance)
                .Select(_ => Read())
                .ToReadOnlyReactivePropertySlim();
        }

        /// <summary>
        /// Open device (default)
        /// </summary>
        public Kinect(AlignBase align = AlignBase.Color, float pitchDeg = -5.8f, float yawDeg = -1.3f, float rollDeg = 0f)
            : this(new DeviceConfiguration
            {
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R720p,
                DepthMode = DepthMode.NFOV_2x2Binned,
                SynchronizedImagesOnly = true,
                CameraFPS = FPS.FPS15
            },
            align, pitchDeg, yawDeg, rollDeg)
        { }


        // ------- Methods ------- //

        public BgrXyzMat Read()
        {
            GC.Collect();
            using var capture = _device.GetCapture();
            if (_align is AlignBase.Color)
            {
                using var colorFrame = capture.Color;
                using var depthFrame = _transformation.DepthImageToColorCamera(capture.Depth);
                using var pointCloudFrame = _transformation.DepthImageToPointCloud(depthFrame, CalibrationDeviceType.Color);
                using var colorMat = colorFrame.ToColorMat();
                using var pointCloudMat = pointCloudFrame.ToPointCloudMat();
                var frame = BgrXyzMat.Create(colorMat, pointCloudMat).Rotate(_pitchRad, _yawRad, _rollRad);
                HasFrame = true;
                return frame.Clone();
            }
            else
            {
                using var colorFrame = _transformation.ColorImageToDepthCamera(capture);
                using var pointCloudFrame = _transformation.DepthImageToPointCloud(capture.Depth);
                using var colorMat = colorFrame.ToColorMat();
                using var pointCloudMat = pointCloudFrame.ToPointCloudMat();
                var frame = BgrXyzMat.Create(colorMat, pointCloudMat).Rotate(_pitchRad, _yawRad, _rollRad);
                HasFrame = true;
                return frame.Clone();
            }
        }

        public void Dispose()
        {
            HasFrame = false;
            ReactiveFrame.Dispose();
            _device?.Dispose();
        }

    }

}
