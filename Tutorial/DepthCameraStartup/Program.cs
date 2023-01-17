using System.Reactive.Linq;
using OpenCvSharp;
using Husty.Extensions;
using Husty.OpenCvSharp.ImageStream;
using Husty.OpenCvSharp.SpatialImaging;
using Kinect = Husty.OpenCvSharp.AzureKinect;
using RealSense = Husty.OpenCvSharp.RealSense;

namespace DepthCameraStartup;

internal class Program
{
    static void Main(string[] args)
    {
        IImageStream<SpatialImage> camera = null;

        camera = new RealSense.CameraStream(new(640, 360));
        // camera = new Kinect.CameraStream(MatchingBase.Color);

        var connector = camera.GetStream()
            .TimeInterval()
            .Subscribe(v =>
            {
                Console.WriteLine((int)v.Interval.TotalMilliseconds);
                Cv2.ImShow(" ", v.Value.Color);
                Cv2.SetMouseCallback(" ", (t, x, y, f, _) =>
                {
                    if (t is MouseEventTypes.LButtonDown)
                    {
                        var px = v.Value.GetPixel(new(x, y));
                        Console.WriteLine(px);
                    }
                });
                Cv2.WaitKey(1);
            });

        Console.WriteLine("Press Enter key to exit...");
        ConsoleEx.WaitKey(ConsoleKey.Enter);
        connector.Dispose();
        camera.Dispose();

        Console.WriteLine("completed.");
        Console.ReadKey();

    }
}
