using System.Text;
using System.Text.Json;
using Husty.IO;

namespace Husty.RosBridge;

public class RosPublisher<TMsg> : IDisposable, IAsyncDisposable where TMsg : class
{

    // ------ fields ------- //

    private readonly WebSocketStream _stream;


    // ------ properties ------ //

    public string Topic { get; }

    public string Type { get; } 


    // ------ constructors ------ //

    private RosPublisher(WebSocketStream stream, string topic, string type)
    {
        _stream = stream;
        Topic = topic;
        Type = type;
    }


    // ------ public methods ------ //

    public static RosPublisher<TMsg> Create(WebSocketStream stream, string topic, CancellationToken? ct = null)
    {
        return CreateAsync(stream, topic, ct).GetAwaiter().GetResult();
    }

    public static async Task<RosPublisher<TMsg>> CreateAsync(WebSocketStream stream, string topic, CancellationToken? ct = null)
    {
        var type = typeof(TMsg).FullName.Split('.').LastOrDefault().Replace('+', '/');
        await stream.WriteAsync(JsonSerializer.Serialize(new { op = "advertise", topic, type }), Encoding.ASCII, ct).ConfigureAwait(false);
        return new(stream, topic, type);
    }

    public async Task WriteAsync(TMsg msg, CancellationToken? ct = null)
    {
        await _stream.WriteAsync(JsonSerializer.Serialize(new { op = "publish", topic = Topic, type = Type, msg }), Encoding.ASCII, ct).ConfigureAwait(false);
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await _stream.WriteAsync(JsonSerializer.Serialize(new { op = "unadvertise", topic = Topic, type = Type }), Encoding.ASCII, null).ConfigureAwait(false);
    }

}