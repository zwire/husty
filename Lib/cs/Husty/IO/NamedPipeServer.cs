using System;
using System.IO.Pipes;

namespace Husty.IO
{
    public class NamedPipeServer : ICommunicator
    {

        // ------- Fields ------- //

        private readonly NamedPipeServerStream _writer;
        private readonly NamedPipeServerStream _reader;


        // ------- Constructors ------- //

        public NamedPipeServer(string pipeName)
        {
            try
            {
                _reader = new(pipeName + "ClientToServer", PipeDirection.In);
                _writer = new(pipeName + "ServerToClient", PipeDirection.Out);
                _reader.WaitForConnection();
                _writer.WaitForConnection();
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
