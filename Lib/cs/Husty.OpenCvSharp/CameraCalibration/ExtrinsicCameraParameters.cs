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
			=> File.WriteAllText(fileName, ToText());

		public static ExtrinsicCameraParameters Load(string fileName)
			=> ParseText(File.ReadAllText(fileName));

		private string ToText()
		{
			var str = "";
			str += $"RotationMatrix,";
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					str += $"{RotationMatrix.At<double>(i, j)},";
			str += "\n";
			str += $"TranslationVector,";
			for (int i = 0; i < 3; i++)
				str += $"{TranslationVector.At<double>(i)},";
			return str;
		}

		private static ExtrinsicCameraParameters ParseText(string str)
		{
			var lines = str.Split("\n");
			var l0 = lines[0].Split(",");
			var l1 = lines[1].Split(",");
			if (l0[0] != "RotationMatrix" || l1[0] != "TranslationVector")
				throw new InvalidDataException();
			var rot = new Mat(3, 3, MatType.CV_64F);
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					rot.At<double>(i, j) = double.Parse(l0[1 + i * 3 + j]);
			var trs = new Mat(3, 1, MatType.CV_64F);
			for (int i = 0; i < 3; i++)
				trs.At<double>(i) = double.Parse(l1[1 + i]);
			return new(rot, trs);
		}
	}
}
