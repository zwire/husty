using System;
using System.IO;
using OpenCvSharp;

namespace Husty.OpenCvSharp
{

	public class IntrinsicCameraParameters
	{

		public Size ImageSize { get; }

		public Mat CameraMatrix { get; }

		public Mat DistortionCoeffs { get; }

		public IntrinsicCameraParameters WithoutDistCoeffs
			=> new(ImageSize, CameraMatrix, new Mat(5, 1, MatType.CV_64F, 0));


		public IntrinsicCameraParameters(Size imageSize, Mat cameraMatrix, Mat distortionCoeffs)
		{
			if (!(cameraMatrix.Rows == 3 && cameraMatrix.Cols == 3))
				throw new ArgumentException("Requires: 3x3 matrix.", nameof(cameraMatrix));
			if (!(distortionCoeffs.Cols == 1 &&
				(distortionCoeffs.Rows == 4 || distortionCoeffs.Rows == 5 || distortionCoeffs.Rows == 8 || distortionCoeffs.Rows == 14)))
				throw new ArgumentException("Requires: 4x1, 5x1, 8x1 or 14x1 matrix.", nameof(distortionCoeffs));
			ImageSize = imageSize;
			CameraMatrix = cameraMatrix;
			DistortionCoeffs = distortionCoeffs;
		}

		public IntrinsicCameraParameters Clone()
			=> new(ImageSize, CameraMatrix, DistortionCoeffs);

		public void Save(string fileName)
			=> File.WriteAllText(fileName, ToText());

		public static IntrinsicCameraParameters Load(string fileName)
			=> ParseText(File.ReadAllText(fileName));

		private string ToText()
        {
			var str = "";
			str += $"ImageSize,{ImageSize.Width},{ImageSize.Height}\n";
			str += $"CameraMatrix,";
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					str += $"{CameraMatrix.At<double>(i, j)},";
			str += "\n";
			str += $"DistortionCoeffs,";
			for (int i = 0; i < DistortionCoeffs.Rows; i++)
					str += $"{DistortionCoeffs.At<double>(i)},";
			return str;
		}

		private static IntrinsicCameraParameters ParseText(string str)
        {
			var lines = str.Split("\n");
			var l0 = lines[0].Split(",");
			var l1 = lines[1].Split(",");
			var l2 = lines[2].Split(",");
			if (l0[0] != "ImageSize" || l1[0] != "CameraMatrix" || l2[0] != "DistortionCoeffs")
				throw new InvalidDataException();
			var size = new Size(int.Parse(l0[1]), int.Parse(l0[2]));
			var cam = new Mat(3, 3, MatType.CV_64F);
			for (int i = 0; i < 3; i++)
				for (int j = 0; j < 3; j++)
					cam.At<double>(i, j) = double.Parse(l1[1 + i * 3 + j]);
			var dis = new Mat(l2.Length - 2, 1, MatType.CV_64F);
			for (int i = 0; i < dis.Rows; i++)
				dis.At<double>(i) = double.Parse(l2[1 + i]);
			return new(size, cam, dis);
        }

	}

}
