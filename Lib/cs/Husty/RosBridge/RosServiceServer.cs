using System.Text;
using System.Text.Json;
using Husty.Communication;

namespace Husty.RosBridge;

public class RosServiceServer<TReq, TRes> : IDisposable, IAsyncDisposable
{

    private record SubType(string op, string id, string service, TReq args);

    // ------ fields ------- //

    private readonly WebSocketDataTransporter _stream;
    private readonly CancellationTokenSource _cts;
    private readonly Task _loopTask;
    private Func<TReq, TRes> _func;


    // ------ properties ------ //

    public string Service { get; }

    public string Type { get; }


    // ------ constructors ------ //

    private RosServiceServer(WebSocketDataTransporter stream, string service, string type, Func<TReq, TRes> func)
    {
        _stream = stream;
        _func = func;
        Service = service;
        Type = type;
        _cts = new();
        _loopTask = Task.Run(() =>
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var (success, rcv) = _stream.TryReadAsync(4096, default, _cts.Token).Result;
                    if (success)
                    {
                        var data = Encoding.ASCII.GetString(rcv);
                        if (data.Contains(service))
                        {
                            var x = JsonSerializer.Deserialize<SubType>(data);
                            if (x is not null && _func is not null)
                            {
                                var response = _func(x.args);
                                _stream.TryWriteAsync(
                                    Encoding.ASCII.GetBytes(
                                        JsonSerializer.Serialize(new
                                        {
                                            op = "service_response",
                                            x.id,
                                            service = Service,
                                            values = response,
                                            result = true
                                        })
                                    ),
                                    default,
                                    _cts.Token
                                ).Wait();
                            }
                        }
                    }
                }
                catch { return; }
            }
        });
    }


    // ------ public methods ------ //

    public static RosServiceServer<TReq, TRes> Create(
        WebSocketDataTransporter stream, 
        string topic,
        Func<TReq, TRes> func,
        CancellationToken ct = default
    )
    {
        return CreateAsync(stream, topic, func, ct).Result;
    }

    public static async Task<RosServiceServer<TReq, TRes>> CreateAsync(
        WebSocketDataTransporter stream, 
        string service,
        Func<TReq, TRes> func,
        CancellationToken ct = default
    )
    {
        var type = typeof(TReq).FullName.Split('.').LastOrDefault().Replace('+', '/').Replace("/Request", "");
        var type2 = typeof(TRes).FullName.Split('.').LastOrDefault().Replace('+', '/').Replace("/Response", "");
        if (type != type2)
            throw new ArgumentException();
        await stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "advertise_service", service, type })), default, ct).ConfigureAwait(false);
        return new(stream, service, type, func);
    }

    public void SetFunction(Func<TReq, TRes> func)
    {
        _func = func;
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
        await _stream.TryWriteAsync(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { op = "unadvertise_service", service = Service })), default, default).ConfigureAwait(false);
    }

}
