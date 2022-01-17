using System;
using System.IO;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using OpenCvSharp;
using Husty.OpenCvSharp;

namespace Tools.VideoTrimmer
{
    internal class Program : ConsoleAppBase
    {

        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        }

        public void Run(
            [Option("f", "source file")] string file,
            [Option("i", "skip interval")] int interval,
            [Option("d", "save directory")] string saveDir = null,
            [Option("w", "width")] int? width = null,
            [Option("h", "height")] int? height = null
        )
        {
            if (interval < 1) throw new ArgumentOutOfRangeException("interval must be larger than 1.");
            saveDir ??= Environment.CurrentDirectory;
            if (!Directory.Exists(saveDir)) throw new ArgumentException("directory not exist!");
            using var cap = new VideoStream(file);
            var count = -1;
            width ??= cap.FrameSize.Width;
            height ??= cap.FrameSize.Height;
            for (int i = 0; i < cap.FrameCount; i++)
            {
                using var frame = cap.Read();
                Cv2.Resize(frame, frame, new((int)width, (int)height));
                if (i % interval is 0)
                {
                    while (File.Exists($"{saveDir}\\i{++count:d3}.png")) ;
                    Cv2.ImWrite($"{saveDir}\\i{count:d3}.png", frame);
                }
            }
            Console.WriteLine("completed!");
        }
    }
}
