using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using Husty.Communication;

namespace Husty.RosBridge;

public class RosSubscriber<TMsg> : IAsyncDisposable
{

  private record SubType(string op, string topic, TMsg msg);

  // ------ fields ------- //

  private readonly WebSocketDataTransporter _stream;
  private readonly Subject<TMsg> _subject;
  private readonly CancellationTokenSource _cts;
  private readonly Task _loopTask;


  // ------ properties ------ //

  public string Topic { get; }

  public string Type { get; }

  public IObservable<TMsg> MessageReceived => _subject;


  // ------ constructors ------ //

  private RosSubscriber(WebSocketDataTransporter stream, string topic, string type)
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
          var result = _stream.TryReadAsync(4096, default, _cts.Token).Result;
          if (result.IsOk && Encoding.ASCII.GetString(result.Unwrap()).Contains(topic))
          {
            var x = JsonSerializer.Deserialize<SubType>(result.Unwrap());
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

  public static async Task<RosSubscriber<TMsg>> CreateAsync(WebSocketDataTransporter stream, string topic, CancellationToken ct = default)
  {
    var type = typeof(TMsg).FullName.Split('.').LastOrDefault().Replace('+', '/');
    await stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "subscribe", topic, type })), default, ct).ConfigureAwait(false);
    return new(stream, topic, type);
  }

  public async ValueTask DisposeAsync()
  {
    _cts.Cancel();
    try
    {
      await _loopTask.WaitAsync(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
    }
    catch { }
    await _stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "unsubscribe", topic = Topic })), default, default).ConfigureAwait(false);
  }

}