using System;
using System.IO.Pipes;

namespace Husty.IO
{
    public class NamedPipeClient : ICommunicator
    {

        // ------- Fields ------- //

        private readonly NamedPipeClientStream _writer;
        private readonly NamedPipeClientStream _reader;


        // ------- Constructors ------- //

        public NamedPipeClient(string pipeName, string serverName = ".")
        {
            try
            {
                _writer = new(serverName, pipeName + "ClientToServer", PipeDirection.Out);
                _reader = new(serverName, pipeName + "ServerToClient", PipeDirection.In);
                _writer.Connect();
                _reader.Connect();
            }
            catch
            {
                throw new Exception("failed to connect!");
            }
        }


        // ------- Methods ------- //

        public BidirectionalDataStream GetStream()
        {
            return new(_writer, _reader);
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _reader?.Dispose();
        }

    }
}
