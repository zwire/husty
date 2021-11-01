using System;
using OpenCvSharp;
using Husty.OpenCvSharp;

namespace Tutorial.Yolo
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // model files
            var cfg = "..\\..\\..\\model\\_.cfg";
            var weights = "..\\..\\..\\model\\_.weights";
            var names = "..\\..\\..\\model\\_.names";

            // initialize detector.
            // blob width snd height must be multiple of 32.
            var detector = new YoloDetector(cfg, weights, names, new Size(640, 480));

            // input image path
            var img = Cv2.ImRead("..\\..\\..\\sample.jpg");

            // execute inference session
            var results = detector.Run(img);

            // show results
            foreach (var r in results)
            {
                r.DrawBox(img, new(0, 0, 255), 2);
                Console.WriteLine($"{r.Label} : {r.Confidence:f0}%");
            }
            Cv2.ImShow(" ", img);
            Cv2.WaitKey();

        }
    }
}
