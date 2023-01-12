using System.Text;
using System.Text.Json;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Husty.IO;

namespace Husty.RosBridge;

public class RosSubscriber<TMsg> : IDisposable, IAsyncDisposable where TMsg : class
{

    private record SubType(string op, string topic, TMsg msg);

    // ------ fields ------- //

    private readonly WebSocketStream _stream;
    private readonly Subject<TMsg> _subject;
    private readonly CancellationTokenSource _cts;
    private readonly Task _loopTask;


    // ------ properties ------ //

    public string Topic { get; }

    public string Type { get; }

    public IObservable<TMsg> MessageReceived => _subject;


    // ------ constructors ------ //

    private RosSubscriber(WebSocketStream stream, string topic, string type)
    {
        _stream = stream;
        Topic = topic;
        Type = type;
        _subject = new();
        _cts = new();
        _loopTask = Task.Run(() =>
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var rcv = _stream.ReadAsync(Encoding.ASCII, CancellationToken.None).Result;
                    if (rcv is not null && rcv.Contains(topic))
                    {
                        var x = JsonSerializer.Deserialize<SubType>(rcv);
                        if (x is not null)
                        {
                            _subject.OnNext(x.msg);
                        }
                    }
                }
                catch { return; }
            }
        });
    }


    // ------ public methods ------ //

    public static RosSubscriber<TMsg> Create(WebSocketStream stream, string topic, CancellationToken? ct = null)
    {
        return CreateAsync(stream, topic, ct).GetAwaiter().GetResult();
    }

    public static async Task<RosSubscriber<TMsg>> CreateAsync(WebSocketStream stream, string topic, CancellationToken? ct = null)
    {
        var type = typeof(TMsg).FullName.Split('.').LastOrDefault().Replace('+', '/');
        await stream.WriteAsync(JsonSerializer.Serialize(new { op = "subscribe", topic, type }), Encoding.ASCII, ct).ConfigureAwait(false);
        return new(stream, topic, type);
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        try
        {
            await _loopTask.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        }
        catch { }
        await _stream.WriteAsync(JsonSerializer.Serialize(new { op = "unsubscribe", topic = Topic, type = Type }), Encoding.ASCII, null).ConfigureAwait(false);
    }

}