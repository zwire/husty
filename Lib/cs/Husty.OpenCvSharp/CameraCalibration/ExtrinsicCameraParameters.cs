using System;
using System.IO;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{

	public sealed class ExtrinsicCameraParameters
	{

		public Mat RotationMatrix { get; }

		public Mat TranslationVector { get; }

		public ExtrinsicCameraParameters(Mat rotationMatrix, Mat translationVector)
		{
			if (!(rotationMatrix.Rows is 3 && rotationMatrix.Cols is 3))
				throw new ArgumentException("Requires: 3x3 matrix.", nameof(rotationMatrix));
			if (!(translationVector.Rows is 3 && translationVector.Cols is 1))
				throw new ArgumentException("Requires: 3x1 matrix.", nameof(translationVector));
			RotationMatrix = rotationMatrix;
			TranslationVector = translationVector;
		}

		public ExtrinsicCameraParameters Clone()
			=> new(RotationMatrix, TranslationVector);

		public unsafe Vec3d GetEulerCoordinate()
		{
			var rd = (double*)RotationMatrix.Data;
			var x = Math.Atan2(rd[7], rd[8]);
			var y = Math.Asin(-rd[6]);
			var z = Math.Atan2(rd[3], rd[0]);
			return new(x, y, z);
		}

		public void Save(string fileName)
        {
            var obj = new ExtrinsicJson(this);
            File.WriteAllText(fileName, obj.Serialize());
        }

        public static ExtrinsicCameraParameters Load(string fileName)
        {
            var str = File.ReadAllText(fileName);
            return ExtrinsicJson.Deserialize(str);
        }

    }
}
