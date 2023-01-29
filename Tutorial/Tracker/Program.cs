using OpenCvSharp;
using OpenCvSharp.Tracking;

// open camera
using var cap = new VideoCapture(0);
using var frame = new Mat();

// initialize tracker
using var tracker = TrackerCSRT.Create();

// loop
var box = new Rect();
while (cap.Read(frame))
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey().Key;
        if (key is ConsoleKey.Enter)
        {
            // input user-defined box
            box = Cv2.SelectROI(frame);
            tracker.Init(frame, box);
            Cv2.DestroyAllWindows();
        }
        else if (key is ConsoleKey.Escape)
        {
            break;
        }
    }

    // check if box exists
    if (box.Size != Size.Zero)
        tracker.Update(frame, ref box);

    // visualize
    Cv2.Rectangle(frame, box, new(0, 0, 255), 2);
    Cv2.ImShow(" ", frame);
    Cv2.WaitKey(1);

}
Console.WriteLine("completed");