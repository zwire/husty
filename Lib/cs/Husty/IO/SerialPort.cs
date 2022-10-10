using System.IO.Ports;

namespace Husty.IO;

public class SerialPort : IDisposable
{

    // ------- fields ------- //

    private readonly System.IO.Ports.SerialPort _port;
    private readonly CancellationTokenSource _cts;


    // ------- constructors ------- //

    public SerialPort(
        string portName, 
        int baudRate, 
        StopBits stopBits = StopBits.One,
        Handshake handshake = default,
        Parity parity = default,
        int readTimeout = -1, 
        int writeTimeout = -1, 
        string newLine = "\n"
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
            WriteTimeout = writeTimeout,
            NewLine = newLine
        };
        try
        {
            _port.Open();
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
            _cts = new();
        }
        catch
        {
            throw new Exception($"failed to open {portName}");
        }
    }


    // ------- public methods ------- //

    public bool Write(byte[] value)
    {
        if (_cts.IsCancellationRequested) return false;
        try
        {
            if (_port.IsOpen is true) _port.Write(value, 0, value.Length);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Write(string value)
    {
        if (_cts.IsCancellationRequested) return false;
        try
        {
            if (_port.IsOpen is true) _port.Write(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool WriteLine(string value)
    {
        if (_cts.IsCancellationRequested) return false;
        try
        {
            if (_port.IsOpen is true) _port.WriteLine(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public byte[] Read(int size)
    {
        if (_cts.IsCancellationRequested) return Array.Empty<byte>();
        var buf = new byte[size];
        var progress = 0;
        while (progress < size)
            progress += _port.Read(buf, progress, size - progress);
        return buf;
    }

    public string? ReadLine()
    {
        if (_cts.IsCancellationRequested) return null;
        try
        {
            return _port.IsOpen ? _port.ReadLine() : null;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _port.Close();
    }

    public static string[] GetPortNames()
    {
        return System.IO.Ports.SerialPort.GetPortNames();
    }

}
