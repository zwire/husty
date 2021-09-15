using System;
using System.IO;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{

	public class ExtrinsicCameraParameters
	{

		public Mat RotationMatrix { get; }

		public Mat TranslationVector { get; }

		public ExtrinsicCameraParameters(Mat rotationMatrix, Mat translationVector)
		{
			if (!(rotationMatrix.Rows == 3 && rotationMatrix.Cols == 3))
				throw new ArgumentException("Requires: 3x3 matrix.", nameof(rotationMatrix));
			if (!(translationVector.Rows == 3 && translationVector.Cols == 1))
				throw new ArgumentException("Requires: 3x1 matrix.", nameof(translationVector));
			RotationMatrix = rotationMatrix;
			TranslationVector = translationVector;
		}

		public ExtrinsicCameraParameters Clone()
			=> new(RotationMatrix, TranslationVector);

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
