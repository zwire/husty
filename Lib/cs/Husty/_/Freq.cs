namespace Husty;

public struct Freq
{

  public TimeSpan TimeSpan { get; }

  public Freq(double heltz)
  {
    if (heltz <= 0)
      throw new ArgumentOutOfRangeException("Frequency must be > 0");
    TimeSpan = TimeSpan.FromMilliseconds(1000 / heltz);
  }

  public Freq(TimeSpan timeSpan)
  {
    TimeSpan = timeSpan;
  }

}
