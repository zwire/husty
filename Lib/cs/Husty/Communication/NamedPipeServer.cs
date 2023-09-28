using System.IO.Pipes;
using System.Text;

namespace Husty.Communication;

public sealed class NamedPipeServer : ICommunicationProtocol
{

  // ------ fields ------ //

  private readonly NamedPipeServerStream _writingStream;
  private readonly NamedPipeServerStream _readingStream;
  private readonly Task _connectionTask;


  // ------ properties ------ //

  public string NewLine { init; get; } = "\n";

  public Encoding Encoding { init; get; } = Encoding.UTF8;


  // ------ constructors ------ //

  public NamedPipeServer(string pipeName)
  {
    _readingStream = new(pipeName + "ClientToServer", PipeDirection.In);
    _writingStream = new(pipeName + "ServerToClient", PipeDirection.Out);
    _connectionTask = Task.Run(() =>
    {
      try
      {
        _readingStream.WaitForConnection();
        _writingStream.WaitForConnection();
      }
      catch
      {
        throw new Exception("failed to connect!");
      }
    });
  }


  // ------ public methods ------ //

  public async Task<Result<IDataTransporter>> GetStreamAsync(
    TimeSpan timeout = default,
    CancellationToken ct = default
  )
  {
    if (ct.IsCancellationRequested)
      return Result<IDataTransporter>.Err(new("cancelled"));
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    if (timeout != default) cts.CancelAfter(timeout);
    await _connectionTask.WaitAsync(cts.Token).ConfigureAwait(false);
    return Result<IDataTransporter>.Ok(new DataTransporter(_writingStream, _readingStream, Encoding, NewLine));
  }

  public void Dispose()
  {
    _writingStream?.Dispose();
    _readingStream?.Dispose();
  }

}
