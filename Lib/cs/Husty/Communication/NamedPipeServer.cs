using System.IO.Pipes;
using System.Text;

namespace Husty.Communication;

public sealed class NamedPipeServer : ICommunicationProtocol
{

    // ------ fields ------ //

    private readonly NamedPipeServerStream _writer;
    private readonly NamedPipeServerStream _reader;
    private readonly Task _connectionTask;


    // ------ properties ------ //

    public string NewLine { init; get; } = "\n";

    public Encoding Encoding { init; get; } = Encoding.UTF8;


    // ------ constructors ------ //

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
