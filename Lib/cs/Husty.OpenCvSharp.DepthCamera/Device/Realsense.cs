using System;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using OpenCvSharp;
using Intel.RealSense;
using System.Threading;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Intel RealSense D415 - 455, L515 C# wrapper
    /// </summary>
    public class Realsense : IImageStream<BgrXyzMat>
    {

        // ------- Fields ------- //

        private readonly Align _align;
        private readonly Pipeline _pipeline;
        //private readonly DecimationFilter _dfill;
        private readonly DisparityTransform _depthto;
        private readonly DisparityTransform _todepth;
        private readonly SpatialFilter _sfill;
        private readonly TemporalFilter _tfill;
        private readonly HoleFillingFilter _hfill;


        // ------- Properties ------- //

        public int Fps { get; }

        public int Channels => 6;

        public Size FrameSize { get; }

        public bool HasFrame { private set; get; }


        // ------- Constructor ------- //

        /// <summary>
        /// Open device
        /// </summary>
        /// <param name="size">Regulated by each device configuration</param>
        /// <param name="fps">1 - 50</param>
        public Realsense(Size size, AlignBase align = AlignBase.Color, int fps = 15,
            bool disparityTransform = true, bool spatialFilter = true, bool temporalFilter = true, bool holeFillingFilter = true)
        {
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
                _               => null
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


        // ------- Methods ------- //

        public BgrXyzMat Read()
        {
            GC.Collect();
            Thread.Sleep(1000 / Fps);
            using var frames1 = _pipeline.WaitForFrames();
            using var frames2 = _align.Process(frames1).AsFrameSet();
            using var color = frames2.ColorFrame.DisposeWith(frames2);
            using var depth1 = frames2.DepthFrame.DisposeWith(frames2);
            using var depth2 = _depthto?.Process(depth1) ?? depth1;
            using var depth3 = _sfill?.Process(depth2) ?? depth2;
            using var depth4 = _tfill?.Process(depth3) ?? depth3;
            using var depth5 = _todepth?.Process(depth4) ?? depth4;
            using var depth6 = _hfill?.Process(depth5) ?? depth5;
            using var colorMat = color.ToColorMat();
            using var pointCloudMat = depth6.ToPointCloudMat(color.Width, color.Height);
            var frame = BgrXyzMat.Create(colorMat, pointCloudMat);
            HasFrame = true;
            return frame.Clone();
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
            _pipeline?.Dispose();
        }

    }

}
