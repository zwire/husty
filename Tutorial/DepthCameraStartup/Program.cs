using System.Reactive.Linq;
using OpenCvSharp;
using Husty.Extensions;
using Husty.OpenCvSharp.ImageStream;
using Husty.OpenCvSharp.ThreeDimensionalImaging;
using Kinect = Husty.OpenCvSharp.AzureKinect;
using RealSense = Husty.OpenCvSharp.RealSense;

IImageStream<BgrXyzImage> camera = null;

camera = new RealSense.CameraStream(new(640, 360));
//camera = new Kinect.CameraStream();

var connector = camera.GetStream()
    .TimeInterval()
    .Subscribe(v =>
    {
        Console.WriteLine((int)v.Interval.TotalMilliseconds);
        Cv2.ImShow(" ", v.Value.Bgr);
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