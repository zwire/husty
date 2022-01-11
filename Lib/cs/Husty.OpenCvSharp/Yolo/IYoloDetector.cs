using System;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public interface IYoloDetector : IDisposable
    {
        public YoloResult[] Run(Mat frame);
    }
}
