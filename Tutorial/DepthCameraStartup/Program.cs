using System.Reactive.Linq;
using OpenCvSharp;
using Husty.OpenCvSharp.DepthCamera;
using Husty.Extensions;
using Husty.OpenCvSharp.CameraCalibration;
using Husty.OpenCvSharp.Transform;

namespace DepthCameraStartup;

internal class Program
{
    static void Main(string[] args)
    {

        var inParam = IntrinsicCameraParameters.Load("..\\..\\..\\intrinsic.json");
        var exParam = ExtrinsicCameraParameters.Load("..\\..\\..\\extrinsic.json");
        var transformer = new PerspectiveTransformer(inParam.CameraMatrix, exParam);

        var bgr = new Mat("..\\..\\..\\000.png");
        var pts = Enumerable.Range(0, 480).SelectMany(y => Enumerable.Range(0, 640).Select(x => new Point2f(x, y))).ToArray();
        pts = transformer.ConvertToWorldCoordinate(pts);
        var xyz = new Mat(480, 640, MatType.CV_16UC3, pts.SelectMany(p => new short[] { (short)p.X, (short)p.Y, 1 }).ToArray());
        using var frame = new BgrXyzMat(bgr, xyz);
        BgrXyzImageIO.SaveAsAsciiPly(null, "ascii", frame);

        var f = pts.Select(p => new Scalar(p.X, p.Y, 0)).ToArray();


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

        Console.WriteLine("Press Enter key to exit...");
        ConsoleEx.WaitKey(ConsoleKey.Enter);
        connector.Dispose();
        camera.Dispose();

        Console.WriteLine("completed.");
        Console.ReadKey();

    }
}
