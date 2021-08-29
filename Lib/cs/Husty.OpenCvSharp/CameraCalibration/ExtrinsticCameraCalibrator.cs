using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public static class ExtrinsticCameraCalibrator
    {

        public static ExtrinsicCameraParameters CalibrateWithGroundCoordinates(
            List<(Point2f, Point3f)> points,
            IntrinsicCameraParameters intrinsticParams)
        {
            if (points.Count == 0)
                throw new ArgumentException("Require: points count >= 1");
            var displayPoints = new List<Point2f>();
            var worldPoints = new List<Point3f>();
            foreach (var (display, world) in points)
            {
                displayPoints.Add(display);
                worldPoints.Add(world);
            }
            var distMat = intrinsticParams.DistortionCoeffs;
            var cameraMat = intrinsticParams.CameraMatrix;
            var rotationVec = new Mat(3, 1, MatType.CV_64F);
            var translationVec = new Mat(3, 1, MatType.CV_64F);
            Cv2.SolvePnP(
                InputArray.Create(worldPoints),
                InputArray.Create(displayPoints),
                cameraMat, distMat,
                rotationVec, translationVec, 
                false, SolvePnPFlags.Iterative);
            var rotationMat = new Mat(3, 3, MatType.CV_64F);
            Cv2.Rodrigues(rotationVec, rotationMat);
            return new ExtrinsicCameraParameters(rotationMat, translationVec);
        }

        public static Point ToPoint(this Point2f p)
            => new(p.X, p.Y);

        public static IEnumerable<Point> ToEnumerablePoint(this IEnumerable<Point2f> points)
            => points.Select(p => new Point(p.X, p.Y));

        public static IEnumerable<Point2f> ToEnumerablePoint2f(this IEnumerable<Point> points)
            => points.Select(p => new Point2f(p.X, p.Y));

    }
}
