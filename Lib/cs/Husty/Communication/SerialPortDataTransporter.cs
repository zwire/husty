using System.IO.Ports;

namespace Husty.Communication;

public sealed class SerialPortDataTransporter : DataTransporterBase
{

  // ------- fields ------- //

  private readonly SerialPort _port;


  // ------ properties ------ //

  public override Stream BaseWritingStream => _port.BaseStream;

  public override Stream BaseReadingStream => _port.BaseStream;


  // ------- constructors ------- //

  public SerialPortDataTransporter(
    string portName,
    int baudRate,
    StopBits stopBits = StopBits.One,
    Handshake handshake = default,
    Parity parity = default,
    int readTimeout = -1,
    int writeTimeout = -1
  )
  {
    _port = new()
    {
      PortName = portName,
      BaudRate = baudRate,
      StopBits = stopBits,
      Handshake = handshake,
      Parity = parity,
      ReadTimeout = readTimeout,
      WriteTimeout = writeTimeout
    };
    try
    {
      _port.Open();
      _port.DiscardInBuffer();
      _port.DiscardOutBuffer();
    }
    catch
    {
      throw new Exception($"failed to open {portName}");
    }
  }


  // ------ protected methods ------ //

  protected override void DoDispose()
  {
    _port.Close();
  }

  protected override Task<bool> DoTryWriteAsync(byte[] data, CancellationToken ct)
  {
    _port.Encoding = Encoding;
    _port.NewLine = NewLine;
    try
    {
      if (!_port.IsOpen) return Task.FromResult(false);
      _port.Write(data, 0, data.Length);
      return Task.FromResult(true);
    }
    catch
    {
      return Task.FromResult(false);
    }
  }

  protected override Task<Result<byte[]>> DoTryReadAsync(int count, CancellationToken ct)
  {
    _port.Encoding = Encoding;
    _port.NewLine = NewLine;
    var buf = new byte[count];
    var progress = 0;
    while (progress < count)
    {
      if (!_port.IsOpen) return Task.FromResult(Result<byte[]>.Err(new("port is not open")));
      progress += _port.Read(buf, progress, count - progress);
    }
    return Task.FromResult(Result<byte[]>.Ok(buf));
  }

  protected override Task<bool> DoTryWriteLineAsync(string data, CancellationToken ct)
  {
    _port.Encoding = Encoding;
    _port.NewLine = NewLine;
    try
    {
      if (!_port.IsOpen) return Task.FromResult(false);
      _port.WriteLine(data);
      return Task.FromResult(true);
    }
    catch
    {
      return Task.FromResult(false);
    }
  }

  protected override Task<Result<string>> DoTryReadLineAsync(CancellationToken ct)
  {
    _port.Encoding = Encoding;
    _port.NewLine = NewLine;
    try
    {
      if (!_port.IsOpen) return Task.FromResult(Result<string>.Err(new("port is not open")));
      var data = _port.ReadLine();
      if (data is null || data.Length is 0) return Task.FromResult(Result<string>.Err(new("failed to read data")));
      return Task.FromResult(Result<string>.Ok(data));
    }
    catch
    {
      return Task.FromResult(Result<string>.Err(new("failed to read data")));
    }
  }


  // ------ static methods ------ //

  public static string[] GetPortNames()
  {
    return SerialPort.GetPortNames();
  }

}
