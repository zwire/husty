using System.Text;

namespace Husty.Communication;

public interface IDataTransporter : IDisposable
{

    public string NewLine { get; }
    public Encoding Encoding { get; }

    public bool TryWrite(byte[] data);

    public byte[]? Read(int count = 4096);

    public bool TryWriteLine(string data);

    public string? ReadLine();

    public bool TryWriteAsJson<T>(T data);

    public T? ReadAsJson<T>();

    public Task<bool> TryWriteAsync(
        byte[] data,
        TimeSpan timeout = default,
        CancellationToken ct = default
    );

    public Task<ResultExpression<byte[]>> TryReadAsync(
        int count,
        TimeSpan timeout = default,
        CancellationToken ct = default
    );

    public Task<bool> TryWriteLineAsync(
        string data,
        TimeSpan timeout = default,
        CancellationToken ct = default
    );

    public Task<ResultExpression<string>> TryReadLineAsync(
        TimeSpan timeout = default,
        CancellationToken ct = default
    );

    public Task<bool> TryWriteAsJsonAsync<T>(
        T data,
        TimeSpan timeout = default,
        CancellationToken ct = default
    );

    public Task<ResultExpression<T>> TryReadAsJsonAsync<T>(
        TimeSpan timeout = default,
        CancellationToken ct = default 
    );

}
