using System.Net;
using RestSharp;

namespace Husty.SkywayGateway;

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
        Dictionary<string, dynamic> jsonContent = null
    )
    {
        var request = new RestRequest();
        if (resource is not null)
            request.Resource = resource;
        if (jsonContent is not null)
            request.AddJsonBody(jsonContent);
        try
        {
            var response = type switch
            {
                ReqType.Post    => await client.PostAsync(request).ConfigureAwait(false),
                ReqType.Delete  => await client.DeleteAsync(request).ConfigureAwait(false),
                ReqType.Get     => await client.GetAsync(request).ConfigureAwait(false),
                ReqType.Put     => await client.PutAsync(request).ConfigureAwait(false),
                _               => default
            };
            if (!response.IsSuccessful)
                throw new InvalidOperationException($"failed to {type} {resource}.");
            return response;
        }
        catch (System.Net.Http.HttpRequestException)
        {
            return null;
        }
    }

}

