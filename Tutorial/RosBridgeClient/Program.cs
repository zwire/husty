using Husty.Extensions;
using Husty.IO;
using Husty.RosBridge;

namespace RosBridgeClient;

internal class Program
{
    static void Main(string[] args)
    {
        var stream = WebSocketStream.CreateClient("127.0.0.1", 9090);
        var subscriber = RosSubscriber<RclInterfaces.Msg.Log>.Create(stream, "rosout");
        subscriber.MessageReceived.Subscribe(x => Console.WriteLine(x.msg));
        var publisher = RosPublisher<GeometryMsgs.Twist>.Create(stream, "/turtle1/cmd_vel");
        
        ConsoleEx.WaitKeyUntil(key =>
        {
            var x = 0.0;
            var y = 0.0;
            if (key is ConsoleKey.Escape) return true;
            else if (key is ConsoleKey.UpArrow) y++;
            else if (key is ConsoleKey.DownArrow) y--;
            else if (key is ConsoleKey.RightArrow) x++;
            else if (key is ConsoleKey.LeftArrow) x--;
            var msg = new GeometryMsgs.Twist(new(x, y, 0), new(0, 0, 0));
            publisher.WriteAsync(msg).Wait();
            return false;
        });
        subscriber.DisposeAsync().AsTask().Wait();
        publisher.DisposeAsync().AsTask().Wait();
        stream.Dispose();
    }
}