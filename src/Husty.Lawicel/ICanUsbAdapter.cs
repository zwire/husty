namespace Husty.Lawicel;

public enum CanUsbStatus { Offline, Online }

public interface ICanUsbAdapter : IDisposable
{

  public string AdapterName { get; }

  public string Baudrate { get; }

  public CanUsbStatus Status { get; }

  public void Open();

  public void Close();

  public void Write(CanMessage message);

  public CanMessage Read();

}
