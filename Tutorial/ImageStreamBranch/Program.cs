using Husty.Extensions;
using Husty.OpenCvSharp.ImageStream;
using OpenCvSharp;

using var cap = new CameraStream(0);

cap.ImageSequence.Subscribe(x =>
{
  Cv2.ImShow("1", x);
  Cv2.WaitKey(1);
});
cap.ImageSequence.Subscribe(x =>
{
  Cv2.ImShow("2", x);
  Cv2.WaitKey(1);
});
Task.Run(() =>
{
  while (true)
  {
    var f = cap.Read();
    Cv2.ImShow("3", f);
    Cv2.WaitKey(1);
  }
});
ConsoleEx.WaitKey(ConsoleKey.Enter);