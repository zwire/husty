using System.IO.Ports;

namespace Husty.Communication;

public class SerialPortDataTransporter : DataTransporterBase
{

    // ------- fields ------- //

    private readonly SerialPort _port;


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


    // ------ inherited methods ------ //

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

    protected override Task<ResultExpression<byte[]>> DoTryReadAsync(int count, CancellationToken ct)
    {
        _port.Encoding = Encoding;
        _port.NewLine = NewLine;
        var buf = new byte[count];
        var progress = 0;
        while (progress < count)
        {
            if (!_port.IsOpen) return Task.FromResult(new ResultExpression<byte[]>(true, default!));
            progress += _port.Read(buf, progress, count - progress);
        }
        return Task.FromResult(new ResultExpression<byte[]>(true, buf));
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

    protected override Task<ResultExpression<string>> DoTryReadLineAsync(CancellationToken ct)
    {
        _port.Encoding = Encoding;
        _port.NewLine = NewLine;
        try
        {
            if (!_port.IsOpen) return Task.FromResult(new ResultExpression<string>(false, default!));
            var data = _port.ReadLine();
            if (data is null || data.Length is 0) return Task.FromResult(new ResultExpression<string>(false, default!));
            return Task.FromResult(new ResultExpression<string>(true, data));
        }
        catch
        {
            return Task.FromResult(new ResultExpression<string>(false, default!));
        }
    }


    // ------ static methods ------ //

    public static string[] GetPortNames()
    {
        return SerialPort.GetPortNames();
    }

}
