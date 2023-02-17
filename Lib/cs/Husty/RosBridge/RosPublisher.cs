using System.Text;
using System.Text.Json;
using Husty.Communication;

namespace Husty.RosBridge;

public class RosPublisher<TMsg> : IDisposable, IAsyncDisposable
{

    // ------ fields ------- //

    private readonly WebSocketDataTransporter _stream;


    // ------ properties ------ //

    public string Topic { get; }

    public string Type { get; } 


    // ------ constructors ------ //

    private RosPublisher(WebSocketDataTransporter stream, string topic, string type)
    {
        _stream = stream;
        Topic = topic;
        Type = type;
    }


    // ------ public methods ------ //

    public static RosPublisher<TMsg> Create(WebSocketDataTransporter stream, string topic, CancellationToken ct = default)
    {
        return CreateAsync(stream, topic, ct).GetAwaiter().GetResult();
    }

    public static async Task<RosPublisher<TMsg>> CreateAsync(WebSocketDataTransporter stream, string topic, CancellationToken ct = default)
    {
        var type = typeof(TMsg).FullName.Split('.').LastOrDefault().Replace('+', '/');
        await stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "advertise", topic, type })), default, ct).ConfigureAwait(false);
        return new(stream, topic, type);
    }

    public async Task WriteAsync(TMsg msg, CancellationToken ct = default)
    {
        await _stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "publish", topic = Topic, type = Type, msg })), default, ct).ConfigureAwait(false);
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await _stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "unadvertise", topic = Topic, type = Type })), default, default).ConfigureAwait(false);
    }

}