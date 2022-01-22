using System;
using System.Threading.Tasks;
using System.Net;
using RestSharp;

namespace Husty.SkywayGateway
{
    public sealed record PeerCredential(DateTimeOffset Timestamp, int Ttl, string AuthToken);

    public sealed record MediaParameters(int BandWidth, string Codec, int PayloadType, int SamplingRate);

    public sealed record DataConnectionInfo(IPEndPoint LocalEP, IPEndPoint RemoteEP);

    public sealed record MediaConnectionInfo(
        MediaParameters VideoParameters,
        MediaParameters AudioParameters,
        IPEndPoint LocalVideoEP,
        IPEndPoint LocalVideoRtcpEP,
        IPEndPoint LocalAudioEP,
        IPEndPoint LocalAudioRtcpEP,
        IPEndPoint RemoteVideoEP,
        IPEndPoint RemoteVideoRtcpEP,
        IPEndPoint RemoteAudioEP,
        IPEndPoint RemoteAudioRtcpEP
    );

    internal enum ReqType
    {
        Post,
        Delete,
        Get,
        Put
    }

    internal static class RestEx
    {

        public static async Task<RestResponse> RequestAsync(
            this RestClient client,
            ReqType type,
            string resource,
            object jsonContent = null, // dictionary or anonymous type object
            TimeSpan? timeOut = null   // default value is infinite
        )
        {
            var request = new RestRequest();
            if (resource is not null)
                request.Resource = resource;
            if (timeOut is TimeSpan ts)
                request.Timeout = ts.Milliseconds;
            if (jsonContent is not null)
                request.AddJsonBody(jsonContent);
            try
            {
                var response = type switch
                {
                    ReqType.Post    => await client.PostAsync(request),
                    ReqType.Delete  => await client.DeleteAsync(request),
                    ReqType.Get     => await client.GetAsync(request),
                    ReqType.Put     => await client.PutAsync(request),
                    _               => default
                };
                if (!response.IsSuccessful)
                    throw new InvalidRequestException($"failed to {type} {resource}.");
                return response;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                return null;
            }
        }

    }

}
