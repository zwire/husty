using Husty.Extensions;
using Husty.IO;
using Husty.RosBridge;

namespace RosBridgeClient;

internal class Program
{
    static void Main(string[] args)
    {
        var stream = WebSocketStream.CreateClient("192.168.6.239", 9090);
        var subscriber = RosSubscriber<rcl_interfaces.Log>.Create(stream, "rosout");
        subscriber.MessageReceived.Subscribe(x => Console.WriteLine(x.msg));
        var publisher = RosPublisher<geometry_msgs.Twist>.Create(stream, "/turtle1/cmd_vel");
        
        ConsoleEx.WaitKeyUntil(key =>
        {
            var x = 0.0;
            var y = 0.0;
            if (key is ConsoleKey.Escape) return true;
            else if (key is ConsoleKey.UpArrow) y++;
            else if (key is ConsoleKey.DownArrow) y--;
            else if (key is ConsoleKey.RightArrow) x++;
            else if (key is ConsoleKey.LeftArrow) x--;
            var msg = new geometry_msgs.Twist(new(x, y, 0), new(0, 0, 0));
            publisher.WriteAsync(msg).Wait();
            return false;
        });
        subscriber.DisposeAsync().AsTask().Wait();
        publisher.DisposeAsync().AsTask().Wait();
        stream.Dispose();
    }
}