using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenCvSharp;

namespace Test.YoloX
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var frame = new Mat("..\\..\\..\\dog3.jpg");
            var yolox = new YoloX("..\\..\\..\\yolox_tiny.onnx", 0.5);
            var names = File.ReadAllText("..\\..\\..\\_.names").Split("\n").Select(t => t.TrimEnd()).ToArray();
            var colors = Enumerable.Range(0, 80).Select(_ => new Scalar(new Random().Next(255), new Random().Next(255), new Random().Next(255))).ToArray();
            var watch = new Stopwatch();
            watch.Start();
            var results = yolox.Run(frame);
            foreach (var r in results)
            {
                Console.WriteLine($"{r.Probability * 100:f0}% {names[r.Label]}, x: {r.Box.X}, y: {r.Box.Y}");
                Cv2.Rectangle(frame, r.Box, colors[r.Label], 1);
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            Cv2.Resize(frame, frame, new(frame.Width * 2, frame.Height * 2));
            Cv2.ImShow(" ", frame);
            Cv2.WaitKey();
        }
    }
}
