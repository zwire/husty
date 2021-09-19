using System;
using System.Net;
using System.Net.Sockets;

namespace Husty.IO
{
    /// <summary>
    /// Tcp socket server class
    /// </summary>
    public class TcpSocketServer : ICommunicator
    {

        // ------- Fields ------- //

        private readonly TcpClient _client1;
        private readonly TcpClient _client2;
        private readonly TcpListener _listener1;
        private readonly TcpListener _listener2;


        // ------- Constructors ------- //

        public TcpSocketServer(int inoutPort)
        {
            try
            {
                _listener1 = new(IPAddress.Any, inoutPort);
                _listener1.Start();
                _client1 = _listener1.AcceptTcpClient();
            }
            catch
            {
                throw new Exception("failed to connect!");
            }
        }

        public TcpSocketServer(int inPort, int outPort)
        {
            try
            {
                _listener1 = new(IPAddress.Any, inPort);
                _listener2 = new(IPAddress.Any, outPort);
                _listener1.Start();
                _listener2.Start();
                _client1 = _listener1.AcceptTcpClient();
                _client2 = _listener2.AcceptTcpClient();
            }
            catch
            {
                throw new Exception("failed to connect!");
            }
        }


        // ------- Methods ------- //

        public BidirectionalDataStream GetStream()
        {
            if (_client2 is null)
            {
                var stream = _client1.GetStream();
                return new(stream, stream);
            }
            else
            {
                var stream1 = _client1.GetStream();
                var stream2 = _client2.GetStream();
                return new(stream2, stream1);
            }
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
