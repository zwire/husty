using OpenCvSharp;

namespace Husty.OpenCvSharp.Yolo;

public interface IYoloDetector : IDisposable
{
  public YoloResult[] Run(Mat frame);
}
