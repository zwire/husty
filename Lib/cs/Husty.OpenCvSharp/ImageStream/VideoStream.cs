using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;
using Reactive.Bindings;

namespace Husty.OpenCvSharp
{
    public class VideoStream
    {

        // ------- Fields ------- //

        private IDisposable _selfConnector;
        private int _positionIndex;
        private readonly VideoCapture _cap;


        // ------- Properties ------- //

        public int Fps { get; }

        public int Channels { get; }

        public Size FrameSize { get; }

        public bool HasFrame { private set; get; }

        public int FrameCount { get; }

        public int CurrentPosition { get; }

        public ReadOnlyReactivePropertySlim<Mat> ReactiveFrame { get; }


        // ------- Constructors ------- //

        public VideoStream(string src, IEnumerable<Properties> properties = null)
        {
            _cap = new(src);
            if (properties is not null)
                foreach (var p in properties)
                    _cap.Set(p.Key, p.Value);
            Fps = (int)_cap.Fps;
            Channels = (int)_cap.Get(VideoCaptureProperties.Channel);
            FrameSize = new(_cap.FrameWidth, _cap.FrameHeight);
            FrameCount = _cap.FrameCount;
            ReactiveFrame = BeginStream(0).ToReadOnlyReactivePropertySlim();
        }


        // ------- Methods ------- //

        public Mat Read()
        {
            GC.Collect();
            if (_positionIndex == FrameCount - 1) _positionIndex--;
            _cap.Set(VideoCaptureProperties.PosFrames, _positionIndex++);
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
            _selfConnector?.Dispose();
            ReactiveFrame?.Dispose();
            _cap?.Dispose();
        }

        private IObservable<Mat> BeginStream(int position)
        {
            if (position > -1 && position < FrameCount) _positionIndex = position;
            var obs = Observable.Interval(TimeSpan.FromMilliseconds(1000 / Fps), ThreadPoolScheduler.Instance)
                .Where(_ => _positionIndex < FrameCount)
                .Select(_ => Read())
                .Publish();
            _selfConnector = obs.Connect();
            return obs;
        }

    }
}
