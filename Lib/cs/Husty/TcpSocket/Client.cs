using System;
using System.Net.Sockets;

namespace Husty.TcpSocket
{
    /// <summary>
    /// Tcp socket client class
    /// </summary>
    public class Client : TcpSocketBase
    {

        // ------- Constructor ------- //

        /// <summary>
        /// Start connection
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Client(string ip, int port)
        {
            try
            {
                _client = new TcpClient(ip, port);
                _stream = _client.GetStream();
                Console.WriteLine("Connected.");
            }
            catch
            {
                throw new Exception("Connection failed!");
            }
        }

    }
}
