using System.Diagnostics;
using Husty.SkywayGateway;

namespace SkywayMediaConnection;

internal class Program
{
    // require GStreamer for media streaming
    // https://gstreamer.freedesktop.org/download/
    static void Main(string[] args)
    {
        Task.Run(async () =>
        {

            var apiKey = "API_KEY";    // get from https://webrtc.ecl.ntt.com/
            var localId = "monitor";
            var remoteId = "streamer";
            var mode = "call";            // listen or call

            // create my peer object
            await using var peer = await Peer.CreateNewAsync(apiKey, localId);
            Console.WriteLine("My ID = " + peer.PeerId);

            // create data channel
            await using var channel = await peer.CreateMediaChannelAsync();
            channel.Closed.Subscribe(_ => Console.WriteLine("channel was disconnected!"));

            // listen or call
            var info = mode is "listen"
                ? await channel.ListenAsync()
                : await channel.CallConnectionAsync(remoteId);

            // show and confirm connection information
            Console.WriteLine($"Local  : {info.LocalVideoEP.Address}:{info.LocalVideoEP.Port}");
            Console.WriteLine($"Remote : {info.RemoteVideoEP.Address}:{info.RemoteVideoEP.Port}");
            var alive = await channel.ConfirmAliveAsync();
            Console.WriteLine(alive is true ? "connected!" : "failed to connect!");
            Console.WriteLine();

            // publish test video
            Process.Start(
                $"C:\\gstreamer\\1.0\\msvc_x86_64\\bin\\gst-launch-1.0 " +
                $"-v videotestsrc " +
                $"! videoscale ! video/x-raw,width=320,height=240 " +
                $"! videorate ! video/x-raw,framerate=30/1 " +
                $"! x264enc ! rtph264pay " +
                $"! udpsink host={info.RemoteVideoEP.Address} port={info.RemoteVideoEP.Port} sync=false"
            );
            // subscribe video
            //Process.Start(
            //    $"C:\\gstreamer\\1.0\\msvc_x86_64\\bin\\gst-launch-1.0 " +
            //    $"-v udpsrc port={info.LocalVideoEP.Port} " +
            //    $"! application/x-rtp,media=video,encoding-name=H264 " +
            //    $"! queue ! rtph264depay ! avdec_h264 ! videoconvert ! autovideosink"
            //);

            Console.WriteLine("press ESC key to finish ...");
            while (Console.ReadKey().Key is not ConsoleKey.Escape)
                Thread.Sleep(50);

        }).Wait();

        Console.WriteLine("completed.");
    }
}
