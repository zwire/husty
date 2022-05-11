using System;
using System.Collections.Generic;
using OpenCvSharp;
using Husty.OpenCvSharp.ImageStream;

namespace Video
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // optional : you can set video capture properties served by OpenCvSharp
            var properties = new List<Properties>()
            { 
                new(VideoCaptureProperties.FrameWidth, 640),
                new(VideoCaptureProperties.FrameHeight, 480),
                new(VideoCaptureProperties.Fps, 10)
            };

            // initialize
            var video = new VideoStream("..\\..\\..\\sample.mp4", properties);

            // capturing loop is asynchronize against main thread
            while (video.CurrentPosition < video.FrameCount - 1)
            {
                // be careful that this instance may read null object.
                var frame = video.Read();
                if (frame is null) continue;

                // or you can read frame like OpenCV Python binding method
                // using var frame = video.Read();

                Cv2.ImShow(" ", frame);
                Cv2.WaitKey(1);
                Console.WriteLine($"{video.CurrentPosition}");
            }

            Console.WriteLine("completed.");

        }
    }
}
