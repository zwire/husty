using OpenCvSharp;

namespace Husty.OpenCvSharp.ImageStream;

public record Properties(VideoCaptureProperties Key, double Value);

public interface IImageStream<TImage> : IDisposable
{

  public int Fps { get; }

  public int Channels { get; }

  public Size FrameSize { get; }

  public IObservable<TImage> ImageSequence { get; }

  public TImage Read();

}
