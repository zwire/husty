using System;
using OpenCvSharp;
using Reactive.Bindings;

namespace Husty.OpenCvSharp
{

    public record Properties(VideoCaptureProperties Key, double Value);

    public interface IImageStream<TImage> : IDisposable
    {

        public int Fps { get; }

        public int Channels { get; }

        public Size FrameSize { get; }

        public bool HasFrame { get; }

        public ReadOnlyReactivePropertySlim<TImage> ReactiveFrame { get; }

        public TImage Read();

    }
}
