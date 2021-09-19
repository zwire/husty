using System;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using OpenCvSharp;
using Intel.RealSense;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Intel RealSense D415 - 455, L515 C# wrapper
    /// </summary>
    public class Realsense : IDepthCamera
    {

        // ------- Fields ------- //

        private readonly Pipeline _pipeline;
        private readonly Align _align;
        //private readonly DecimationFilter _dfill;
        private readonly DisparityTransform _depthto;
        private readonly DisparityTransform _todepth;
        private readonly SpatialFilter _sfill;
        private readonly TemporalFilter _tfill;
        private readonly HoleFillingFilter _hfill;
        private readonly RealsenseConverter _converter;


        // ------- Properties ------- //

        public double Fps { get; }

        public Size FrameSize { get; }


        // ------- Constructor ------- //

        /// <summary>
        /// Open device
        /// </summary>
        /// <param name="size">Regulated by each device configuration</param>
        /// <param name="fps">0.1 - 50.0</param>
        public Realsense(Size size, double fps = 30,
            bool disparityTransform = true, bool spatialFilter = true, bool temporalFilter = true, bool holeFillingFilter = true)
        {
            var width = size.Width;
            var height = size.Height;
            FrameSize = size;
            _pipeline = new Pipeline();
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, width, height);
            cfg.EnableStream(Stream.Color, Format.Rgb8);
            _pipeline.Start(cfg);
            _align = new(Stream.Depth);
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
            _converter = new(width, height);
            if (fps < 0.1) fps = 0.1;
            if (fps > 50) fps = 50;
            Fps = fps;
        }


        // ------- Methods ------- //

        /// <summary>
        /// Please 'Subscribe', which is a Rx function.
        /// </summary>
        /// <returns>Observable instance contains BgrXyzMat</returns>
        public IObservable<BgrXyzMat> Connect()
        {
            var colorMat = new Mat();
            var pointCloudMat = new Mat();
            var observable = Observable.Interval(TimeSpan.FromMilliseconds(1000 / (int)Fps), ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    GC.Collect();
                    var frames = _pipeline.WaitForFrames();
                    frames = _align.Process(frames).AsFrameSet();
                    using var color = frames.ColorFrame.DisposeWith(frames);
                    Frame depth = frames.DepthFrame.DisposeWith(frames);
                    depth = _depthto?.Process(depth) ?? depth;
                    depth = _sfill?.Process(depth) ?? depth;
                    depth = _tfill?.Process(depth) ?? depth;
                    depth = _todepth?.Process(depth) ?? depth;
                    depth = _hfill?.Process(depth) ?? depth;
                    _converter.ToColorMat(color, ref colorMat);
                    _converter.ToPointCloudMat(depth, ref pointCloudMat);
                    depth.Dispose();
                    frames.Dispose();
                    return new BgrXyzMat(colorMat, pointCloudMat);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        /// <summary>
        /// Close device.
        /// And must not forget 'Dispose' subscribing instance.
        /// </summary>
        public void Disconnect() => _pipeline?.Dispose();

        /// <summary>
        /// (Not recommend) Get current frame synchronously
        /// </summary>
        /// <returns></returns>
        public BgrXyzMat Read()
        {
            var colorMat = new Mat();
            var pointCloudMat = new Mat();
            var frames = _pipeline.WaitForFrames();
            frames = _align.Process(frames).AsFrameSet();
            using var color = frames.ColorFrame.DisposeWith(frames);
            Frame depth = frames.DepthFrame.DisposeWith(frames);
            depth = _depthto?.Process(depth) ?? depth;
            depth = _sfill?.Process(depth) ?? depth;
            depth = _tfill?.Process(depth) ?? depth;
            depth = _todepth?.Process(depth) ?? depth;
            depth = _hfill?.Process(depth) ?? depth;
            _converter.ToColorMat(color, ref colorMat);
            _converter.ToPointCloudMat(depth, ref pointCloudMat);
            depth.Dispose();
            frames.Dispose();
            return new BgrXyzMat(colorMat, pointCloudMat);
        }

    }

}
