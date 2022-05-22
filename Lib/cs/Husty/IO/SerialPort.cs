using System;
using System.IO.Ports;

namespace Husty.IO
{
    public sealed class SerialPort : IDisposable
    {

        // ------- fields ------- //

        private bool _disposed;
        private readonly System.IO.Ports.SerialPort _port;


        // ------- constructors ------- //

        public SerialPort(string portName, int baudRate, int readTimeout = -1, int writeTimeout = -1, string newLine = "\n")
        {
            _port = new()
            {
                PortName = portName,
                BaudRate = baudRate,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                Parity = Parity.None,
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout,
                NewLine = newLine
            };
            try
            {
                _port.Open();
            }
            catch
            {
                throw new Exception("failed to open!");
            }
        }


        // ------- public methods ------- //

        public bool Write(byte[] value)
        {
            if (_disposed) return false;
            try
            {
                if (_port.IsOpen is true) _port.Write(value, 0, value.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Write(string value)
        {
            if (_disposed) return false;
            try
            {
                if (_port.IsOpen is true) _port.Write(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool WriteLine(string value)
        {
            if (_disposed) return false;
            try
            {
                if (_port.IsOpen is true) _port.WriteLine(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public byte[] Read(int size)
        {
            var buf = new byte[size];
            var progress = 0;
            while (progress < size)
                progress += _port.Read(buf, progress, size - progress);
            return buf;
        }

        public string ReadLine()
        {
            if (_disposed) return null;
            try
            {
                return _port.IsOpen is true ? _port.ReadLine() : null;
            }
            catch
            {
                return null;
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _port.Close();
            _port.Dispose();
        }

        public static string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

    }
}
