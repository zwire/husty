using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Husty.SkywayGateway
{
    public sealed class DataChannel : IAsyncDisposable
    {

        // ------ fields ------ //

        private readonly RestClient _client;
        private readonly string _token;
        private readonly string _dataId;
        private readonly DataConnectionInfo _info;
        private readonly CancellationTokenSource _cts;
        private readonly Task<string> _called;
        private string _dataConnectionId;


        // ------ properties ------ //

        public string LocalPeerId { get; }

        public string RemotePeerId { private set; get; }

        public DataConnectionInfo ConnectionInfo => _info;


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
            var response = await client.RequestAsync(ReqType.Post, "/data", new { });
            var p = JObject.Parse(response.Content);
            var dataId = p["data_id"].Value<string>();
            var remoteEP = new IPEndPoint(IPAddress.Parse(p["ip_v4"].Value<string>()), p["port"].Value<int>());
            return new(client, peerId, token, dataId, new(localEP, remoteEP), called, cts);
        }


        // ------ public methods ------ //

        public async ValueTask DisposeAsync()
        {
            if (_dataConnectionId is not null)
                await _client.RequestAsync(ReqType.Delete, $"/data/connections/{_dataConnectionId}");
            await _client.RequestAsync(ReqType.Delete, $"/data/{_dataId}");
        }

        public async Task<bool> ConfirmAliveAsync()
        {
            if (_dataConnectionId is null)
                return false;
            var response = await _client.RequestAsync(ReqType.Get, $"/data/connections/{_dataConnectionId}/status");
            return JObject.Parse(response.Content)["open"].Value<bool>();
        }

        public async Task<DataStream> ListenAsync()
        {
            _dataConnectionId = await _called;
            var e = await ListenEventAsync("OPEN");
            if (e is null)
                throw new InvalidRequestException("failed to confirm connection open.");
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
            await _client.RequestAsync(ReqType.Put, $"/data/connections/{_dataConnectionId}", json);
            var response = await _client.RequestAsync(ReqType.Get, $"/data/connections/{_dataConnectionId}/status");
            RemotePeerId = JObject.Parse(response.Content)["remote_id"].Value<string>();
            return new(_info, LocalPeerId, RemotePeerId);
        }

        public async Task<DataStream> CallConnectionAsync(string remoteId)
        {
            RemotePeerId = remoteId;
            var json = new Dictionary<string, dynamic>
            {
                { "peer_id", LocalPeerId },
                { "token", _token },
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
            var response = await _client.RequestAsync(ReqType.Post, "/data/connections", json);
            _dataConnectionId = JObject.Parse(response.Content)["params"]["data_connection_id"].Value<string>();
            var e = await ListenEventAsync("OPEN");
            if (e is null)
                throw new InvalidRequestException("failed to confirm connection open.");
            return new(_info, LocalPeerId, RemotePeerId);
        }


        // ------ private methods ------ //

        private async Task<string> ListenEventAsync(string type)
        {
            while (!_cts.IsCancellationRequested)
            {
                var response = await _client.RequestAsync(ReqType.Get, $"/data/connections/{_dataConnectionId}/events");
                var p = JObject.Parse(response.Content);
                var e = p["event"].ToString();
                if (e is "ERROR")
                    Debug.WriteLine(p["error_message"]);
                if (type != "*" && e != type)
                    continue;
                return e switch
                {
                    "OPEN" => "",
                    "CLOSE" => "",
                    _ => null
                };
            }
            return null;
        }

    }
}
