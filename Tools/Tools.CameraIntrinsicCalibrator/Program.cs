using System;
using System.Threading.Tasks;
using System.IO;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using Husty.OpenCvSharp.CameraCalibration;

namespace Tools.CameraIntrinsicCalibrator
{
    internal class Program : ConsoleAppBase
    {

        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        }

        public void Run(
            [Option("d", "working directory")] string dir = null
        )
        {
            dir ??= Environment.CurrentDirectory;
            if (!Directory.Exists(dir)) throw new ArgumentException("directory not exist!");
            var files = Directory.GetFiles(dir, "*.png");
            var param = IntrinsicCameraCalibrator.CalibrateWithChessboardImages(new(7, 10, 32.5f), files);
            param.Save($"{dir}\\intrinsic.json");
            Console.WriteLine("successfully calibrated and saved as intrinsic.json");
        }
    }
}

