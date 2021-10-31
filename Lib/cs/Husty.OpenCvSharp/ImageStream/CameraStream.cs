using System.Linq;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;
using Reactive.Bindings;

namespace Husty.OpenCvSharp
{
    public class CameraStream : IImageStream<Mat>
    {

        // ------- Fields ------- //

        private readonly VideoCapture _cap;


        // ------- Properties ------- //

        public int Fps { get; }

        public int Channels { get; }

        public Size FrameSize { get; }

        public bool HasFrame { private set; get; }

        public ReadOnlyReactivePropertySlim<Mat> ReactiveFrame { get; }


        // ------- Constructors ------- //

        public CameraStream(int src, IEnumerable<Properties> properties = null)
        {
            _cap = new(src);
            if (properties is not null)
                foreach (var p in properties)
                    _cap.Set(p.Key, p.Value);
            Fps = (int)_cap.Fps;
            Channels = (int)_cap.Get(VideoCaptureProperties.Channel);
            FrameSize = new(_cap.FrameWidth, _cap.FrameHeight);
            ReactiveFrame = Observable
                .Repeat(0, ThreadPoolScheduler.Instance)
                .Select(_ => Read())
                .ToReadOnlyReactivePropertySlim();
        }


        // ------- Methods ------- //

        public Mat Read()
        {
            var frame = new Mat();
            HasFrame = _cap.Read(frame);
            if (HasFrame)
                return frame;
            else
                return null;
        }

        public void Dispose()
        {
            HasFrame = false;
            ReactiveFrame?.Dispose();
            _cap?.Dispose();
        }

    }
}
