using System;
using System.Collections.Generic;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public static class ExtrinsicCameraCalibrator
    {

        public static ExtrinsicCameraParameters CalibrateWithGroundCoordinates(
            List<(Point2f, Point3f)> points,
            IntrinsicCameraParameters intrinsticParams)
        {
            if (points.Count is 0)
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

    }
}
