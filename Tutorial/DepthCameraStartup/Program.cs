using System.Reactive.Linq;
using OpenCvSharp;
using Husty.OpenCvSharp.DepthCamera;

namespace DepthCameraStartup;

internal class Program
{
    static void Main(string[] args)
    {

        IDepthCamera camera = null;

        camera = new Realsense(new(640, 360));
        //camera = new Kinect(AlignBase.Color);

        var connector = camera.GetBgrStream()
            .TimeInterval()
            .Subscribe(x =>
            {
                Console.WriteLine((int)x.Interval.TotalMilliseconds);
                Cv2.ImShow(" ", x.Value);
                Cv2.SetMouseCallback(" ", (t, x, y, f, _) =>
                {
                    if (t is MouseEventTypes.LButtonDown)
                    {
                        var xyz = camera.ReadXyz().At<Vec3s>(y, x);
                        Console.WriteLine(xyz);
                    }
                });
                Cv2.WaitKey(1);
            });

        Console.ReadKey();
        connector.Dispose();
        camera.Dispose();

        Console.WriteLine("completed.");
        Console.ReadKey();

    }
}
