using System.Text;
using System.Text.Json;
using Husty.Communication;

namespace Husty.RosBridge;

public class RosServiceClient<TReq, TRes> : IAsyncDisposable
{

  private record SubType(string op, string service, TRes values);

  // ------ fields ------- //

  private readonly WebSocketDataTransporter _stream;
  private readonly CancellationTokenSource _cts;


  // ------ properties ------ //

  public string Service { get; }

  public string Type { get; }


  // ------ constructors ------ //

  private RosServiceClient(WebSocketDataTransporter stream, string service, string type)
  {
    _stream = stream;
    Service = service;
    Type = type;
    _cts = new();
  }


  // ------ public methods ------ //

  public static Task<RosServiceClient<TReq, TRes>> CreateAsync(WebSocketDataTransporter stream, string service, CancellationToken? ct = null)
  {
    var type = typeof(TReq).FullName.Split('.').LastOrDefault().Replace('+', '/').Replace("/Request", "");
    var type2 = typeof(TRes).FullName.Split('.').LastOrDefault().Replace('+', '/').Replace("/Response", "");
    if (type != type2)
      throw new ArgumentException();
    return Task.FromResult<RosServiceClient<TReq, TRes>>(new(stream, service, type));
  }

  public async Task<TRes> CallAsync(TReq req, CancellationToken ct = default)
  {
    await _stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "call_service", service = Service, args = req })), default, ct).ConfigureAwait(false);
    while (!_cts.IsCancellationRequested)
    {
      var r = await _stream.TryReadAsync(4096, default, ct).ConfigureAwait(false);
      if (r.IsOk)
      {
        var data = Encoding.ASCII.GetString(r.Unwrap());
        if (data.Contains("service_response") && data.Contains(Service))
        {
          var x = JsonSerializer.Deserialize<SubType>(data);
          if (x is not null)
          {
            return x.values;
          }
        }
      }
    }
    return default;
  }

  public async ValueTask DisposeAsync()
  {
    _cts.Dispose();
  }

}
