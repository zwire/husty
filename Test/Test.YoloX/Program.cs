using System;
using System.Diagnostics;
using System.Linq;
using OpenCvSharp;

namespace Test.YoloX
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var frame = new Mat("..\\..\\..\\sample2.jpg");
            var yolox = new Husty.OpenCvSharp.YoloX("..\\..\\..\\yolox_tiny.onnx", "..\\..\\..\\_.names", 0.5f);
            var colors = Enumerable.Range(0, 80).Select(_ => new Scalar(new Random().Next(255), new Random().Next(255), new Random().Next(255))).ToArray();
            var watch = new Stopwatch();
            watch.Start();
            yolox.Run(frame).ToList().ForEach(r => r.DrawBox(frame, colors[0], 2, true, true, 0.5));
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds + " ms");
            Cv2.Resize(frame, frame, new(frame.Width * 2, frame.Height * 2));
            Cv2.ImShow(" ", frame);
            Cv2.WaitKey();
        }
    }
}
