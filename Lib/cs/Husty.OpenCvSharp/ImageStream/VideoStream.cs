using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public class VideoStream : IVideoStream<Mat>
    {

        // ------ fields ------ //

        private int _positionIndex;
        private readonly VideoCapture _cap;


        // ------ properties ------ //

        public int Fps { get; }

        public int Channels { get; }

        public Size FrameSize { get; }

        public bool HasFrame { private set; get; }

        public int FrameCount { get; }

        public int CurrentPosition => _positionIndex;


        // ------ constructors ------ //

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
        }


        // ------ public methods ------ //

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

        public IObservable<Mat> GetStream()
        {
            return Observable.Interval(TimeSpan.FromMilliseconds(1000 / Fps), ThreadPoolScheduler.Instance)
                .Where(_ => _positionIndex < FrameCount)
                .Select(_ => Read())
                .Publish().RefCount();
        }

        public void Seek(int position)
        {
            if (position > -1 && position < FrameCount) _positionIndex = position;
        }

        public void Dispose()
        {
            HasFrame = false;
            _cap?.Dispose();
        }

    }
}
