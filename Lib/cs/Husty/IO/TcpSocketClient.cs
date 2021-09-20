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


        // ------- Constructors ------- //

        public TcpSocketClient(string ip, int inoutPort)
        {
            _client1 = new();
            Task.Run(() =>
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
            Task.Run(() =>
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

        public bool WaitForConnect()
        {
            return WaitForConnectAsync().Result;
        }

        public async Task<bool> WaitForConnectAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (_client2 is null)
                    {
                        while (!_client1.Connected) ;
                    }
                    else
                    {
                        while (!_client1.Connected) ;
                        while (!_client2.Connected) ;
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public BidirectionalDataStream GetStream()
        {
            return GetStreamAsync().Result;
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.Run(() =>
            {
                WaitForConnect();
                if (_client2 is null)
                {
                    var stream = _client1.GetStream();
                    return new BidirectionalDataStream(stream, stream);
                }
                else
                {
                    var stream1 = _client1.GetStream();
                    var stream2 = _client2.GetStream();
                    return new BidirectionalDataStream(stream1, stream2);
                }
            });
        }

        public void Dispose()
        {
            _client1?.Dispose();
            _client2?.Dispose();
        }

    }
}
