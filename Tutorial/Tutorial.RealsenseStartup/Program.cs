using System;
using OpenCvSharp;
using Husty.OpenCvSharp.DepthCamera;

namespace Tutorial.RealsenseStartup
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var camera = new Realsense(new(640, 360));
            while (true)
            {
                var frame = camera.Read();
                if (frame is null) continue;
                using var d8 = frame.Depth8(300, 3000);
                Cv2.ImShow("BGR", frame.BGR);
                Cv2.ImShow("D", d8);
                Cv2.WaitKey(1);
                if (Console.KeyAvailable)
                    if (Console.ReadKey().Key is ConsoleKey.Q)
                        break;
            }

            Console.WriteLine("completed.");
            camera.Dispose();
        }
    }
}
