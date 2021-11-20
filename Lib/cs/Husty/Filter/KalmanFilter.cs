using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

// k ... State Vector Length
// m ... Measurement Vector Length
// n ... Control Vector Length
// 
// If you describe Matrix like,
// 
//     M = A B C
//         D E F
//         G H I
//         
// you should write code like this,
// 
// m = { a, b, c, d, e, f, g, h, i }
// 
// Simple version constructor is available if you need.

namespace Husty
{
    /// <summary>
    /// Filtering & control methods subject to Gaussian distribution
    /// </summary>
    public class KalmanFilter
    {

        // ------ fields ------ //

        private readonly int k;                 // State Vector Length
        private readonly int m;                 // Measurement Vector Length
        private readonly int n;                 // Control Vector Length
        private readonly Matrix<double> A;      // Transition Matrix
        private readonly Matrix<double> AT;     // Transition Matrix Transpose
        private readonly Matrix<double> B;      // Control Matrix
        private readonly Matrix<double> C;      // Measure Matrix
        private readonly Matrix<double> CT;     // Measure Matrix Transpose
        private readonly Matrix<double> Q;      // Process Noise Matrix
        private readonly Matrix<double> R;      // Measure Noise Matrix
        private Matrix<double> P;               // Error Covariance Matrix
        private Vector<double> X;               // State Vector


        // ------ constructors ------ //

        /// <summary>
        /// Use same status and observe parameter.
        /// You can input control.
        /// Noise covariance is default value.
        /// </summary>        
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="filterStrength"></param>
        public KalmanFilter(double[] initialStateVec, double[]? controlMatrix, double filterStrength = 1.0)
        {

            k = initialStateVec.Length;
            m = initialStateVec.Length;
            n = controlMatrix is not null ? controlMatrix.Length / k : 0;
            X = DenseVector.OfArray(initialStateVec);
            B = controlMatrix is not null ? new DenseMatrix(n, k, controlMatrix).Transpose() : null;
            C = DenseMatrix.OfArray(new double[m, k]);
            A = DenseMatrix.OfArray(new double[k, k]);
            R = DenseMatrix.OfArray(new double[m, m]);
            Q = DenseMatrix.OfArray(new double[k, k]);
            P = DenseMatrix.OfArray(new double[k, k]);
            for (int i = 0; i < k; i++)
            {
                C[i, i] = 1;
                A[i, i] = 1;
                R[i, i] = filterStrength;
                Q[i, i] = 1.0 / filterStrength;
                P[i, i] = 1.0;
            }
            AT = A.Transpose();
            CT = C.Transpose();
        }

        /// <summary>
        /// Use same status and observe parameter.
        /// You can input control.
        /// Noise covariance is default value.
        /// </summary>        
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="preError">P (default)</param>
        public KalmanFilter(double[] initialStateVec, double[]? controlMatrix, double measurementNoise = 1.0, double processNoise = 1.0, double preError = 1.0)
        {

            k = initialStateVec.Length;
            m = initialStateVec.Length;
            n = controlMatrix is not null ? controlMatrix.Length / k : 0;
            X = DenseVector.OfArray(initialStateVec);
            B = controlMatrix is not null ? new DenseMatrix(n, k, controlMatrix).Transpose() : null;
            C = DenseMatrix.OfArray(new double[m, k]);
            A = DenseMatrix.OfArray(new double[k, k]);
            R = DenseMatrix.OfArray(new double[m, m]);
            Q = DenseMatrix.OfArray(new double[k, k]);
            P = DenseMatrix.OfArray(new double[k, k]);
            for (int i = 0; i < k; i++)
            {
                C[i, i] = 1;
                A[i, i] = 1;
                R[i, i] = measurementNoise;
                Q[i, i] = processNoise;
                P[i, i] = preError;
            }
            AT = A.Transpose();
            CT = C.Transpose();
        }

        /// <summary>
        /// In the case of that observe differ from status.
        /// You can input control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoise">R (default)</param>
        /// <param name="processNoise">Q (default)</param>
        /// <param name="preError">P (default)</param>
        public KalmanFilter(double[] initialStateVec, double[]? controlMatrix, double[] transitionMatrix, double[] measurementMatrix, double measurementNoise = 1.0, double processNoise = 1.0, double preError = 1.0)
        {
            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            n = controlMatrix is not null ? controlMatrix.Length / k : 0;
            X = DenseVector.OfArray(initialStateVec);
            B = controlMatrix is not null ? new DenseMatrix(n, k, controlMatrix).Transpose() : null;
            C = new DenseMatrix(k, m, measurementMatrix).Transpose();
            A = new DenseMatrix(k, k, transitionMatrix).Transpose();
            R = DenseMatrix.OfArray(new double[m, m]);
            Q = DenseMatrix.OfArray(new double[k, k]);
            P = DenseMatrix.OfArray(new double[k, k]);
            for (int i = 0; i < k; i++)
            {
                Q[i, i] = processNoise;
                P[i, i] = preError;
            }
            for (int i = 0; i < m; i++)
                R[i, i] = measurementNoise;
            AT = A.Transpose();
            CT = C.Transpose();
        }

        /// <summary>
        /// You can design noise covariance matrix.
        /// But can input control.
        /// </summary>
        /// <param name="initialStateVec">X (k * 1)</param>
        /// <param name="controlMatrix">B (k * n)</param>
        /// <param name="transitionMatrix">A (k * k)</param>
        /// <param name="measurementMatrix">C (m * k)</param>
        /// <param name="measurementNoiseMatrix">R (m * m)</param>
        /// <param name="processNoiseMatrix">Q (k * k)</param>
        /// <param name="preError">P (default)</param>
        public KalmanFilter(double[] initialStateVec, double[]? controlMatrix, double[] transitionMatrix, double[] measurementMatrix, double[] measurementNoiseMatrix, double[] processNoiseMatrix, double preError = 1.0)
        {
            k = initialStateVec.Length;
            m = measurementMatrix.Length / k;
            n = controlMatrix is not null ? controlMatrix.Length / k : 0;
            X = DenseVector.OfArray(initialStateVec);
            B = controlMatrix is not null ? new DenseMatrix(n, k, controlMatrix).Transpose() : null;
            C = new DenseMatrix(k, m, measurementMatrix).Transpose();
            A = new DenseMatrix(k, k, transitionMatrix).Transpose();
            R = new DenseMatrix(m, m, measurementNoiseMatrix).Transpose();
            Q = new DenseMatrix(k, k, processNoiseMatrix).Transpose();
            for (int i = 0; i < k; i++)
                P[i, i] = preError;
            AT = A.Transpose();
            CT = C.Transpose();
        }


        // ------ public methods ------ //

        /// <summary>
        /// Do 'Correct' and 'Predict'.
        /// </summary>
        /// <param name="measurementVec">Y (m * 1)</param>
        /// <param name="controlVec">U (n * 1)</param>
        /// <returns>Results as same type of input</returns>
        public (double[] Correct, double[] Predict) Update(double[] measurementVec, double[] controlVec = null)
        {

            var Y = DenseVector.OfArray(measurementVec);
            var CP = C * P;
            var G = P * CT * (CP * CT + R).Inverse();
            X += G * (Y - C * X);
            P -= G * CP;
            var correct = X.ToArray();

            X = A * X;
            if (controlVec is not null)
            {
                if (B is null)
                    throw new InvalidOperationException("Control matrix is not initialized.");
                X += B * DenseVector.OfArray(controlVec);
            }
            P = A * P * AT + Q;
            var predict = X.ToArray();

            return (correct, predict);

        }

    }
}
