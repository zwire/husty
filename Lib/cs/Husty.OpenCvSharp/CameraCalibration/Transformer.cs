using OpenCvSharp;

namespace Husty.OpenCvSharp
{

    public class Transformer
    {

        // 
        // Display Coordinate ... x
        // World Coordinate ... X
        // Rotation Matrix ... R
        // Translation Vector ... T
        // Camera Matrix ... A
        // 
        // It need to calculate scale factor, now naming 's'.
        // 
        // s * x = A * (R * X + T)
        // s = (X + R^-1 * T) / (R^-1 * A^-1 * x)
        // 
        // This equation couldn't be solved generally,
        // but assuming flat ground (z = 0) ...
        // 
        // s = (R^-1 * T) / (R^-1 * A^-1 * x)
        // 

        private readonly Mat _A;
        private readonly Mat _T;
        private readonly Mat _R;
        private readonly Mat _AInv;
        private readonly Mat _RInv;
        private readonly Mat _RInv_T;
        private readonly Mat _RInv_AInv;

        public Transformer(IntrinsicCameraParameters paramIn, ExtrinsicCameraParameters paramEx)
        {
            _A = paramIn.CameraMatrix;
            _T = paramEx.TranslationVector;
            _R = paramEx.RotationMatrix;
            _AInv = _A.Inv();
            _RInv = _R.Inv();
            _RInv_T = _RInv * _T;
            _RInv_AInv = _RInv * _AInv;
        }

        /// <summary>
        /// convert display XY --> world XY
        /// </summary>
        /// <param name="p">display point</param>
        /// <returns></returns>
        public Point2f ConvertToWorldCoordinate(Point2f p)
        {
            var displayPoint = new Mat(3, 1, MatType.CV_64F, new double[] { p.X, p.Y, 1 });
            var s = (_RInv_T / (_RInv_AInv * displayPoint)).ToMat().At<double>(2);
            var worldPoint = (_RInv * (_AInv * s * displayPoint - _T)).ToMat();
            return new((float)worldPoint.At<double>(0), (float)worldPoint.At<double>(1));
        }

        /// <summary>
        /// convert world XY --> display XY
        /// </summary>
        /// <param name="p">world point</param>
        /// <returns></returns>
        public Point2f ConvertToDisplayCoordinate(Point2f p)
        {
            var worldPoint = new Mat(3, 1, MatType.CV_64F, new double[] { p.X, p.Y, 0 });
            var tmp = (_R * worldPoint + _T).ToMat();
            var x = tmp.At<double>(0) / tmp.At<double>(2);
            var y = tmp.At<double>(1) / tmp.At<double>(2);
            x = _A.At<double>(0, 0) * x + _A.At<double>(0, 2);
            y = _A.At<double>(1, 1) * y + _A.At<double>(1, 2);
            return new((float)x, (float)y);
        }

    }
}
