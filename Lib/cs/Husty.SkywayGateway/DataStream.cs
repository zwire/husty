using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;

namespace Husty.SkywayGateway;

public class DataStream : IDisposable
{

    // ------ fields ------ //
    
    private bool _disposed;
    private readonly UdpClient _client;
    private readonly DataConnectionInfo _info;


    // ------ properties ------ //

    public bool IsDisposed => _disposed;


    // ------ constructors ------ //

    internal DataStream(DataConnectionInfo info)
    {
        _client = new(info.LocalEP.Port);
        _info = info;
    }


    // ------ public methods ------ //

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _client.Close();
            _client.Dispose();
        }
    }

    public bool WriteBinary(byte[] bytes)
    {
        try
        {
            var sent = 0;
            var len = bytes.Length;
            while (sent < len)
                sent += _client.Send(bytes[sent..], len - sent, _info.RemoteEP);
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> WriteBinaryAsync(byte[] bytes, CancellationToken? ct = null)
    {
        try
        {
            var sent = 0;
            var len = bytes.Length;
            while (sent < len)
            {
                var m = new ReadOnlyMemory<byte>(bytes[sent..]);
                sent += await _client.SendAsync(m, _info.RemoteEP, ct ?? CancellationToken.None).ConfigureAwait(false);
            }
            return true;
        }
        catch { return false; }
    }

    public byte[] ReadBinary()
    {
        try
        {
            IPEndPoint ep = null;
            return _client.Receive(ref ep);
        }
        catch { return Array.Empty<byte>(); }
    }

    public async Task<byte[]> ReadBinaryAsync(CancellationToken? ct = null)
    {
        try
        {
            var rcv = await _client.ReceiveAsync(ct ?? CancellationToken.None).ConfigureAwait(false);
            return rcv.Buffer;
        }
        catch { return Array.Empty<byte>(); }
    }

    public bool WriteString(string sendmsg) 
        => WriteBinary(Encoding.UTF8.GetBytes(sendmsg));

    public async Task<bool> WriteStringAsync(string sendmsg, CancellationToken? ct = null) 
        => await WriteBinaryAsync(Encoding.UTF8.GetBytes(sendmsg), ct).ConfigureAwait(false);

    public string ReadString()
        => Encoding.UTF8.GetString(ReadBinary());

    public async Task<string> ReadStringAsync(CancellationToken? ct = null)
        => Encoding.UTF8.GetString(await ReadBinaryAsync(ct).ConfigureAwait(false));

    public bool WriteAsJson<T>(T sendmsg)
        => WriteString(JsonSerializer.Serialize(sendmsg));

    public async Task<bool> WriteAsJsonAsync<T>(T sendmsg, CancellationToken? ct = null)
        => await WriteStringAsync(JsonSerializer.Serialize(sendmsg), ct).ConfigureAwait(false);

    public T ReadAsJson<T>()
        => JsonSerializer.Deserialize<T>(ReadString());

    public async Task<T> ReadAsJsonAsync<T>(CancellationToken? ct = null)
        => JsonSerializer.Deserialize<T>(await ReadStringAsync(ct).ConfigureAwait(false));

}
