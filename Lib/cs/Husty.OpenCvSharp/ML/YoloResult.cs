using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public record YoloResult
    {

        public string Label { get; init; }

        public float Confidence { get; init; }

        public Rect Box { get; init; }

        public Point Center => new(Box.Left + Box.Width / 2, Box.Top + Box.Height / 2);

        public void DrawPoint(Mat image, Scalar color)
        {
            image.Circle(Center, 3, color, 5);
        }

        public void DrawRect(Mat image, Scalar color)
        {
            var txt = $"{Label}{Confidence * 100:0}%";
            Cv2.Rectangle(image, Box, color, 2);
            var textSize = Cv2.GetTextSize(txt, HersheyFonts.HersheyTriplex, 0.3, 0, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(Box.Left, Box.Top - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), color, Cv2.FILLED);
            var textColor = Cv2.Mean(color).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, txt, new Point(Box.Left, Box.Top - baseline), HersheyFonts.HersheyTriplex, 0.3, textColor);
        }

    }

}
