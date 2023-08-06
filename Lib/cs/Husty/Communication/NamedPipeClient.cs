using System.IO.Pipes;
using System.Text;

namespace Husty.Communication;

public sealed class NamedPipeClient : ICommunicationProtocol
{

  // ------ fields ------ //

  private readonly NamedPipeClientStream _writingStream;
  private readonly NamedPipeClientStream _readingStream;
  private readonly Task _connectionTask;


  // ------ properties ------ //

  public string NewLine { init; get; } = "\n";

  public Encoding Encoding { init; get; } = Encoding.UTF8;


  // ------ constructors ------ //

  public NamedPipeClient(string pipeName, string serverName = ".")
  {
    _writingStream = new(serverName, pipeName + "ClientToServer", PipeDirection.Out);
    _readingStream = new(serverName, pipeName + "ServerToClient", PipeDirection.In);
    _connectionTask = Task.Run(() =>
    {
      try
      {
        _writingStream.Connect();
        _readingStream.Connect();
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
    return new(true, new DataTransporter(_writingStream, _readingStream, Encoding, NewLine));
  }

  public void Dispose()
  {
    _writingStream?.Dispose();
    _readingStream?.Dispose();
  }

}
