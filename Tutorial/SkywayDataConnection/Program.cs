using Husty.SkywayGateway;

var apiKey = "API_KEY";         // get from https://console-webrtc-free.ecl.ntt.com/users/login
var mode = "listen";            // listen or call
var localId = mode is "listen" ? "listener" : "caller";
var remoteId = mode is "listen" ? "caller" : "listener";

// create my peer object
await using var peer = await Peer.CreateNewAsync(apiKey, localId);
Console.WriteLine("My ID = " + peer.PeerId);

// create data channel
await using var channel = await peer.CreateDataChannelAsync();
channel.Closed.Subscribe(_ => Console.WriteLine("channel was disconnected!"));

// listen or call
using var stream = mode is "listen"
    ? await channel.ListenAsync()
    : await channel.CallConnectionAsync(remoteId);

// show and confirm connection information
var info = channel.ConnectionInfo;
Console.WriteLine($"Local  : {info.LocalEP.Address}:{info.LocalEP.Port}");
Console.WriteLine($"Remote : {info.RemoteEP.Address}:{info.RemoteEP.Port}");
var alive = await channel.ConfirmAliveAsync();
Console.WriteLine(alive is true ? "connected!" : "failed to connect!");
Console.WriteLine();

// loop
_ = Task.Run(async () =>
{
  var count = 0;
  while (true)
  {
    // send
    var msg = $"Message {count++} from {localId}.";
    await stream.TryWriteLineAsync(msg);
    Console.WriteLine("---> : " + msg);
    // receive
    var rcv = await stream.TryReadLineAsync();
    if (rcv.HasValue)
      Console.WriteLine("<--- : " + rcv.Value);
    await Task.Delay(1000);
  }
});

Console.WriteLine("press ESC key to finish ...");
while (Console.ReadKey().Key is not ConsoleKey.Escape)
  Thread.Sleep(50);

Console.WriteLine("completed.");