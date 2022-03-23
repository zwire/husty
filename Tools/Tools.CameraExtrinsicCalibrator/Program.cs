using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using OpenCvSharp;
using Husty.OpenCvSharp.CameraCalibration;

namespace Tools.CameraExtrinsicCalibrator
{
    internal class Program : ConsoleAppBase
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        }

        public void Run(
            [Option("i", "input image path")] string imgPath,
            [Option("d", "working directory")] string dir = null,
            [Option("p", "intrinsic param path")] string intrinsicPath = null
        )
        {
            dir ??= Environment.CurrentDirectory;
            if (!Directory.Exists(dir)) throw new ArgumentException("directory not exist!");
            intrinsicPath ??= dir + "\\intrinsic.json";
            var paramIn = IntrinsicCameraParameters.Load(intrinsicPath);
            using var frame = Cv2.ImRead(imgPath).Undistort(paramIn.CameraMatrix, paramIn.DistortionCoeffs);
            var escape = false;
            ExtrinsicCameraParameters paramEx = null;

            Console.Write("check only? (y/n) : ");
            if (Console.ReadKey().Key is not ConsoleKey.Y)
            {
                var list = new List<(Point2f, Point3f)>();
                Console.WriteLine("click image window\n");
                while (!escape)
                {
                    Cv2.ImShow(" ", frame);
                    Cv2.SetMouseCallback(" ", (e, x, y, f, u) =>
                    {
                        if (e is MouseEventTypes.LButtonDown)
                        {
                            Console.WriteLine("input real scale X, Y ");
                            Console.Write($" {x}, {y} --> ");
                            var line = Console.ReadLine().Split(",");
                            if (line.Length > 1 &&
                                float.TryParse(line[0], out var xx) &&
                                float.TryParse(line[1], out var yy)
                            )
                            {
                                list.Add((new(x, y), new(xx, yy, 0)));
                                Cv2.Circle(frame, new Point(x, y), 2, new(0, 0, 220), 4);
                                Cv2.DestroyAllWindows();
                            }
                            Console.WriteLine($"current points count is {list.Count}\n");
                            if (list.Count >= 4)
                            {
                                Console.Write("do you execute calibration now? (y/n) : ");
                                if (Console.ReadKey().Key is ConsoleKey.Y)
                                {
                                    escape = true;
                                    Cv2.DestroyAllWindows();
                                }
                            }
                        }
                    });
                    if (Cv2.WaitKey(0) is 27) Cv2.DestroyAllWindows();
                }
                paramEx = ExtrinsicCameraCalibrator.CalibrateWithGroundCoordinates(list, paramIn);
                paramEx.Save(dir + "\\extrinsic.json");
            }

            Console.WriteLine("\ncheck result");
            paramEx = ExtrinsicCameraParameters.Load(dir + "\\extrinsic.json");
            var trf = new PerspectiveTransformer(paramIn.CameraMatrix, paramEx);
            escape = false;
            while (!escape)
            {
                Cv2.ImShow(" ", frame);
                Cv2.SetMouseCallback(" ", (e, x, y, f, u) =>
                {
                    if (e is MouseEventTypes.LButtonDown)
                    {
                        var p = trf.ConvertToWorldCoordinate(new(x, y));
                        Console.WriteLine($" {x}, {y} --> {p.X:f3}, {p.Y:f3}");
                        Cv2.Circle(frame, new Point(x, y), 2, new(0, 0, 220), 4);
                        Cv2.DestroyAllWindows();
                    }
                    if (Console.KeyAvailable)
                    {
                        if (Console.ReadKey().Key is ConsoleKey.Escape)
                        {
                            escape = true;
                            Cv2.DestroyAllWindows();
                        }
                    }
                });
                if (Cv2.WaitKey(0) is 27) Cv2.DestroyAllWindows();
            }
        }
    }
}
