using OpenCvSharp;

namespace Husty.OpenCvSharp;

public interface IInteractiveWindow : IDisposable
{
    public string Name { get; }

    public double Ratio { get; }

    public double WheelSpeed { get; }

    public Size WindowSize { get; }

    public Mat Canvas { get; }

    public Rect ROI { get; }

    public IObservable<string> KeyInvoked { get; }

    public void ClickDown(Point point);

    public void ClickUp(Point point);

    public void Move(Point point);

    public void Drag(Point point);

    public void Cancel();
}
