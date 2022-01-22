using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Husty.SkywayGateway
{
    public sealed class MediaChannel : IAsyncDisposable
    {

        // ------ fields ------ //

        private readonly RestClient _client;
        private readonly string _peerId;
        private readonly string _token;
        private readonly string _videoId;
        private readonly string _videoRtcpId;
        private readonly string _audioId;
        private readonly string _audioRtcpId;
        private readonly MediaConnectionInfo _info;
        private readonly CancellationTokenSource _cts;
        private readonly Task<string> _called;
        private string _mediaConnectionId;


        // ------ constructors ------ //

        private MediaChannel(
            RestClient client,
            string peerId,
            string token,
            string videoId,
            string videoRtcpId,
            string audioId,
            string audioRtcpId,
            MediaConnectionInfo info,
            Task<string> called,
            CancellationTokenSource cts
        )
        {
            _client = client;
            _peerId = peerId;
            _token = token;
            _videoId = videoId;
            _videoRtcpId = videoRtcpId;
            _audioId = audioId;
            _audioRtcpId = audioRtcpId;
            _info = info;
            _cts = cts;
            _called = called;
        }

        internal static async Task<MediaChannel> CreateNewAsync(
            RestClient client,
            string peerId,
            string token,
            MediaParameters videoParameters,
            MediaParameters audioParameters,
            IPEndPoint localVideoEP,
            IPEndPoint localAudioEP,
            IPEndPoint localVideoRtcpEP,
            IPEndPoint localAudioRtcpEP,
            Task<string> called,
            CancellationTokenSource cts
        )
        {
            var response = await client.RequestAsync(ReqType.Post, "/media", new { is_video = true });
            var p = JObject.Parse(response.Content);
            var videoId = p["media_id"].Value<string>();
            var remoteVideoEP = new IPEndPoint(IPAddress.Parse(p["ip_v4"].Value<string>()), p["port"].Value<int>());

            response = await client.RequestAsync(ReqType.Post, "/media", new { is_video = false });
            p = JObject.Parse(response.Content);
            var audioId = p["media_id"].Value<string>();
            var remoteAudioEP = new IPEndPoint(IPAddress.Parse(p["ip_v4"].Value<string>()), p["port"].Value<int>());

            response = await client.RequestAsync(ReqType.Post, "/media/rtcp", new { });
            p = JObject.Parse(response.Content);
            var videoRtcpId = p["rtcp_id"].Value<string>();
            var remoteVideoRtcpEP = new IPEndPoint(IPAddress.Parse(p["ip_v4"].Value<string>()), p["port"].Value<int>());

            response = await client.RequestAsync(ReqType.Post, "/media/rtcp", new { });
            p = JObject.Parse(response.Content);
            var audioRtcpId = p["rtcp_id"].Value<string>();
            var remoteAudioRtcpEP = new IPEndPoint(IPAddress.Parse(p["ip_v4"].Value<string>()), p["port"].Value<int>());

            return new(
                client, peerId, token,
                videoId, videoRtcpId, audioId, audioRtcpId,
                new(
                    videoParameters, audioParameters,
                    localVideoEP, localVideoRtcpEP, localAudioEP, localAudioRtcpEP,
                    remoteVideoEP, remoteVideoRtcpEP, remoteAudioEP, remoteAudioRtcpEP
                ), called, cts
            );
        }


        // ------ public methods ------ //

        public async ValueTask DisposeAsync()
        {
            if (_mediaConnectionId is not null)
                await _client.RequestAsync(ReqType.Delete, $"/media/connections/{_mediaConnectionId}");
            await _client.RequestAsync(ReqType.Delete, $"/media/rtcp/{_videoRtcpId}");
            await _client.RequestAsync(ReqType.Delete, $"/media/rtcp/{_audioRtcpId}");
            await _client.RequestAsync(ReqType.Delete, $"/media/{_videoId}");
            await _client.RequestAsync(ReqType.Delete, $"/media/{_audioId}");
        }

        public async Task<bool> ConfirmAliveAsync()
        {
            if (_mediaConnectionId is null)
                return false;
            var response = await _client.RequestAsync(ReqType.Get, $"/media/connections/{_mediaConnectionId}/status");
            return JObject.Parse(response.Content)["open"].Value<bool>();
        }

        public async Task<MediaConnectionInfo> ListenAsync()
        {
            _mediaConnectionId = await _called;
            await AnswerAsync();
            var e = await ListenEventAsync("READY", _mediaConnectionId, _client, _cts.Token);
            if (e is null)
                throw new InvalidRequestException("failed to confirm ready.");
            return _info;
        }

        public async Task<MediaConnectionInfo> CallConnectionAsync(string remotePeerId)
        {
            var json = new
            {
                peer_id = _peerId,
                token = _token,
                target_id = remotePeerId,
                constraints = GetConstraints(),
                redirect_params = GetRedirectParams()
            };
            var response = await _client.RequestAsync(ReqType.Post, "/media/connections", json);
            _mediaConnectionId = JObject.Parse(response.Content)["params"]["media_connection_id"].Value<string>();
            var e = await ListenEventAsync("READY", _mediaConnectionId, _client, _cts.Token);
            if (e is null)
                throw new InvalidRequestException("failed to confirm ready.");
            return _info;
        }


        // ------ private methods ------ //

        private async Task AnswerAsync()
        {
            var json = new Dictionary<string, dynamic>
            {
                { "constraints", GetConstraints() },
                { "redirect_params", GetRedirectParams() }
            };
            await _client.RequestAsync(ReqType.Post, $"/media/connections/{_mediaConnectionId}/answer", json);
        }

        private dynamic GetConstraints()
        {
            return new Dictionary<string, dynamic>
            {
                { "video", true },
                { "videoReceiveEnabled", true },
                { "audio", true },
                { "audioReceiveEnabled", true },
                {
                    "video_params", new Dictionary<string, dynamic>
                    {
                        { "band_width", _info.VideoParameters.BandWidth },
                        { "codec", _info.VideoParameters.Codec },
                        { "media_id", _videoId },
                        { "rtcp_id", _videoRtcpId },
                        { "payload_type", _info.VideoParameters.PayloadType },
                        { "sampling_rate", _info.VideoParameters.SamplingRate }
                    }
                },
                {
                    "audio_params", new Dictionary<string, dynamic>
                    {
                        { "band_width", _info.AudioParameters.BandWidth },
                        { "codec", _info.AudioParameters.Codec },
                        { "media_id", _audioId },
                        { "rtcp_id", _audioRtcpId },
                        { "payload_type", _info.AudioParameters.PayloadType },
                        { "sampling_rate", _info.AudioParameters.SamplingRate }
                    }
                }
            };
        }

        private dynamic GetRedirectParams()
        {
            return new Dictionary<string, dynamic>
            {
                {
                    "video", new Dictionary<string, dynamic>
                    {
                        { "ip_v4", _info.LocalVideoEP.Address.ToString() },
                        { "port", _info.LocalVideoEP.Port }
                    }
                },
                {
                    "video_rtcp", new Dictionary<string, dynamic>
                    {
                        { "ip_v4", _info.LocalVideoRtcpEP.Address.ToString() },
                        { "port", _info.LocalVideoRtcpEP.Port }
                    }
                },
                {
                    "audio", new Dictionary<string, dynamic>
                    {
                        { "ip_v4", _info.LocalAudioEP.Address.ToString() },
                        { "port", _info.LocalAudioEP.Port }
                    }
                },
                {
                    "audio_rtcp", new Dictionary<string, dynamic>
                    {
                        { "ip_v4", _info.LocalAudioRtcpEP.Address.ToString() },
                        { "port", _info.LocalAudioRtcpEP.Port }
                    }
                }
            };
        }

        private static async Task<string> ListenEventAsync(
           string type,
           string mediaConnectionId,
           RestClient client,
           CancellationToken ct
        )
        {
            while (!ct.IsCancellationRequested)
            {
                var response = await client.RequestAsync(ReqType.Get, $"/media/connections/{mediaConnectionId}/events");
                var p = JObject.Parse(response.Content);
                var e = p["event"].ToString();
                if (e is "ERROR")
                {
                    Debug.WriteLine(p["error_message"]);
                }
                if (type != "*" && e != type)
                {
                    continue;
                }
                return e switch
                {
                    "READY" => "",
                    "STREAM" => "",
                    "CLOSE" => "",
                    _ => null,
                };
            }
            return null;
        }

    }
}
