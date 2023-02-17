using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Husty.Communication;

public sealed class TcpSocketServer : ICommunicationProtocol
{

    // ------ fields ------ //

    private TcpClient _client1;
    private TcpClient _client2;
    private readonly TcpListener _listener1;
    private readonly TcpListener _listener2;
    private readonly Task _connectionTask;


    // ------ properties ------ //

    public string NewLine { init; get; } = "\n";

    public Encoding Encoding { init; get; } = Encoding.UTF8;


    // ------ constructors ------ //

    public TcpSocketServer(int port)
    {
        _listener1 = new(IPAddress.Any, port);
        _listener1.Start();
        _connectionTask = Task.Run(() =>
        {
            try
            {
                _client1 = _listener1.AcceptTcpClient();
            }
            catch
            {
                throw new Exception("failed to connect!");
            }
        });
    }

    public TcpSocketServer(int listeningPort, int targetPort)
    {
        _listener1 = new(IPAddress.Any, listeningPort);
        _listener2 = new(IPAddress.Any, targetPort);
        _listener1.Start();
        _listener2.Start();
        _connectionTask = Task.Run(() =>
        {
            try
            {
                _client1 = _listener1.AcceptTcpClient();
                _client2 = _listener2.AcceptTcpClient();
            }
            catch
            {
                throw new Exception("failed to connect!");
            }
        });
    }


    // ------ public methods ------ //

    public ResultExpression<IDataTransporter> GetStream()
    {
        return GetStreamAsync().Result;
    }

    public async Task<ResultExpression<IDataTransporter>> GetStreamAsync(
        TimeSpan timeout = default,
        CancellationToken ct = default
    )
    {
        if (ct.IsCancellationRequested) return new(false, default!);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (timeout != default) cts.CancelAfter(timeout);
        await _connectionTask.WaitAsync(cts.Token).ConfigureAwait(false);
        if (_client1 is not null && _client2 is null)
        {
            var stream = _client1.GetStream();
            return new(true, new TcpDataTransporter(stream, stream, Encoding, NewLine));
        }
        else if (_client1 is not null && _client2 is not null)
        {
            var stream1 = _client1.GetStream();
            var stream2 = _client2.GetStream();
            return new(true, new TcpDataTransporter(stream2, stream1, Encoding, NewLine));
        }
        else
        {
            return new(false, default!);
        }
    }

    public void Dispose()
    {
        _client1?.Dispose();
        _client2?.Dispose();
        _listener1?.Stop();
        _listener2?.Stop();
    }

}
