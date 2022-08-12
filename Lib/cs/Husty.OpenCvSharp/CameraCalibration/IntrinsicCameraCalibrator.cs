using OpenCvSharp;

namespace Husty.OpenCvSharp.CameraCalibration;

public static class IntrinsicCameraCalibrator
{

	public static IntrinsicCameraParameters CalibrateWithChessboardImages(Chessboard chessboard, IEnumerable<Mat> images)
	{
		return CalibrateWithChessboardImages(
			chessboard, images,
			image =>
			{
				var corners = chessboard.FindCorners(image);
				return (image.Size(), corners);
			});
	}

	public static IntrinsicCameraParameters CalibrateWithChessboardImages(Chessboard chessboard, IEnumerable<string> imageFiles)
	{
		return CalibrateWithChessboardImages(
			chessboard, imageFiles,
			imageFile =>
			{
				using var image = new Mat(imageFile, ImreadModes.Grayscale);
				var corners = chessboard.FindCorners(image);
				return (image.Size(), corners);
			});
	}

	private static IntrinsicCameraParameters CalibrateWithChessboardImages<TImage>(
		Chessboard chessboard, IEnumerable<TImage> sources,
		Func<TImage, (Size Size, Point2f[] Point)> func)
	{
		var imageSize = Size.Zero;
		var objectPointsList = new List<Point3f[]>();
		var imagePointsList = new List<Point2f[]>();
		foreach (var source in sources)
		{
			var (size, imageCorners) = func(source);
			if (imageSize == Size.Zero) imageSize = size;
			if (size != imageSize) throw new ArgumentException("All images must have a same size.");
			if (imageCorners is null) continue;
			imagePointsList.Add(imageCorners);
			var worldCorners = chessboard.GetWorldCorners();
			objectPointsList.Add(worldCorners);
		}
		var objectPoints = objectPointsList.ToArray();
		var imagePoints = imagePointsList.ToArray();
		return CalibrateWithCorrespoindingPoints(objectPoints, imagePoints, imageSize);
	}


	private static IntrinsicCameraParameters CalibrateWithCorrespoindingPoints(
		Point3f[][] objectPoints,
		Point2f[][] imagePoints,
		Size imageSize)
	{

		if (objectPoints.Length != imagePoints.Length)
			throw new ArgumentException("Requires: objectPoints.Length == imagePoints.Length");
		if (imagePoints.Length < 10)
			throw new ArgumentException("Requires: imagePoints.Length >= 10", nameof(imagePoints));

		var cam = new double[3, 3];
		var dist = new double[5];

		Cv2.CalibrateCamera(objectPoints, imagePoints, imageSize, cam, dist, out _, out _);

		var camMat = new Mat(3, 3, MatType.CV_64F, cam);
		var distCoeffs = new Mat(5, 1, MatType.CV_64F, dist);

		return new IntrinsicCameraParameters(imageSize, camMat, distCoeffs);

	}

}
