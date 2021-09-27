using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Husty.IO
{
    /// <summary>
    /// Tcp socket client class
    /// </summary>
    public class TcpSocketClient : ICommunicator
    {

        // ------- Fields ------- //

        private TcpClient _client1;
        private TcpClient _client2;
        private readonly Task _connectionTask;


        // ------- Properties ------- //

        public int ReadTimeout { set; get; } = -1;

        public int WriteTimeout { set; get; } = -1;


        // ------- Constructors ------- //

        public TcpSocketClient(string ip, int inoutPort)
        {
            _client1 = new();
            _connectionTask = Task.Run(() =>
            {
                try
                {
                    _client1 = new(ip, inoutPort);
                }
                catch
                {
                    throw new Exception("failed to connect!");
                }
            });
        }

        public TcpSocketClient(string ip, int inPort, int outPort)
        {
            _client1 = new();
            _client2 = new();
            _connectionTask = Task.Run(() =>
            {
                try
                {
                    _client1 = new(ip, outPort);
                    _client2 = new(ip, inPort);
                }
                catch
                {
                    throw new Exception("failed to connect!");
                }
            });
        }


        // ------- Methods ------- //

        public BidirectionalDataStream GetStream()
        {
            _connectionTask.Wait();
            if (_client1 is not null && _client2 is null)
            {
                var stream = _client1.GetStream();
                return new BidirectionalDataStream(stream, stream, ReadTimeout, WriteTimeout);
            }
            else if (_client1 is not null && _client2 is not null)
            {
                var stream1 = _client1.GetStream();
                var stream2 = _client2.GetStream();
                return new BidirectionalDataStream(stream1, stream2, ReadTimeout, WriteTimeout);
            }
            else
            {
                throw new Exception("failed to get stream!");
            }
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.Run(() => GetStream());
        }

        public void Dispose()
        {
            _client1?.Dispose();
            _client2?.Dispose();
        }

    }
}
