using Husty.Communication;
using Husty.Extensions;
using Husty.RosBridge;

using var stream = await WebSocketDataTransporter.CreateClientAsync("127.0.0.1", 9090);
using var subscriber = RosSubscriber<rcl_interfaces.msg.Log>.Create(stream, "/rosout");
subscriber.MessageReceived.Subscribe(x => Console.WriteLine(x.msg));
using var publisher = RosPublisher<geometry_msgs.msg.Twist>.Create(stream, "/turtle1/cmd_vel");

ConsoleEx.WaitKeyUntil(key =>
{
  var x = 0f;
  var y = 0f;
  if (key is ConsoleKey.Escape) return true;
  else if (key is ConsoleKey.UpArrow) y++;
  else if (key is ConsoleKey.DownArrow) y--;
  else if (key is ConsoleKey.RightArrow) x++;
  else if (key is ConsoleKey.LeftArrow) x--;
  var msg = new geometry_msgs.msg.Twist(new(x, y, 0), new(0, 0, 0));
  publisher.WriteAsync(msg).Wait();
  return false;
});