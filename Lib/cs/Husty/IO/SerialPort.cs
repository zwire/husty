using System;
using System.IO;
using System.IO.Ports;

namespace Husty.IO
{
    public class SerialPort
    {

        // ------- Fields ------- //

        private readonly System.IO.Ports.SerialPort _port;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private object _locker = new();


        // ------- Constructor ------- //

        public SerialPort(string portName, int baudRate, int readTimeout = -1, int writeTimeout = -1)
        {
            _port = new();
            _port.PortName = portName;
            _port.BaudRate = baudRate;
            _port.StopBits = StopBits.One;
            _port.Handshake = Handshake.None;
            _port.Parity = Parity.None;
            _port.ReadTimeout = readTimeout;
            _port.WriteTimeout = writeTimeout;
            try
            {
                lock (_locker)
                {
                    _port.Open();
                    _reader = new(_port.BaseStream);
                    _writer = new(_port.BaseStream);
                }
            }
            catch
            {
                throw new Exception("failed to open!");
            }
        }


        // ------- Methods ------- //

        public string ReadLine()
        {
            lock (_locker)
            {
                if (_port is not null && _port.IsOpen)
                    return _reader?.ReadLine();
                else
                    return null;
            }
        }

        public void WriteLine()
        {
            lock (_locker)
            {
                if (_port is not null && _port.IsOpen)
                    _writer?.WriteLine();
            }
        }

        public void Dispose()
        {
            lock (_locker)
            {
                _writer?.Dispose();
                _reader?.Dispose();
                _port?.Close();
                _port?.Dispose();
            }
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
