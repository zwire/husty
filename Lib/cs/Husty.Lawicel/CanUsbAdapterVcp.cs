// https://www.canusb.com/documents/canusb_manual.pdf

using System.Globalization;
using System.IO.Ports;

namespace Husty.Lawicel;

public class CanUsbAdapterVcp : ICanUsbAdapter
{

    // ------ fields ------ //

    private bool _disposed;
    private readonly string _baudrate;
    private readonly SerialPort _port;


    // ------ properties ------ //

    public string AdapterName { get; }

    public string Baudrate { get; }

    public CanUsbStatus Status { private set; get; } = CanUsbStatus.Offline;


    // ------ constructors ------ //

    public CanUsbAdapterVcp(string portName, string baudrate)
    {
        _baudrate = ParseBaudrate(baudrate);
        _port = new()
        {
            PortName = portName,
            BaudRate = 9600,
            NewLine = "\r"
        };
    }


    // ------ public methods ------ //

    public static string[] FindAdapterNames(string baudrate)
    {
        var list = new List<string>();
        foreach (var portName in SerialPort.GetPortNames())
        {
            using var port = new SerialPort()
            {
                PortName = portName,
                BaudRate = 9600,
                NewLine = "\r"
            };
            port.Open();
            Thread.Sleep(20);
            if (port.IsOpen)
            {
                InitPort(port, ParseBaudrate(baudrate));
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(20);
                    port.WriteLine("N");
                    var line = port.ReadLine();
                    if (
                        TryParseSerialNumber(line, out _) || 
                        TryParseMsgFrame(line, out _) ||
                        TryParseRtrMsgFrame(line, out _)
                    )
                    {
                        list.Add(portName);
                        break;
                    }
                }
            }
        }
        return list.ToArray();
    }

    public void Open()
    {
        _port.Open();
        InitPort(_port, _baudrate);
        Status = CanUsbStatus.Online;
    }

    public void Close()
    {
        if (_disposed) return;
        if (Status is CanUsbStatus.Offline) return;
        if (_port.IsOpen)
        {
            Status = CanUsbStatus.Offline;
            _port.WriteLine("C");
            Thread.Sleep(100);
            _port.Close();
        }
    }

    public void Write(CanMessage message)
    {
        if (_disposed) return;
        var msg = message.Flags is CanUsbOption.EXTENDED ? "T" : "t";
        msg += message.Id.ToString(msg is "t" ? "X3" : "X8");
        msg += message.Length;
        foreach (var d in BitConverter.GetBytes(message.Data))
            msg += d.ToString("X2");
        _port.WriteLine(msg);
    }

    public CanMessage Read()
    {
        if (_disposed) return null;
        if (_port.IsOpen)
        {
            CanMessage msg = null;
            var line = _port.ReadLine();
            if (
                TryParseMsgFrame(line, out msg) &&
                TryParseRtrMsgFrame(line, out msg)
            )
            {
                return msg;
            }
        }
        return null;
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


    // ------ private methods ------ //

    private static void InitPort(SerialPort port, string baudrate)
    {
        port.DiscardInBuffer();
        port.DiscardOutBuffer();
        port.WriteLine("C");
        port.WriteLine("S" + baudrate);
        port.WriteLine("Z1");
        port.WriteLine("O");
    }

    private static string ParseBaudrate(string baudrate)
    {
        return baudrate switch
        {
            CanUsbOption.BAUD_10K => "0",
            CanUsbOption.BAUD_20K => "1",
            CanUsbOption.BAUD_50K => "2",
            CanUsbOption.BAUD_100K => "3",
            CanUsbOption.BAUD_125K => "4",
            CanUsbOption.BAUD_250K => "5",
            CanUsbOption.BAUD_500K => "6",
            CanUsbOption.BAUD_800K => "7",
            CanUsbOption.BAUD_1M => "8",
            _ => throw new Exception()
        };
    }

    private static bool TryParseSerialNumber(string line, out int id)
    {
        id = default;
        var mode = line.FirstOrDefault();
        if (mode is 'N')
        {
            if (line.Length is 5)
            {
                return int.TryParse(line[1..4], NumberStyles.AllowHexSpecifier, null, out id);
            }
        }
        return false;
    }

    private static bool TryParseMsgFrame(string line, out CanMessage msg)
    {
        msg = null;
        var mode = line.FirstOrDefault();
        if (mode is 't')
        {
            if (
                line.Length > 6 &&
                uint.TryParse(line[1..4], NumberStyles.AllowHexSpecifier, null, out var id) &&
                byte.TryParse(line[4..5], out var len)
            )
            {
                var pos = 5 + len * 2;
                if (
                    ulong.TryParse(line[5..pos], NumberStyles.AllowHexSpecifier, null, out var data) &&
                    uint.TryParse(line[pos..], NumberStyles.AllowHexSpecifier, null, out var timestamp)
                )
                {
                    var ary = BitConverter.GetBytes(data).Reverse().ToArray();
                    msg = new(id, ary, default, len, timestamp);
                    return true;
                }
            }
        }
        return false;
    }

    private static bool TryParseRtrMsgFrame(string line, out CanMessage msg)
    {
        msg = null;
        var mode = line.FirstOrDefault();
        if (mode is 'T')
        {
            if (
                line.Length > 11 &&
                uint.TryParse(line[1..9], NumberStyles.AllowHexSpecifier, null, out var id) &&
                byte.TryParse(line[9..10], out var len)
            )
            {
                var pos = 10 + len * 2;
                if (
                    ulong.TryParse(line[10..pos], NumberStyles.AllowHexSpecifier, null, out var data) &&
                    uint.TryParse(line[pos..], NumberStyles.AllowHexSpecifier, null, out var timestamp)
                )
                {
                    var ary = BitConverter.GetBytes(data).Reverse().ToArray();
                    msg = new(id, ary, default, len, timestamp);
                    return true;
                }
            }
        }
        return false;
    }
}
