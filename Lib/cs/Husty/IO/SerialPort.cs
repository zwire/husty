using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Husty.IO
{
    public class SerialPort : ICommunicator
    {

        // ------- Fields ------- //

        private readonly System.IO.Ports.SerialPort _port;


        // ------- Constructor ------- //

        public SerialPort(string portName, int baudRate)
        {
            try
            {
                _port = new();
                _port.PortName = portName;
                _port.BaudRate = baudRate;
                _port.StopBits = StopBits.One;
                _port.Handshake = Handshake.None;
                _port.Parity = Parity.None;
                _port.Open();
            }
            catch
            {
                throw new Exception("failed to open!");
            }
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
                    while (!_port.IsOpen) ;
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
                var stream = _port.BaseStream;
                return new BidirectionalDataStream(stream, stream);
            });
        }
        
        public void Dispose()
        {
            _port?.Close();
            _port?.Dispose();
        }

        public static string[] GetPortNames()
        {
            try
            {
                return System.IO.Ports.SerialPort.GetPortNames();
            }
            catch
            {
                return new string[] { "null" };
            }
        }

    }
}
