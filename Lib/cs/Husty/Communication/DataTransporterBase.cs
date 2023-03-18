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
    protected abstract Task<ResultExpression<byte[]>> DoTryReadAsync(int count, CancellationToken ct);
    protected abstract Task<bool> DoTryWriteLineAsync(string data, CancellationToken ct);
    protected abstract Task<ResultExpression<string>> DoTryReadLineAsync(CancellationToken ct);


    // ------ public methods ------ //

    public void Dispose()
    {
        _cts.Cancel();
        DoDispose();
    }

    public bool TryWrite(byte[] data)
    {
        return TryWriteAsync(data).Result;
    }

    public byte[]? Read(int count = 4096)
    {
        var (s, v) = TryReadAsync(count).Result;
        if (s) return v;
        return null;
    }

    public bool TryWriteLine(string data)
    {
        return TryWriteLineAsync(data).Result;
    }

    public string? ReadLine()
    {
        var (s, v) = TryReadLineAsync().Result;
        if (s) return v;
        return null;
    }

    public bool TryWriteAsJson<T>(T data)
    {
        return TryWriteAsJsonAsync(data).Result;
    }

    public T? ReadAsJson<T>()
    {
        var (s, v) = TryReadAsJsonAsync<T>().Result;
        if (s) return v;
        return default;
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

    public async Task<ResultExpression<byte[]>> TryReadAsync(
        int count,
        TimeSpan timeout = default,
        CancellationToken ct = default
    )
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
        if (cts.Token.IsCancellationRequested) return new(false, default!);
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

    public async Task<ResultExpression<string>> TryReadLineAsync(
        TimeSpan timeout = default,
        CancellationToken ct = default
    )
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
        if (cts.Token.IsCancellationRequested) return new(false, default!);
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

    public async Task<ResultExpression<T>> TryReadAsJsonAsync<T>(
        TimeSpan timeout = default,
        CancellationToken ct = default
    )
    {
        var result = await TryReadLineAsync(timeout, ct).ConfigureAwait(false);
        if (!result.HasValue) return new(false, default!);
        try
        {
            var obj = JsonSerializer.Deserialize<T>(result.Value!);
            if (obj is null) return new(false, default!);
            return new(true, obj);
        }
        catch
        {
            return new(false, default!);
        }
    }

}
