using System.Text;
using System.Text.Json;
using Husty.IO;

namespace Husty.RosBridge;

public class RosServiceClient<TReq, TRes> : IDisposable, IAsyncDisposable
{

    private record SubType(string op, string service, TRes values);

    // ------ fields ------- //

    private readonly WebSocketStream _stream;
    private readonly CancellationTokenSource _cts;


    // ------ properties ------ //

    public string Service { get; }

    public string Type { get; }


    // ------ constructors ------ //

    private RosServiceClient(WebSocketStream stream, string service, string type)
    {
        _stream = stream;
        Service = service;
        Type = type;
        _cts = new();
    }


    // ------ public methods ------ //

    public static RosServiceClient<TReq, TRes> Create(WebSocketStream stream, string service, CancellationToken? ct = null)
    {
        return CreateAsync(stream, service, ct).GetAwaiter().GetResult();
    }

    public static async Task<RosServiceClient<TReq, TRes>> CreateAsync(WebSocketStream stream, string service, CancellationToken? ct = null)
    {
        var type = typeof(TReq).FullName.Split('.').LastOrDefault().Replace('+', '/').Replace("/Request", "");
        var type2 = typeof(TRes).FullName.Split('.').LastOrDefault().Replace('+', '/').Replace("/Response", "");
        if (type != type2)
            throw new ArgumentException();
        return new(stream, service, type);
    }

    public async Task<TRes> CallAsync(TReq req, CancellationToken? ct = null)
    {
        await _stream.WriteAsync(JsonSerializer.Serialize(new { op = "call_service", service = Service, args = req }), Encoding.ASCII, ct ?? CancellationToken.None).ConfigureAwait(false);
        while (!_cts.IsCancellationRequested)
        {
            var rcv = await _stream.ReadAsync(Encoding.ASCII, ct ?? CancellationToken.None).ConfigureAwait(false);
            if (rcv is not null && rcv.Contains("service_response") && rcv.Contains(Service))
            {
                var x = JsonSerializer.Deserialize<SubType>(rcv);
                if (x is not null)
                {
                    return x.values;
                }
            }
        }
        return default;
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Dispose();   
    }

}
