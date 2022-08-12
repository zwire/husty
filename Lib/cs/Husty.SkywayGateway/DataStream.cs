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
            _client.Close();
            _client.Dispose();
        }
        _disposed = true;
    }

    public bool WriteBinary(byte[] bytes)
    {
        var sent = 0;
        var len = bytes.Length;
        while (sent < len)
            sent += _client.Send(bytes[sent..], len - sent, _info.RemoteEP);
        return true;
    }

    public async Task<bool> WriteBinaryAsync(byte[] bytes)
    {
        var sent = 0;
        var len = bytes.Length;
        while (sent < len)
            sent += await _client.SendAsync(bytes[sent..], len - sent, _info.RemoteEP).ConfigureAwait(false);
        return true;
    }

    public byte[] ReadBinary()
    {
        IPEndPoint ep = null;
        return _client.Receive(ref ep);
    }

    public async Task<byte[]> ReadBinaryAsync()
    {
        return (await _client.ReceiveAsync().ConfigureAwait(false)).Buffer;
    }

    public bool WriteString(string sendmsg) 
        => WriteBinary(Encoding.UTF8.GetBytes(sendmsg));

    public async Task<bool> WriteStringAsync(string sendmsg) 
        => await WriteBinaryAsync(Encoding.UTF8.GetBytes(sendmsg)).ConfigureAwait(false);

    public string ReadString()
        => Encoding.UTF8.GetString(ReadBinary());

    public async Task<string> ReadStringAsync()
        => Encoding.UTF8.GetString(await ReadBinaryAsync().ConfigureAwait(false));

    public bool WriteAsJson<T>(T sendmsg)
        => WriteString(JsonSerializer.Serialize(sendmsg));

    public async Task<bool> WriteAsJsonAsync<T>(T sendmsg)
        => await WriteStringAsync(JsonSerializer.Serialize(sendmsg)).ConfigureAwait(false);

    public T ReadAsJson<T>()
        => JsonSerializer.Deserialize<T>(ReadString());

    public async Task<T> ReadAsJsonAsync<T>()
        => JsonSerializer.Deserialize<T>(await ReadStringAsync().ConfigureAwait(false));

}
