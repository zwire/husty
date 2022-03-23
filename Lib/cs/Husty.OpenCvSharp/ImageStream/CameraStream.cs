using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;

namespace Husty.OpenCvSharp.ImageStream
{
    public sealed class CameraStream : IImageStream<Mat>
    {

        // ------ fields ------ //

        private readonly VideoCapture _cap;
        private readonly ObjectPool<Mat> _pool;


        // ------ properties ------ //

        public int Fps { get; }

        public int Channels { get; }

        public Size FrameSize { get; }

        public bool HasFrame { private set; get; }


        // ------ constructors ------ //

        public CameraStream(int src, IEnumerable<Properties> properties = null)
        {
            _cap = new(src);
            _pool = new(2);
            if (properties is not null)
                foreach (var p in properties)
                    _cap.Set(p.Key, p.Value);
            Fps = (int)_cap.Fps;
            Channels = (int)_cap.Get(VideoCaptureProperties.Channel);
            FrameSize = new(_cap.FrameWidth, _cap.FrameHeight);
        }


        // ------ public methods ------ //

        public Mat Read()
        {
            var frame = _pool.GetObject();
            HasFrame = _cap.Read(frame);
            if (HasFrame)
                return frame;
            else
                return null;
        }

        public IObservable<Mat> GetStream()
        {
            return Observable
                .Repeat(0, ThreadPoolScheduler.Instance)
                .Select(_ => Read())
                .Publish().RefCount();
        }

        public void Dispose()
        {
            HasFrame = false;
            _cap?.Dispose();
            _pool?.Dispose();
        }

    }
}
