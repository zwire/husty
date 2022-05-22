using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Husty.IO
{

    public enum WebSocketType { Server, Client }

    public class WebSocketStream : IDisposable
    {

        // ------ fields ------ //

        private readonly WebSocket _socket;


        // ------ properties ------ //

        public bool IsOpened => _socket.State is WebSocketState.Open;

        public bool IsClosed =>
            _socket.State is WebSocketState.Closed ||
            _socket.State is WebSocketState.CloseSent ||
            _socket.State is WebSocketState.CloseReceived;

        public bool IsAborted => _socket.State is WebSocketState.Aborted;


        // ------ constructors ------ //

        public WebSocketStream(WebSocketType type, string ip, int port)
        {
            if (type is WebSocketType.Server)
            {
                var listener = new HttpListener();
                listener.Prefixes.Add($"http://{ip}:{port}/ws/");
                listener.Start();
                var context = listener.GetContextAsync().Result;
                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                    return;
                }
                _socket = context.AcceptWebSocketAsync(null).Result.WebSocket;
            }
            else
            {
                var socket = new ClientWebSocket();
                socket.ConnectAsync(new($"ws://{ip}:{port}/ws/"), CancellationToken.None).Wait();
                _socket = socket;
            }
        }


        // ------ public methods ------ //

        public void Write(byte[] data)
        {
            WriteAsync(data).Wait();
        }

        public void Write(string data, Encoding encoding)
        {
            WriteAsync(data, encoding).Wait();
        }

        public async Task WriteAsync(byte[] data, CancellationToken? ct = null)
        {
            await _socket.SendAsync(data.AsMemory(), WebSocketMessageType.Binary, true, ct ?? CancellationToken.None);
        }

        public async Task WriteAsync(string data, Encoding encoding, CancellationToken? ct = null)
        {
            await _socket.SendAsync(encoding.GetBytes(data), WebSocketMessageType.Text, true, ct ?? CancellationToken.None);
        }

        public byte[] Read()
        {
            return ReadAsync().Result;
        }

        public string Read(Encoding encoding)
        {
            return ReadAsync(encoding).Result;
        }

        public async Task<byte[]> ReadAsync(CancellationToken? ct = null)
        {
            var data = new byte[4096];
            var result = await _socket.ReceiveAsync(data.AsMemory(), ct ?? CancellationToken.None);
            if (result.MessageType is WebSocketMessageType.Close) Close();
            data = data.AsSpan(new Range(0, result.Count)).ToArray();
            while (!result.EndOfMessage)
            {
                var d0 = data;
                var d1 = new byte[4096];
                result = await _socket.ReceiveAsync(d1.AsMemory(), ct ?? CancellationToken.None);
                data = new byte[d0.Length + d1.Length];
                Array.Copy(d0, data, d0.Length);
                Array.Copy(d1, 0, data, d0.Length, d1.Length);
            }
            return data;
        }

        public async Task<string> ReadAsync(Encoding encoding, CancellationToken? ct = null)
        {
            var data = await ReadAsync(ct);
            return encoding.GetString(data);
        }

        public void Close()
        {
            if (IsClosed) return;
            _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }

        public async Task CloseAsync()
        {
            if (IsClosed) return;
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
        }

        public void Dispose()
        {
            if (!IsClosed) Close();
            _socket.Dispose();
        }

    }
}
