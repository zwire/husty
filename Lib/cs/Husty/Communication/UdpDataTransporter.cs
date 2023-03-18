using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Husty.Communication;

public sealed class UdpDataTransporter : DataTransporterBase
{

    // ------ fields ------ //

    private UdpClient? _sender;
    private UdpClient? _receiver;
    private IPEndPoint[]? _targets;


    // ------ properties ------ //

    public override Stream BaseWritingStream => throw new NotImplementedException();

    public override Stream BaseReadingStream => throw new NotImplementedException();


    // ------ public methods ------ //

    public UdpDataTransporter SetListeningPort(int port)
    {
        _receiver = new(port);
        return this;
    }

    public UdpDataTransporter SetTargetPorts(params IPEndPoint[] ep)
    {
        _sender = new();
        _targets = ep;
        return this;
    }

    public UdpDataTransporter SetTargetPorts(params int[] ports)
    {
        return SetTargetPorts(ports.Select(p => new IPEndPoint(IPAddress.Loopback, p)).ToArray());
    }


    // ------ inherited methods ------ //

    protected override void DoDispose()
    {
        _receiver?.Close();
        _sender?.Close();
    }

    protected override async Task<bool> DoTryWriteAsync(byte[] data, CancellationToken ct)
    {
        if (_targets?.Length > 0 && _sender is UdpClient s)
        {
            try
            {
                foreach (var ep in _targets)
                    await s.SendAsync(data, ep, ct).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }
        throw new InvalidOperationException("sender is not set");
    }

    protected override async Task<ResultExpression<byte[]>> DoTryReadAsync(int count, CancellationToken ct)
    {
        if (_receiver is UdpClient r)
        {
            try
            {
                var data = await r.ReceiveAsync(ct).ConfigureAwait(false);
                if (data.Buffer.Length is 0) return new(false, default!);
                return new(true, data.Buffer);
            }
            catch
            {
                return new(false, default!);
            }
        }
        throw new InvalidOperationException("receiver is not set");
    }

    protected override async Task<bool> DoTryWriteLineAsync(string data, CancellationToken ct)
    {
        return await DoTryWriteAsync(Encoding.GetBytes(data + NewLine), ct).ConfigureAwait(false);
    }

    protected override async Task<ResultExpression<string>> DoTryReadLineAsync(CancellationToken ct)
    {
        var txt = "";
        while (true)
        {
            var (success, data) = await DoTryReadAsync(0, ct).ConfigureAwait(false);
            if (!success) return new(false, default!);
            txt += Encoding.GetString(data);
            if (NewLine is "" || txt.Contains(NewLine)) return new(true, txt.TrimEnd());
        }
    }

}
