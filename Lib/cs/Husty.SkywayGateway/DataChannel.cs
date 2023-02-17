using System.Diagnostics;
using System.Net;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using Husty.Communication;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Husty.SkywayGateway;

public sealed class DataChannel : IAsyncDisposable
{

    // ------ fields ------ //

    private readonly RestClient _client;
    private readonly string _token;
    private readonly DataConnectionInfo _info;
    private readonly CancellationTokenSource _cts;
    private readonly Task<string> _called;
    private readonly AsyncSubject<bool> _opened = new();
    private readonly AsyncSubject<bool> _closed = new();
    private string _dataId;
    private string _dataConnectionId;


    // ------ properties ------ //

    public string LocalPeerId { get; }

    public string RemotePeerId { private set; get; }

    public DataConnectionInfo ConnectionInfo => _info;

    public IObservable<bool> Opened => _opened;

    public IObservable<bool> Closed => _closed;


    // ------ constructors ------ //

    private DataChannel(
        RestClient client,
        string peerId,
        string token,
        string dataId,
        DataConnectionInfo info,
        Task<string> called,
        CancellationTokenSource cts
    )
    {
        LocalPeerId = peerId;
        _client = client;
        _token = token;
        _dataId = dataId;
        _info = info;
        _cts = cts;
        _called = called;
        Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
                await ListenEventAsync().ConfigureAwait(false);
        });
    }

    internal static async Task<DataChannel> CreateNewAsync(
        RestClient client,
        string peerId,
        string token,
        IPEndPoint localEP,
        Task<string> called,
        CancellationTokenSource cts
    )
    {
        var response = await client.RequestAsync(ReqType.Post, "/data", new() { }).ConfigureAwait(false);
        var p = JObject.Parse(response.Content);
        var dataId = p["data_id"].Value<string>();
        var remoteEP = new IPEndPoint(IPAddress.Parse(p["ip_v4"].Value<string>()), p["port"].Value<int>());
        return new(client, peerId, token, dataId, new(localEP, remoteEP), called, cts);
    }


    // ------ public methods ------ //

    public async ValueTask DisposeAsync()
    {
        if (_dataConnectionId is not null)
            await _client.RequestAsync(ReqType.Delete, $"/data/connections/{_dataConnectionId}").ConfigureAwait(false);
        _dataConnectionId = null;
        await _client.RequestAsync(ReqType.Delete, $"/data/{_dataId}").ConfigureAwait(false);
        _dataId = null;
    }

    public async Task<bool> ConfirmAliveAsync()
    {
        if (_dataConnectionId is null)
            return false;
        var response = await _client.RequestAsync(ReqType.Get, $"/data/connections/{_dataConnectionId}/status").ConfigureAwait(false);
        return JObject.Parse(response.Content)["open"].Value<bool>();
    }

    public async Task<UdpDataTransporter> ListenAsync()
    {
        _dataConnectionId = await _called.ConfigureAwait(false);
        await _opened.ToTask().ConfigureAwait(false);
        var json = new Dictionary<string, dynamic>
        {
            {
                "feed_params", new Dictionary<string, dynamic>
                {
                    { "data_id", _dataId }
                }
            },
            {
                "redirect_params", new Dictionary<string, dynamic>
                {
                    { "ip_v4", _info.LocalEP.Address.ToString() },
                    { "port", _info.LocalEP.Port }
                }
            }
        };
        await _client.RequestAsync(ReqType.Put, $"/data/connections/{_dataConnectionId}", json).ConfigureAwait(false);
        var response = await _client.RequestAsync(ReqType.Get, $"/data/connections/{_dataConnectionId}/status").ConfigureAwait(false);
        RemotePeerId = JObject.Parse(response.Content)["remote_id"].Value<string>();
        return new UdpDataTransporter() { Encoding = Encoding.UTF8, NewLine = "" }
            .SetListeningPort(_info.LocalEP.Port)
            .SetTargetPorts(_info.RemoteEP.Port);
    }

    public async Task<UdpDataTransporter> CallConnectionAsync(string remoteId)
    {
        RemotePeerId = remoteId;
        var json = new Dictionary<string, dynamic>
        {
            { "peer_id", LocalPeerId },
            { "token", _token },
            { "options", new Dictionary<string, string>
                {
                    { "serialization", "NONE" }
                }
            },
            { "target_id", RemotePeerId },
            {
                "params", new Dictionary<string, dynamic>
                {
                    { "data_id", _dataId }
                }
            },
            {
                "redirect_params", new Dictionary<string, dynamic>
                {
                    { "ip_v4", _info.LocalEP.Address.ToString() },
                    { "port", _info.LocalEP.Port }
                }
            }
        };
        var response = await _client.RequestAsync(ReqType.Post, "/data/connections", json).ConfigureAwait(false);
        _dataConnectionId = JObject.Parse(response.Content)["params"]["data_connection_id"].Value<string>();
        await _opened.ToTask().ConfigureAwait(false);
        return new UdpDataTransporter() { Encoding = Encoding.UTF8, NewLine = "" }
            .SetListeningPort(_info.LocalEP.Port)
            .SetTargetPorts(_info.RemoteEP.Port);
    }


    // ------ private methods ------ //

    private async Task ListenEventAsync()
    {
        if (_dataConnectionId is null)
        {
            await Task.Delay(100);
            return;
        }
        var response = await _client.RequestAsync(ReqType.Get, $"/data/connections/{_dataConnectionId}/events", null).ConfigureAwait(false);
        if (response is null)
        {
            await Task.Delay(1000);
            return;
        }
        var p = JObject.Parse(response.Content);
        var e = p["event"].ToString();
        if (e is "ERROR")
            Debug.WriteLine(p["error_message"]);
        switch (e)
        {
            case "OPEN":
                _opened.OnNext(true);
                _opened.OnCompleted();
                break;
            case "CLOSE":
                _closed.OnNext(true);
                _closed.OnCompleted();
                break;
            default:
                break;
        }
    }

}
