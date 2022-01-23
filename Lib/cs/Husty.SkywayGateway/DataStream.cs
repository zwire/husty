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
            try
            {
                _client.Send(bytes, bytes.Length, _info.RemoteEP);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> WriteBinaryAsync(byte[] bytes)
        {
            try
            {
                await _client.SendAsync(bytes, bytes.Length, _info.RemoteEP);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public byte[] ReadBinary()
        {
            IPEndPoint ep = null;
            return _client.Receive(ref ep);
        }

        public async Task<byte[]> ReadBinaryAsync()
        {
            return (await _client.ReceiveAsync()).Buffer;
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

    }
}
