using System.IO.Pipes;
using System.Text;

namespace Husty.IO;

public sealed class NamedPipeClient : ICommunicationProtocol
{

    // ------ fields ------ //

    private readonly NamedPipeClientStream _writer;
    private readonly NamedPipeClientStream _reader;
    private readonly Task _connectionTask;


    // ------ properties ------ //

    public string NewLine { init; get; } = "\n";

    public Encoding Encoding { init; get; } = Encoding.UTF8;


    // ------ constructors ------ //

    public NamedPipeClient(string pipeName, string serverName = ".")
    {
        _writer = new(serverName, pipeName + "ClientToServer", PipeDirection.Out);
        _reader = new(serverName, pipeName + "ServerToClient", PipeDirection.In);
        _connectionTask = Task.Run(() =>
        {
            try
            {
                _writer.Connect();
                _reader.Connect();
            }
            catch
            {
                throw new Exception("failed to connect!");
            }
        });
    }


    // ------ public methods ------ //

    public ResultExpression<IDataTransporter> GetStream()
    {
        return GetStreamAsync().Result;
    }

    public async Task<ResultExpression<IDataTransporter>> GetStreamAsync(
        TimeSpan timeout = default,
        CancellationToken ct = default
    )
    {
        if (ct.IsCancellationRequested) return new(false, default!);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (timeout != default) cts.CancelAfter(timeout);
        await _connectionTask.WaitAsync(cts.Token).ConfigureAwait(false);
        return new(true, new TcpDataTransporter(_writer, _reader, Encoding, NewLine));
    }

    public void Dispose()
    {
        _writer?.Dispose();
        _reader?.Dispose();
    }

}
