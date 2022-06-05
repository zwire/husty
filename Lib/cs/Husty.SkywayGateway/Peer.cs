using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Husty.SkywayGateway
{
    public sealed class Peer : IAsyncDisposable
    {

        // ------ fields ------ //

        private readonly RestClient _client;
        private readonly string _peerId;
        private readonly string _token;
        private readonly IPAddress _host = IPAddress.Parse("127.0.0.1");
        private readonly CancellationTokenSource _cts = new();
        private readonly AsyncSubject<bool> _opened = new();
        private readonly AsyncSubject<bool> _closed = new();
        private readonly AsyncSubject<string> _dataCalled = new();
        private readonly AsyncSubject<string> _mediaCalled = new();
        private readonly AsyncSubject<int> _expiresRemainingSecondNotified = new();


        // ------ properties ------ //

        public string PeerId => _peerId;

        public IObservable<bool> Opened => _opened;

        public IObservable<bool> Closed => _closed;

        public IObservable<int> ExpiresRemainingSecondNotified => _expiresRemainingSecondNotified;


        // ------ constructors ------ //

        private Peer(RestClient client, string id, string token)
        {
            _client = client;
            _peerId = id;
            _token = token;
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                    await ListenEventAsync().ConfigureAwait(false);
            });
            _opened.ToTask().Wait();
        }

        public static async Task<Peer> CreateNewAsync(
            string apiKey,
            string id,
            bool useTurnServer = false,
            PeerCredential credential = default
        )
        {
            var client = new RestClient(new Uri("http://localhost:8000/"));
            var json = credential is null
                ? new Dictionary<string, dynamic>
                {
                    { "key", apiKey },
                    { "domain", "localhost" },
                    { "peer_id", id },
                    { "turn", useTurnServer }
                }
                : new Dictionary<string, dynamic>
                {
                    { "key", apiKey },
                    { "domain", "localhost" },
                    { "peer_id", id },
                    { "turn", useTurnServer },
                    {
                        "credential", new Dictionary<string, dynamic>
                        {
                            { "timestamp", credential.Timestamp.ToUnixTimeSeconds() },
                            { "ttl", credential.Ttl },
                            { "authToken", credential.AuthToken }
                        }
                    }
                };
            var response = await client.RequestAsync(ReqType.Post, "/peers", json).ConfigureAwait(false);
            var p = JObject.Parse(response.Content)["params"];
            var peerId = p["peer_id"].Value<string>();
            var token = p["token"].Value<string>();
            return new(client, peerId, token);
        }


        // ------ public methods ------ //

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _opened.Dispose();
            _closed.Dispose();
            _dataCalled.Dispose();
            _mediaCalled.Dispose();
            _expiresRemainingSecondNotified.Dispose();
            await _client.RequestAsync(ReqType.Delete, $"/peers/{_peerId}?token={_token}").ConfigureAwait(false);
        }

        public async Task<bool> ConfirmAliveAsync()
        {
            var response = await _client.RequestAsync(ReqType.Get, $"/peers/{_peerId}/status?token={_token}").ConfigureAwait(false);
            return !JObject.Parse(response.Content)["disconnected"].Value<bool>();
        }

        public async Task ChangeCredentialAsync(PeerCredential credential)
        {
            var json = new Dictionary<string, dynamic>()
            {
                { "timestamp", credential.Timestamp },
                { "ttl", credential.Ttl },
                { "authToken", credential.AuthToken }
            };
            await _client.RequestAsync(ReqType.Put, $"/peers/{_peerId}/credential?token={_token}", json).ConfigureAwait(false);
        }

        public async Task<DataChannel> CreateDataChannelAsync(IPEndPoint localEP = default)
        {
            if (localEP is null) // generate default end point
            {
                var port = new Random().Next(20000) + 10000;
                var usedPorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                    .Select(p => p.LocalEndPoint.Port).ToList();
                usedPorts.AddRange(IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port));
                usedPorts.AddRange(IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Select(p => p.Port));
                while (usedPorts.Contains(port++)) ;
                localEP = new(_host, port);
            }
            var dataChannel = await DataChannel.CreateNewAsync(
                _client, _peerId, _token,
                localEP, _dataCalled.ToTask(), _cts
            ).ConfigureAwait(false);
            return dataChannel;
        }

        public async Task<MediaChannel> CreateMediaChannelAsync(
            MediaParameters videoParameters = default,
            MediaParameters audioParameters = default,
            IPEndPoint localVideoEP = default,
            IPEndPoint localVideoRtcpEP = default,
            IPEndPoint localAudioEP = default,
            IPEndPoint localAudioRtcpEP = default
        )
        {
            var port = new Random().Next(20000) + 30000;
            var usedPorts = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
                .Select(p => p.LocalEndPoint.Port).ToList();
            usedPorts.AddRange(IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port));
            usedPorts.AddRange(IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Select(p => p.Port));
            if (localVideoEP is null) // generate default end point
            {
                while (usedPorts.Contains(port++)) ;
                localVideoEP = new(_host, port);
            }
            if (localVideoRtcpEP is null) // generate default end point
            {
                while (usedPorts.Contains(port++)) ;
                localVideoRtcpEP = new(_host, port);
            }
            if (localAudioEP is null) // generate default end point
            {
                while (usedPorts.Contains(port++)) ;
                localAudioEP = new(_host, port);
            }
            if (localAudioRtcpEP is null) // generate default end point
            {
                while (usedPorts.Contains(port++)) ;
                localAudioRtcpEP = new(_host, port);
            }
            var mediaChannel = await MediaChannel.CreateNewAsync(
                _client, _peerId, _token,
                videoParameters ?? new(0, "H264", 100, 90000), audioParameters ?? new(0, "opus", 111, 48000),
                localVideoEP, localVideoRtcpEP,
                localAudioEP, localAudioRtcpEP,
                _mediaCalled.ToTask(), _cts
            );
            return mediaChannel;
        }


        // ------ private methods ------ //

        private async Task ListenEventAsync()
        {
            // execute long polling
            var response = await _client.RequestAsync(ReqType.Get, $"/peers/{_peerId}/events?token={_token}", null).ConfigureAwait(false);
            if (response is null)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                return;
            }
            var p = JObject.Parse(response.Content);
            var e = p["event"].ToString();
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
                case "CONNECTION":
                    _dataCalled.OnNext(p["data_params"]["data_connection_id"].ToString());
                    _dataCalled.OnCompleted();
                    break;
                case "CALL":
                    _mediaCalled.OnNext(p["call_params"]["media_connection_id"].ToString());
                    _mediaCalled.OnCompleted();
                    break;
                case "EXPIRESIN":
                    _expiresRemainingSecondNotified.OnNext(int.Parse(p["remainingSec"].ToString()));
                    _expiresRemainingSecondNotified.OnCompleted();
                    break;
                case "ERROR":
                    Debug.WriteLine(p["error_message"]);
                    break;
                default:
                    throw new Exception(e);
            }
        }

    }
}
