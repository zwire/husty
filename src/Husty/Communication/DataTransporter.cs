using System.Text;

namespace Husty.Communication;

public sealed class DataTransporter : DataTransporterBase
{

  // ------ fields ------ //

  private readonly Stream _writingStream;
  private readonly Stream _readingStream;
  private readonly StreamWriter _writer;
  private readonly StreamReader _reader;


  // ------ properties ------ //

  public override Stream BaseWritingStream => _writingStream;

  public override Stream BaseReadingStream => _readingStream;


  // ------ constructors ------ //

  internal DataTransporter(
    Stream writingStream,
    Stream readingStream,
    Encoding encoding,
    string newLine
  )
  {
    _writingStream = writingStream;
    _readingStream = readingStream;
    _writer = new(_writingStream, encoding) { NewLine = newLine };
    _reader = new(_readingStream, encoding);
  }


  // ------ protected methods ------ //

  protected override void DoDispose()
  {
    _writingStream?.Dispose();
    _readingStream?.Dispose();
  }

  protected override async Task<bool> DoTryWriteAsync(byte[] data, CancellationToken ct)
  {
    try
    {
      await _writingStream.WriteAsync(data, 0, data.Length, ct).ConfigureAwait(false);
      await _writingStream.FlushAsync().ConfigureAwait(false);
      return true;
    }
    catch
    {
      return false;
    }
  }

  protected override async Task<Result<byte[]>> DoTryReadAsync(int count, CancellationToken ct)
  {
    var bytes = new byte[count];
    try
    {
      var offset = 0;
      do
      {
        var size = await _readingStream.ReadAsync(bytes, offset, count, ct).ConfigureAwait(false);
        if (size is 0) break;
        offset += size;
        count -= size;
      } while (count > 0);
      if (offset is 0)
        return Result<byte[]>.Err(new("offset is 0"));
      return Result<byte[]>.Ok(bytes);
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
      await _writer.WriteLineAsync(new StringBuilder(data), ct).ConfigureAwait(false);
      await _writer.FlushAsync().ConfigureAwait(false);
      return true;
    }
    catch
    {
      return false;
    }
  }

  protected override async Task<Result<string>> DoTryReadLineAsync(CancellationToken ct)
  {
    try
    {
      var line = await _reader.ReadLineAsync(ct).ConfigureAwait(false);
      if (line is null)
        return Result<string>.Err(new("null"));
      return Result<string>.Ok(line);
    }
    catch
    {
      return Result<string>.Err(new());
    }
  }

}
