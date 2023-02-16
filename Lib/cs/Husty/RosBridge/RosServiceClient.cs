using System.Text;
using System.Text.Json;
using Husty.IO;

namespace Husty.RosBridge;

public class RosServiceClient<TReq, TRes> : IDisposable, IAsyncDisposable
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

    public static RosServiceClient<TReq, TRes> Create(WebSocketDataTransporter stream, string service, CancellationToken? ct = null)
    {
        return CreateAsync(stream, service, ct).GetAwaiter().GetResult();
    }

    public static async Task<RosServiceClient<TReq, TRes>> CreateAsync(WebSocketDataTransporter stream, string service, CancellationToken? ct = null)
    {
        var type = typeof(TReq).FullName.Split('.').LastOrDefault().Replace('+', '/').Replace("/Request", "");
        var type2 = typeof(TRes).FullName.Split('.').LastOrDefault().Replace('+', '/').Replace("/Response", "");
        if (type != type2)
            throw new ArgumentException();
        return new(stream, service, type);
    }

    public async Task<TRes> CallAsync(TReq req, CancellationToken ct = default)
    {
        await _stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "call_service", service = Service, args = req })), default, ct).ConfigureAwait(false);
        while (!_cts.IsCancellationRequested)
        {
            var (success, rcv) = await _stream.TryReadAsync(4096, default, ct).ConfigureAwait(false);
            if (success)
            {
                var data = Encoding.ASCII.GetString(rcv);
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

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Dispose();   
    }

}
