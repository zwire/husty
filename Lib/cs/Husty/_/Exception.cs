namespace Husty;

public class HustyInternalException : Exception
{
  public HustyInternalException(string? message = null, Exception? inner = null)
      : base(message, inner) { }
}
