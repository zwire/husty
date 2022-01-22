using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Husty.SkywayGateway
{
    public class DataStream : IDisposable
    {

        // ------ fields ------ //

        private readonly UdpClient _client;
        private readonly DataConnectionInfo _info;
        private readonly byte[] _localId;
        private readonly byte[] _remoteId;


        // ------ properties ------ //

        public DataConnectionInfo ConnectionInfo => _info;


        // ------ constructors ------ //

        internal DataStream(DataConnectionInfo info, string localId, string remoteId)
        {
            _client = new(info.LocalEP.Port);
            _info = info;
            var localIdBytes = Encoding.UTF8.GetBytes(localId);
            var remoteIdBytes = Encoding.UTF8.GetBytes(remoteId);
            _localId = new byte[localIdBytes.Length + 2];
            Array.Copy(localIdBytes, _localId, localIdBytes.Length);
            _localId[^2] = (byte)(localIdBytes.Length >> 8);
            _localId[^1] = (byte)localIdBytes.Length;
            _remoteId = new byte[remoteIdBytes.Length + 2];
            Array.Copy(remoteIdBytes, _remoteId, remoteIdBytes.Length);
            _remoteId[^2] = (byte)(remoteIdBytes.Length >> 8);
            _remoteId[^1] = (byte)remoteIdBytes.Length;
        }


        // ------ public methods ------ //

        public void Dispose()
        {
            _client.Close();
            _client.Dispose();
        }

        public bool WriteBinary(byte[] bytes)
        {
            var buf = AddFooter(bytes);
            try
            {
                _client.Send(buf, buf.Length, _info.RemoteEP);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> WriteBinaryAsync(byte[] bytes)
        {
            var buf = AddFooter(bytes);
            try
            {
                await _client.SendAsync(buf, buf.Length, _info.RemoteEP);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public byte[] ReadBinary()
        {
            while (true)
            {
                IPEndPoint ep = null;
                var bytes = _client.Receive(ref ep);
                return TrimFooter(bytes);
            }
        }

        public async Task<byte[]> ReadBinaryAsync()
        {
            while (true)
            {
                var rcv = await _client.ReceiveAsync();
                return TrimFooter(rcv.Buffer);
            }
        }

        public bool WriteString(string sendmsg) 
            => WriteBinary(Encoding.UTF8.GetBytes(sendmsg));

        public async Task<bool> WriteStringAsync(string sendmsg) 
            => await WriteBinaryAsync(Encoding.UTF8.GetBytes(sendmsg));

        public string ReadString()
            => Encoding.UTF8.GetString(ReadBinary());

        public async Task<string> ReadStringAsync()
            => Encoding.UTF8.GetString(await ReadBinaryAsync());

        public bool WriteAsJson<T>(T sendmsg)
            => WriteString(JsonSerializer.Serialize(sendmsg));

        public async Task<bool> WriteAsJsonAsync<T>(T sendmsg)
            => await WriteStringAsync(JsonSerializer.Serialize(sendmsg));

        public T ReadAsJson<T>()
            => JsonSerializer.Deserialize<T>(ReadString());

        public async Task<T> ReadAsJsonAsync<T>()
            => JsonSerializer.Deserialize<T>(await ReadStringAsync());


        // ------ private methods ------ //
        
        private byte[] AddFooter(byte[] bytes)
        {
            var buf = new byte[bytes.Length + _localId.Length];
            Array.Copy(bytes, buf, bytes.Length);
            Array.Copy(_localId, 0, buf, bytes.Length, _localId.Length);
            return buf;
        }

        private byte[] TrimFooter(byte[] bytes)
        {
            var span = bytes.AsSpan();
            var len = span[^2] << 8 | span[^1];
            var dataLength = span.Length - len - 2;
            var id1 = span.Slice(dataLength, len);
            var id2 = _remoteId.AsSpan()[0..^2];
            if (id1.Length != id2.Length) 
                return null;
            for (int i = 0; i < id1.Length; i++)
                if (id1[i] != id2[i])
                    return null;
            return span[..dataLength].ToArray();
        }

    }
}
