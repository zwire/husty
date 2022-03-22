using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Husty
{
    /// <summary>
    /// Tcp socket server class
    /// </summary>
    public sealed class TcpSocketServer : ICommunicator
    {

        // ------ fields ------ //

        private TcpClient _client1;
        private TcpClient _client2;
        private readonly TcpListener _listener1;
        private readonly TcpListener _listener2;
        private readonly Task _connectionTask;


        // ------ properties ------ //

        public int ReadTimeout { set; get; } = -1;

        public int WriteTimeout { set; get; } = -1;


        // ------ constructors ------ //

        public TcpSocketServer(int port)
        {
            _listener1 = new(IPAddress.Any, port);
            _listener1.Start();
            _connectionTask = Task.Run(() =>
            {
                try
                {
                    _client1 = _listener1.AcceptTcpClient();
                }
                catch
                {
                    throw new Exception("failed to connect!");
                }
            });
        }

        public TcpSocketServer(int recvPort, int sendPort)
        {
            _listener1 = new(IPAddress.Any, recvPort);
            _listener2 = new(IPAddress.Any, sendPort);
            _listener1.Start();
            _listener2.Start();
            _connectionTask = Task.Run(() =>
            {
                try
                {
                    _client1 = _listener1.AcceptTcpClient();
                    _client2 = _listener2.AcceptTcpClient();
                }
                catch
                {
                    throw new Exception("failed to connect!");
                }
            });
        }


        // ------ public methods ------ //

        public BidirectionalDataStream GetStream()
        {
            _connectionTask.Wait();
            if (_client1 is not null && _client2 is null)
            {
                var stream = _client1.GetStream();
                return new BidirectionalDataStream(stream, stream, WriteTimeout, ReadTimeout);
            }
            else if (_client1 is not null && _client2 is not null)
            {
                var stream1 = _client1.GetStream();
                var stream2 = _client2.GetStream();
                return new BidirectionalDataStream(stream2, stream1, WriteTimeout, ReadTimeout);
            }
            else
            {
                throw new Exception("failed to get stream!");
            }
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.FromResult(GetStream()).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _client1?.Dispose();
            _client2?.Dispose();
            _listener1?.Stop();
            _listener2?.Stop();
        }

    }
}
