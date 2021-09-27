using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Husty.IO
{
    public class NamedPipeServer : ICommunicator
    {

        // ------- Fields ------- //

        private readonly NamedPipeServerStream _writer;
        private readonly NamedPipeServerStream _reader;
        private readonly Task _connectionTask;


        // ------- Constructors ------- //

        public NamedPipeServer(string pipeName)
        {
            _reader = new(pipeName + "ClientToServer", PipeDirection.In);
            _writer = new(pipeName + "ServerToClient", PipeDirection.Out);
            _connectionTask = Task.Run(() =>
            {
                try
                {
                    _reader.WaitForConnection();
                    _writer.WaitForConnection();
                }
                catch
                {
                    throw new Exception("failed to connect!");
                }
            });
        }


        // ------- Methods ------- //

        public BidirectionalDataStream GetStream()
        {
            _connectionTask.Wait();
            return new BidirectionalDataStream(_writer, _reader);
        }

        public async Task<BidirectionalDataStream> GetStreamAsync()
        {
            return await Task.Run(() => GetStream());
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _reader?.Dispose();
        }

    }
}
