using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Husty.IO;

public class UdpDataTransporter : DataTransporterBase
{

    // ------ fields ------ //

    private readonly UdpClient _receiver;
    private readonly UdpClient _sender;
    private readonly IPEndPoint[] _ep;


    // ------ constructors ------ //

    public UdpDataTransporter(int inPort, params int[] outPorts)
    {
        _receiver = new UdpClient(inPort) { EnableBroadcast = true };
        _sender = new UdpClient() { EnableBroadcast = true };
        _ep = outPorts.Select(p => new IPEndPoint(IPAddress.Broadcast, p)).ToArray();
    }


    // ------ inherited methods ------ //

    protected override void DoDispose()
    {
        _receiver.Close();
        _sender.Close();
    }

    protected override async Task<bool> DoTryWriteAsync(byte[] data, CancellationToken ct)
    {
        try
        {
            foreach (var ep in _ep)
                await _sender.SendAsync(data, ep, ct).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected override async Task<ResultExpression<byte[]>> DoTryReadAsync(int count, CancellationToken ct)
    {
        try
        {
            var data = await _receiver.ReceiveAsync(ct).ConfigureAwait(false);
            if (data.Buffer.Length is 0) return new(false, default!);
            return new(true, data.Buffer);
        }
        catch
        {
            return new(false, default!);
        }
    }

    protected override async Task<bool> DoTryWriteLineAsync(string data, CancellationToken ct)
    {
        return await DoTryWriteAsync(Encoding.GetBytes(data), ct).ConfigureAwait(false);
    }

    protected override async Task<ResultExpression<string>> DoTryReadLineAsync(CancellationToken ct)
    {
        var (success, data) = await DoTryReadAsync(0, ct).ConfigureAwait(false);
        if (!success) return new(false, default!);
        return new(success, Encoding.GetString(data));
    }

}
