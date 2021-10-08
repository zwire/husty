using System;
using System.Collections.Generic;
using OpenCvSharp;
using static System.Math;

namespace Test.HoughSingleLine
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // settings
            var thetaMin = 60 * PI / 180;
            var thetaMax = 120 * PI / 180;
            var rhoMin = 100;
            var rhoMax = 200;
            var xMin = 0;
            var xMax = 800;
            var yMin = 0;
            var yMax = 400;
            var thetaStep = 0.01;
            var rhoStep = 5;
            var xStep = 2;
            var yStep = 2;

            // initialize
            var hough = new Husty.HoughSingleLine(
                thetaMin, thetaMax,rhoMin, rhoMax,
                xMin, xMax, yMin, yMax,
                thetaStep, rhoStep, xStep, yStep);

            // load images and get points
            var img = Cv2.ImRead(@"..\..\..\line1.png", ImreadModes.Grayscale);
            Cv2.BitwiseNot(img, img);
            Cv2.Threshold(img, img, 100, 255, ThresholdTypes.Binary);
            Cv2.Resize(img, img, new Size(800, 400));
            var xyList = new List<(double X, double Y)>();
            for (int h = 0; h < img.Height; h++)
                for (int w = 0; w < img.Width; w++)
                    if (img.At<byte>(h, w) is not 0)
                        xyList.Add((w, h));

            // y = a * x + b
            // x = ... にすると発散しちゃうかも?
            var (th, rh) = hough.Run(xyList.ToArray());
            Console.WriteLine($"Theta = {th / PI * 180}, Rho = {rh}");

            var a = -1.0 / Tan(th);
            var b = rh / Sin(th);
            var p1 = new Point(0, b);
            var p2 = new Point(img.Width, a * img.Width + b);
            Cv2.CvtColor(img, img, ColorConversionCodes.GRAY2BGR);
            Cv2.Line(img, p1, p2, new Scalar(0, 0, 255), 2);
            Cv2.ImShow(" ", img);
            Cv2.WaitKey(0);

        }
    }
}
