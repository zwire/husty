using System.Diagnostics;

namespace Husty;

public class LapTimer : IDisposable
{

  private readonly Stopwatch _watch;

  public LapTimer()
  {
    _watch = new();
    _watch.Start();
  }

  public long GetLapTime()
  {
    _watch.Stop();
    var dt = _watch.ElapsedMilliseconds;
    _watch.Restart();
    return dt;
  }

  public void Dispose()
  {
    _watch?.Stop();
  }

}
