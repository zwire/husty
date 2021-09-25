using OpenCvSharp;

namespace Husty.OpenCvSharp
{
    public record YoloResult(Rect2d Box, float Confidence, string Label, float Probability)
    {

        public void DrawCenterPoint(Mat image, Scalar color, int pointSize)
        {
            image.Circle(Box.Scale(image.Width, image.Height).ToRect().GetCenter(), pointSize, color, pointSize + 2);
        }

        public void DrawBox(Mat image, Scalar color, int lineWidth)
        {
            DrawBox(image, color, lineWidth, false, false);
        }

        public void DrawBox(Mat image, Scalar color, int lineWidth, bool putLabel, bool putProbability, double labelFontScale = 1.0)
        {
            var b = Box.Scale(image.Width, image.Height).ToRect();
            Cv2.Rectangle(image, b, color, lineWidth);
            if (putLabel || putProbability)
            {
                var l = putLabel ? $"{Label} " : "";
                var p = putProbability ? $"{Probability * 100:f0}%" : "";
                var txt = $"{l}{p}";
                var textSize = Cv2.GetTextSize(txt, HersheyFonts.HersheyTriplex, labelFontScale, 0, out var baseline);
                var textColor = Cv2.Mean(color).Val0 < 70 ? Scalar.White : Scalar.Black;
                var labelTop = b.Top - textSize.Height - baseline;
                if (labelTop < 0) labelTop = 0;
                var labelLeft = b.Left - lineWidth / 2;
                Cv2.Rectangle(image, new Rect(new Point(labelLeft, labelTop),
                    new Size(textSize.Width, textSize.Height + baseline)), color, Cv2.FILLED);
                Cv2.PutText(image, txt, new Point(labelLeft, labelTop + textSize.Height), HersheyFonts.HersheyTriplex, labelFontScale, textColor);
            }
        }

    }

}
