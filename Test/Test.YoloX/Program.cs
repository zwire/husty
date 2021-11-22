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

            using var frame = new Mat("..\\..\\..\\sample1.jpg");
            var yolox = new YoloX("..\\..\\..\\yolox_tiny.onnx");
            var watch = new Stopwatch();
            watch.Start();
            var results = yolox.Run(frame).Where(r => r.Label is 0);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            foreach (var r in results)
            {
                Console.WriteLine($"class: {r.Label}, x: {r.Box.X}, y: {r.Box.Y}, w: {r.Box.Width}, h: {r.Box.Height}");
                Cv2.Rectangle(frame, r.Box, new(0, 0, 255), 2);
            }
            Cv2.ImShow(" ", frame);
            Cv2.WaitKey();
        }
    }
}
