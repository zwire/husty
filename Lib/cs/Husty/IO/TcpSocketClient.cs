using System;
using System.Net.Sockets;

namespace Husty.IO
{
    /// <summary>
    /// Tcp socket client class
    /// </summary>
    public class TcpSocketClient : ICommunicator
    {

        // ------- Fields ------- //

        private readonly TcpClient _client1;
        private readonly TcpClient _client2;


        // ------- Constructors ------- //

        public TcpSocketClient(string ip, int inoutPort)
        {
            try
            {
                _client1 = new(ip, inoutPort);
            }
            catch
            {
                throw new Exception("failed to connect!");
            }
        }

        public TcpSocketClient(string ip, int inPort, int outPort)
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
                return new(stream1, stream2);
            }
        }

        public void Dispose()
        {
            _client1?.Dispose();
            _client2?.Dispose();
        }

    }
}
