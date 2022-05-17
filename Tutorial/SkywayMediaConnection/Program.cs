using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using Husty.SkywayGateway;

namespace SkywayMediaConnection
{
    // *** This sample code does not work now. I don't know how to use SDP configuration parameters. ***

    // Subscribe only
    // To publish network stream, you must build OpenCvSharp with GStreamer
    internal class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {

                var apiKey = "API_KEY";    // publish at https://webrtc.ecl.ntt.com/
                var localId = "monitor";
                var remoteId = "streamer";
                var mode = "connect";            // listen or connect

                // create my peer object
                await using var peer = await Peer.CreateNewAsync(apiKey, localId);
                Console.WriteLine("My ID = " + peer.PeerId);

                // create data channel
                await using var channel = await peer.CreateMediaChannelAsync();
                channel.Closed.Subscribe(_ => Console.WriteLine("channel was disconnected!"));

                // listen or connect
                var info = mode is "listen"
                    ? await channel.ListenAsync()
                    : await channel.CallConnectionAsync(remoteId);

                // show and confirm connection information
                Console.WriteLine($"Local  : {info.LocalVideoEP.Address}:{info.LocalVideoEP.Port}");
                Console.WriteLine($"Remote : {info.RemoteVideoEP.Address}:{info.RemoteVideoEP.Port}");
                var alive = await channel.ConfirmAliveAsync();
                Console.WriteLine(alive is true ? "connected!" : "failed to connect!");
                Console.WriteLine();

                var cap = new VideoCapture($"rtp://{info.LocalVideoEP.Address}:{info.LocalVideoEP.Port}");
                var frame = new Mat();
                while (cap.Read(frame))
                {
                    Cv2.ImShow(" ", frame);
                    Cv2.WaitKey(1);
                }

                Console.WriteLine("press ESC key to finish ...");
                while (Console.ReadKey().Key is not ConsoleKey.Escape)
                    Thread.Sleep(50);

            }).Wait();

            Console.WriteLine("completed.");
        }
    }
}
