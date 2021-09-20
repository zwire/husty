using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Husty.IO
{
    public class SerialPort : ICommunicator
    {

        // ------- Fields ------- //

        private readonly System.IO.Ports.SerialPort _port;
        private readonly Task _connectionTask;


        // ------- Constructor ------- //

        public SerialPort(string portName, int baudRate)
        {
            _port = new();
            _port.PortName = portName;
            _port.BaudRate = baudRate;
            _port.StopBits = StopBits.One;
            _port.Handshake = Handshake.None;
            _port.Parity = Parity.None;
            _connectionTask = Task.Run(() =>
            {
                try
                {
                    _port.Open();
                }
                catch
                {
                    throw new Exception("failed to open!");
                }
            });
        }


        // ------- Methods ------- //

        public BidirectionalDataStream GetStream()
        {
            return GetStreamAsync().Result;
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.Run(() =>
            {
                _connectionTask.Wait();
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
