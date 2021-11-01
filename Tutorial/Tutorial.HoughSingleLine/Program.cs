using System;
using OpenCvSharp;
using Husty.OpenCvSharp;
using static System.Math;

namespace Tutorial.HoughSingleLine
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
            var hough = new Husty.OpenCvSharp.HoughSingleLine(
                thetaMin, thetaMax,rhoMin, rhoMax,
                xMin, xMax, yMin, yMax,
                thetaStep, rhoStep, xStep, yStep);

            // load images and get points
            using var img = Cv2.ImRead(@"..\..\..\line1.png", ImreadModes.Grayscale);
            Cv2.BitwiseNot(img, img);
            Cv2.Threshold(img, img, 100, 255, ThresholdTypes.Binary);
            Cv2.Resize(img, img, new Size(800, 400));
            var result = hough.Run(img.GetNonZeroLocations());
            Console.WriteLine($"Theta = {result.ThetaDegree}, Rho = {result.Rho}");

            var line = result.ToLine2D();
            var p1 = new Point(0, line.GetY(0));
            var p2 = new Point(img.Width, line.GetY(img.Width));
            Cv2.CvtColor(img, img, ColorConversionCodes.GRAY2BGR);
            Cv2.Line(img, p1, p2, new Scalar(0, 0, 255), 2);
            Cv2.ImShow(" ", img);
            Cv2.WaitKey(0);

        }
    }
}
