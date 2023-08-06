using Husty.Communication;

namespace Tests_.Husty;

public class Communication_Test
{

  internal record struct Message(string Greeting, int Number);

  private static async Task<Message[]> EchoAsync(
      IDataTransporter stream,
      string msg,
      CancellationToken ct
  )
  {
    var msgs = new List<Message>();
    for (int i = 0; i < 10; i++)
    {
      await stream.TryWriteAsJsonAsync<Message>(new(msg, i), default, ct);
      await Task.Delay(10, ct);
      if (ct.IsCancellationRequested) continue;
      var (s, v) = await stream.TryReadAsJsonAsync<Message>(default, ct);
      if (s) msgs.Add(v);
      await Task.Delay(10, ct);
    }
    stream.Dispose();
    return msgs.ToArray();
  }

  private static async Task<Message[]> Test_TcpServerAsync(string msg, CancellationToken ct)
  {
    using var sock = new TcpSocketServer(3000);
    var (success, stream) = await sock.GetStreamAsync(default, ct);
    if (!success) return default!;
    return await EchoAsync(stream, msg, ct);
  }

  private static async Task<Message[]> Test_TcpClientAsync(string msg, CancellationToken ct)
  {
    var sock = new TcpSocketClient("127.0.0.1", 3000);
    var (success, stream) = await sock.GetStreamAsync();
    if (!success) return default!;
    return await EchoAsync(stream, msg, ct);
  }

  private static async Task<Message[]> Test_WebSocketServerAsync(string msg, CancellationToken ct)
  {
    return await EchoAsync(await WebSocketDataTransporter.CreateServerAsync("127.0.0.1", 3000), msg, ct);
  }

  private static async Task<Message[]> Test_WebSocketClientAsync(string msg, CancellationToken ct)
  {
    return await EchoAsync(await WebSocketDataTransporter.CreateClientAsync("127.0.0.1", 3000), msg, ct);
  }


  // ------ test methods ------ //

  [Fact]
  public void Test_Echo()
  {
    var cts = new CancellationTokenSource();
    cts.CancelAfter(10000);
    var task1 = Test_TcpServerAsync("A", cts.Token);
    var task2 = Test_TcpClientAsync("B", cts.Token);
    Task.WhenAll(task1, task2).Wait();
    var task3 = Test_WebSocketServerAsync("C", cts.Token);
    var task4 = Test_WebSocketClientAsync("D", cts.Token);
    Task.WhenAll(task3, task4).Wait();

    for (int i = 0; i < 10; i++)
    {
      Assert.Equal("B", task1.Result[i].Greeting);
      Assert.Equal(i, task1.Result[i].Number);
      Assert.Equal("A", task2.Result[i].Greeting);
      Assert.Equal(i, task2.Result[i].Number);
      Assert.Equal("D", task3.Result[i].Greeting);
      Assert.Equal(i, task3.Result[i].Number);
      Assert.Equal("C", task4.Result[i].Greeting);
      Assert.Equal(i, task4.Result[i].Number);
    }
  }

}
