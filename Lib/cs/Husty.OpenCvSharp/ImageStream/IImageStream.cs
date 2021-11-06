using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{

    public record Properties(VideoCaptureProperties Key, double Value);

    public interface IImageStream<TImage> : IDisposable
    {

        public int Fps { get; }

        public int Channels { get; }

        public Size FrameSize { get; }

        public bool HasFrame { get; }

        public TImage Read();

        public IObservable<TImage> GetStream();

    }
}
