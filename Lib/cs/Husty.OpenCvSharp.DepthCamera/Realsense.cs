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
        private readonly bool _alignOn;


        // ------- Properties ------- //

        public Size DepthFrameSize { private set; get; }
        public Size ColorFrameSize { private set; get; }


        // ------- Constructor ------- //

        /// <summary>
        /// Open device
        /// </summary>
        /// <param name="size">Regulated by each device configuration</param>
        public Realsense(Size size, 
            bool disparityTransform = true, bool spatialFilter = true, bool temporalFilter = true, bool holeFillingFilter = true)
        {
            var width = size.Width;
            var height = size.Height;
            DepthFrameSize = size;
            ColorFrameSize = DepthFrameSize;
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
            _converter = new(width, height, width, height);
            _alignOn = true;
        }

        /// <summary>
        /// Open device
        /// </summary>
        /// <param name="colorSize">Regulated by each device configuration</param>
        /// <param name="depthSize">Regulated by each device configuration</param>
        public Realsense(Size colorSize, Size depthSize, 
            bool disparityTransform = true, bool spatialFilter = true, bool temporalFilter = true, bool holeFillingFilter = true)
        {
            var cWidth = colorSize.Width;
            var cHeight = colorSize.Height;
            var dWidth = depthSize.Width;
            var dHeight = depthSize.Height;
            ColorFrameSize = colorSize;
            DepthFrameSize = depthSize;
            _pipeline = new Pipeline();
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, dWidth, dHeight);
            cfg.EnableStream(Stream.Color, cWidth, cHeight, Format.Rgb8);
            _pipeline.Start(cfg);
            _align = new(Stream.Color);
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
            _converter = new(cWidth, cHeight, dWidth, dHeight);
            _alignOn = false;
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
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    GC.Collect();
                    var frames = _pipeline.WaitForFrames();
                    if (_alignOn) frames = _align.Process(frames).AsFrameSet();
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

    }

}
