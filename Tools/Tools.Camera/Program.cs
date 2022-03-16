using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using OpenCvSharp;
using Husty.OpenCvSharp;

namespace Tools.Camera
{
    internal class Program : ConsoleAppBase
    {

        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        }

        public void Run(
            [Option("w", "image width")] int width = 640,
            [Option("h", "image height")] int height = 480,
            [Option("f", "fps")] int fps = 30,
            [Option("i", "camera index number")] int index = 0,
            [Option("d", "save directory")] string saveDir = null,
            [Option("r", "recording")] bool recording = false
        )
        {
            saveDir ??= Environment.CurrentDirectory;
            if (!Directory.Exists(saveDir)) throw new ArgumentException("directory not exist!");
            VideoWriter writer = null;
            var count = -1;
            if (recording)
            {
                while (File.Exists($"{saveDir}\\v{++count:d3}.mp4")) ;
                writer = new($"{saveDir}\\v{++count:d3}.mp4", FourCC.MPG4, fps, new(width, height));
            }

            var properties = new List<Properties>()
            {
                new(VideoCaptureProperties.FrameWidth, width),
                new(VideoCaptureProperties.FrameHeight, height),
                new(VideoCaptureProperties.Fps, fps)
            };
            using var cap = new CameraStream(index, properties);

            Console.WriteLine("press SPACE to take a picture");
            Console.WriteLine("press ESC to finish ...");
            count = -1;
            while (true)
            {
                var frame = cap.Read();
                if (frame.Width != width || frame.Height != height)
                    Cv2.Resize(frame, frame, new(width, height));
                Cv2.ImShow(" ", frame);
                Cv2.WaitKey(1);
                writer?.Write(frame);
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.Escape:
                            goto ESC;
                        case ConsoleKey.Spacebar:
                            while (File.Exists($"{saveDir}\\i{++count:d3}.png")) ;
                            Cv2.ImWrite($"{saveDir}\\i{count:d3}.png", frame);
                            Console.WriteLine($"capture frame as i{count:d3}.png");
                            break;
                    }
                }
            }
        ESC:
            writer?.Dispose();
        }
    }
}
