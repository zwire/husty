using System.Text;
using System.Text.Json;

namespace Husty.Communication;

public abstract class DataTransporterBase : IDataTransporter
{

  // ------ fields ------ //

  private readonly CancellationTokenSource _cts;


  // ------ properties ------ //

  public string NewLine { init; get; } = "\n";
  public Encoding Encoding { init; get; } = Encoding.UTF8;
  public abstract Stream BaseWritingStream { get; }
  public abstract Stream BaseReadingStream { get; }


  // ------ constructors ------ //

  public DataTransporterBase()
  {
    _cts = new();
  }


  // ------ protected methods ------ //

  protected abstract void DoDispose();
  protected abstract Task<bool> DoTryWriteAsync(byte[] data, CancellationToken ct);
  protected abstract Task<Result<byte[]>> DoTryReadAsync(int count, CancellationToken ct);
  protected abstract Task<bool> DoTryWriteLineAsync(string data, CancellationToken ct);
  protected abstract Task<Result<string>> DoTryReadLineAsync(CancellationToken ct);


  // ------ public methods ------ //

  public void Dispose()
  {
    _cts.Cancel();
    DoDispose();
  }

  public async Task<bool> TryWriteAsync(
    byte[] data,
    TimeSpan timeout = default,
    CancellationToken ct = default
  )
  {
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
    if (cts.Token.IsCancellationRequested) return false;
    if (timeout != default) cts.CancelAfter(timeout);
    return await DoTryWriteAsync(data, cts.Token).ConfigureAwait(false);
  }

  public async Task<Result<byte[]>> TryReadAsync(
    int count,
    TimeSpan timeout = default,
    CancellationToken ct = default
  )
  {
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
    if (cts.Token.IsCancellationRequested) 
      return Result<byte[]>.Err(new("cancelled"));
    if (timeout != default) cts.CancelAfter(timeout);
    return await DoTryReadAsync(count, cts.Token).ConfigureAwait(false);
  }

  public async Task<bool> TryWriteLineAsync(
    string data,
    TimeSpan timeout = default,
    CancellationToken ct = default
  )
  {
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
    if (cts.Token.IsCancellationRequested) return false;
    if (timeout != default) cts.CancelAfter(timeout);
    return await DoTryWriteLineAsync(data, cts.Token).ConfigureAwait(false);
  }

  public async Task<Result<string>> TryReadLineAsync(
    TimeSpan timeout = default,
    CancellationToken ct = default
  )
  {
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
    if (cts.Token.IsCancellationRequested)
      return Result<string>.Err(new("cancelled"));
    if (timeout != default) cts.CancelAfter(timeout);
    return await DoTryReadLineAsync(cts.Token).ConfigureAwait(false);
  }

  public async Task<bool> TryWriteAsJsonAsync<T>(
    T data,
    TimeSpan timeout = default,
    CancellationToken ct = default
  )
  {
    return await TryWriteLineAsync(JsonSerializer.Serialize(data), timeout, ct).ConfigureAwait(false);
  }

  public async Task<Result<T>> TryReadAsJsonAsync<T>(
    TimeSpan timeout = default,
    CancellationToken ct = default
  )
  {
    var result = await TryReadLineAsync(timeout, ct).ConfigureAwait(false);
    if (!result.IsOk)
      return Result<T>.Err(new("failed to read"));
    try
    {
      var obj = JsonSerializer.Deserialize<T>(result.Unwrap());
      if (obj is null) 
        return Result<T>.Err(new("null"));
      return Result<T>.Ok(obj);
    }
    catch
    {
      return Result<T>.Err(new());
    }
  }

}
