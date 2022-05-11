using System;
using System.IO;
using System.Linq;
using Husty.Lawicel;

namespace CanAnalyzer
{
    public class CanLogger : IDisposable
    {

        private readonly StreamWriter _writer;
        private bool _disposed;

        public CanLogger()
        {
            if (!Directory.Exists("log"))
                Directory.CreateDirectory("log");
            _writer = new($"log\\{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv");
        }

        public void Write(CanMessage msg)
        {
            if (!_disposed)
            {
                var id = Convert.ToString(msg.Id, 16);
                var data = BitConverter.GetBytes(msg.Data).Select(x => Convert.ToString(x, 16).ToUpper()).ToArray();
                _writer?.Write("0x" + id + ",");
                for (int i = data.Length - 1; i > 0; i--)
                    _writer?.Write(data[i] + ",");
                _writer?.WriteLine(data[0]);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _writer.Dispose();
        }

    }
}
