using System.Text;
using static Husty.Lawicel.CANUSB;
using static Husty.Lawicel.CanUsbOption;

namespace Husty.Lawicel;

public class CanUsbAdapter : ICanUsbAdapter
{

  // ------ fields ------ //

  private bool _disposed;
  private uint _handle;


  // ------ properties ------ //

  public string AdapterName { get; }

  public string Baudrate { get; }

  public CanUsbStatus Status { private set; get; } = CanUsbStatus.Offline;


  // ------ constructors ------ //

  public CanUsbAdapter(string adapterName, string baudrate)
  {
    AdapterName = adapterName;
    Baudrate = baudrate;
  }


  // ------ public methods ------ //

  public static string[] FindAdapterNames()
  {
    var adapterList = new List<string>();
    var buf = new StringBuilder(32);
    int numAdapters = canusb_getFirstAdapter(buf, 32);
    if (numAdapters is 0) return adapterList.ToArray();
    adapterList.Add(buf.ToString());
    for (int i = 1; i < numAdapters; i++)
      if (canusb_getNextAdapter(buf, 32) > 0)
        adapterList.Add(buf.ToString());
    return adapterList.ToArray();

  }

  public void Open()
  {
    ThrowIfDisposed();
    ThrowIfNotOffline();
    _handle = canusb_Open(
        AdapterName,
        Baudrate,
        ACCEPTANCE_CODE_ALL, ACCEPTANCE_MASK_ALL,
        FLAG_TIMESTAMP | FLAG_BLOCK
    );
    if (_handle <= 0) throw new Exception("failed to open the CANUSB Adapter.");
    Status = CanUsbStatus.Online;
  }

  public void Close()
  {
    if (_disposed) return;
    if (Status is CanUsbStatus.Offline) return;
    var ret = canusb_Close(_handle);
    if (ret is not ERROR_OK) throw new Exception("failed to close the CANUSB adapter");
    Status = CanUsbStatus.Offline;
  }


  public void Write(CanMessage message)
  {
    ThrowIfDisposed();
    ThrowIfNotOnline();
    var msg = message.ToCANMsg();
    canusb_Flush(_handle, Convert.ToByte(FLUSH_WAIT));
    var ret = canusb_Write(_handle, ref msg);
    if (ret is not ERROR_OK) throw new Exception("failed to send the message.");
  }

  public CanMessage Read()
  {
    var ret = canusb_Read(_handle, out var message);
    if (ret is not ERROR_OK) throw new Exception("failed to receive the message.");
    return CanMessage.FromCANMsg(message);
  }

  public void Dispose()
  {
    if (!_disposed)
    {
      Close();
      GC.SuppressFinalize(this);
      _disposed = true;
    }
  }


  // ------ protected methods ------ //

  protected void ThrowIfNotOffline()
  {
    if (Status is not CanUsbStatus.Offline)
      throw new InvalidOperationException("must be invoked on Offline status.");
  }

  protected void ThrowIfNotOnline()
  {
    if (Status is not CanUsbStatus.Online)
      throw new InvalidOperationException("must be invoked on Online status.");
  }

  protected void ThrowIfDisposed()
  {
    if (_disposed) throw new ObjectDisposedException(GetType().FullName);
  }

}

