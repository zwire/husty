using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace Husty.Communication;

public sealed class WebSocketDataTransporter : DataTransporterBase
{

  // ------ fields ------ //

  private readonly WebSocket _socket;


  // ------ properties ------ //

  public override Stream BaseWritingStream => throw new NotImplementedException();

  public override Stream BaseReadingStream => throw new NotImplementedException();


  // ------ properties ------ //

  public bool IsOpened => _socket.State is WebSocketState.Open;

  public bool IsClosed =>
      _socket.State is WebSocketState.Closed ||
      _socket.State is WebSocketState.CloseSent ||
      _socket.State is WebSocketState.CloseReceived;

  public bool IsAborted => _socket.State is WebSocketState.Aborted;


  // ------ constructors ------ //

  private WebSocketDataTransporter(WebSocket socket)
  {
    _socket = socket;
  }


  // ------ inherited methods ------ //

  protected override void DoDispose()
  {
    if (!IsClosed && !IsAborted) CloseAsync().Wait();
    _socket.Dispose();
  }

  protected override async Task<bool> DoTryWriteAsync(byte[] data, CancellationToken ct)
  {
    try
    {
      await _socket.SendAsync(data.AsMemory(), WebSocketMessageType.Binary, true, ct);
      return true;
    }
    catch
    {
      return false;
    }
  }

  protected override async Task<Result<byte[]>> DoTryReadAsync(int count, CancellationToken ct)
  {
    try
    {
      var data = new byte[count];
      var result = await _socket.ReceiveAsync(data.AsMemory(), ct).ConfigureAwait(false);
      if (result.MessageType is WebSocketMessageType.Close)
      {
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, ct).ConfigureAwait(false);
        return Result<byte[]>.Err(new("closed"));
      }
      data = data.AsSpan(new Range(0, result.Count)).ToArray();
      while (!result.EndOfMessage)
      {
        var d0 = data;
        var d1 = new byte[count];
        result = await _socket.ReceiveAsync(d1.AsMemory(), ct).ConfigureAwait(false);
        data = new byte[d0.Length + d1.Length];
        Array.Copy(d0, data, d0.Length);
        Array.Copy(d1, 0, data, d0.Length, d1.Length);
      }
      return Result<byte[]>.Ok(data);
    }
    catch
    {
      return Result<byte[]>.Err(new());
    }
  }

  protected override async Task<bool> DoTryWriteLineAsync(string data, CancellationToken ct)
  {
    try
    {
      await _socket.SendAsync(Encoding.GetBytes(data + NewLine), WebSocketMessageType.Text, true, ct).ConfigureAwait(false);
      return true;
    }
    catch
    {
      return false;
    }
  }

  protected override async Task<Result<string>> DoTryReadLineAsync(CancellationToken ct)
  {
    var txt = "";
    while (true)
    {
      var result = await DoTryReadAsync(4096, ct).ConfigureAwait(false);
      if (!result.IsOk)
        return Result<string>.Err(new("failed to read data"));
      txt += Encoding.GetString(result.Unwrap());
      if (NewLine is "" || txt.Contains(NewLine)) 
        return Result<string>.Ok(txt.TrimEnd());
    }
  }


  // ------ static methods ------ //

  public static async Task<WebSocketDataTransporter> CreateServerAsync(string ip, int port, string? suffix = default)
  {
    var listener = new HttpListener();
    listener.Prefixes.Add($"http://{ip}:{port}/{suffix ?? ""}");
    listener.Start();
    var context = await listener.GetContextAsync().ConfigureAwait(false);
    if (!context.Request.IsWebSocketRequest)
    {
      context.Response.StatusCode = 400;
      context.Response.Close();
      throw new Exception("Closed: (Response: 400)");
    }
    var context2 = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
    return new(context2.WebSocket);
  }

  public static async Task<WebSocketDataTransporter> CreateClientAsync(
      string ip,
      int port,
      string? suffix = default,
      TimeSpan timeout = default,
      CancellationToken ct = default
  )
  {
    if (ct.IsCancellationRequested) throw new Exception();
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    if (timeout != default) cts.CancelAfter(timeout);
    var socket = new ClientWebSocket();
    await socket.ConnectAsync(new($"ws://{ip}:{port}/{suffix ?? ""}"), cts.Token).ConfigureAwait(false);
    return new(socket);
  }

  public async Task CloseAsync(CancellationToken ct = default)
  {
    if (IsClosed) return;
    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, ct).ConfigureAwait(false);
  }

}
