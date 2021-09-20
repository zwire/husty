using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Husty.IO
{
    /// <summary>
    /// Tcp socket server class
    /// </summary>
    public class TcpSocketServer : ICommunicator
    {

        // ------- Fields ------- //

        private TcpClient _client1;
        private TcpClient _client2;
        private readonly TcpListener _listener1;
        private readonly TcpListener _listener2;


        // ------- Constructors ------- //

        public TcpSocketServer(int inoutPort)
        {
            _listener1 = new(IPAddress.Any, inoutPort);
            _listener1.Start();
            Task.Run(() =>
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

        public TcpSocketServer(int inPort, int outPort)
        {
            _listener1 = new(IPAddress.Any, inPort);
            _listener2 = new(IPAddress.Any, outPort);
            _listener1.Start();
            _listener2.Start();
            Task.Run(() =>
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
                    if (_listener2 is null)
                    {
                        while (_client1 is null) ;
                    }
                    else
                    {
                        while (_client1 is null) ;
                        while (_client2 is null) ;
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
                if (_client2 is not null)
                {
                    var stream1 = _client1.GetStream();
                    var stream2 = _client2.GetStream();
                    return new BidirectionalDataStream(stream2, stream1);
                }
                else if (_client1 is not null)
                {
                    var stream = _client1.GetStream();
                    return new BidirectionalDataStream(stream, stream);
                }
                else
                {
                    throw new Exception("Client instance has not been created yet!");
                }
            });
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
