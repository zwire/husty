using System.Text.Json;

namespace Husty.IO;

public sealed class BidirectionalDataStream : IDisposable
{

    // ------- Fields ------- //

    private readonly Stream _writingStream;
    private readonly Stream _readingStream;
    private readonly StreamWriter _writer;
    private readonly StreamReader _reader;


    // ------- Properties ------- //

    /// <summary>
    /// If streams are available
    /// </summary>
    public bool Available => _writingStream is not null && _readingStream is not null;

    
    // ------- Constructors ------- //

    public BidirectionalDataStream(Stream writingStream, Stream readingStream, int writeTimeout = -1, int readTimeout = -1)
    {
        _writingStream = writingStream;
        _readingStream = readingStream;
        if (_writingStream.CanTimeout)
            _writingStream.WriteTimeout = writeTimeout;
        if (_readingStream.CanTimeout)
            _readingStream.ReadTimeout = readTimeout;
        _writer = new(_writingStream);
        _reader = new(_readingStream);
    }


    // ------- Methods ------- //

    public void Dispose()
    {
        _writingStream?.Dispose();
        _readingStream?.Dispose();
    }

    /// <summary>
    /// Write byte array directly
    /// </summary>
    /// <param name="bytes"></param>
    public bool WriteBytes(byte[] bytes)
    {
        try
        {
            _writingStream.Write(bytes, 0, bytes.Length);
            _writingStream.Flush();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Write byte array directly
    /// </summary>
    /// <param name="bytes"></param>
    public async Task<bool> WriteBytesAsync(byte[] bytes)
    {
        try
        {
            await _writingStream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            await _writingStream.FlushAsync().ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Read byte array directly
    /// </summary>
    /// <returns>byte array, and if some error occured it returns null</returns>
    public byte[] ReadBytes()
    {
        try
        {
            using var ms = new MemoryStream();
            var bytes = new byte[2048];
            var size = 0;
            do
            {
                size = _readingStream.Read(bytes, 0, bytes.Length);
                if (size is 0) break;
                ms.Write(bytes, 0, size);
            } while (size is 2048);
            return ms.ToArray();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Read byte array directly
    /// </summary>
    /// <returns>byte array, and if some error occured it returns null</returns>
    public async Task<byte[]> ReadBytesAsync()
    {
        try
        {
            using var ms = new MemoryStream();
            var bytes = new byte[2048];
            var size = 0;
            do
            {
                size = await _readingStream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                if (size is 0) break;
                ms.Write(bytes, 0, size);
            } while (size is 2048);
            return ms.ToArray();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Write string
    /// </summary>
    /// <param name="sendmsg"></param>
    public bool WriteString(string sendmsg)
    {
        try
        {
            _writer.WriteLine(sendmsg);
            _writer.Flush();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Write string
    /// </summary>
    /// <param name="sendmsg"></param>
    public async Task<bool> WriteStringAsync(string sendmsg)
    {
        try
        {
            await _writer.WriteLineAsync(sendmsg).ConfigureAwait(false);
            await _writer.FlushAsync().ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Read string
    /// </summary>
    /// <returns>string value, and if some error occured it returns null</returns>
    public string ReadString()
    {
        try
        {
            return _reader.ReadLine();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Read string
    /// </summary>
    /// <returns>string value, and if some error occured it returns null</returns>
    public async Task<string> ReadStringAsync()
    {
        try
        {
            return await _reader.ReadLineAsync().ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Write JSON object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sendmsg"></param>
    /// <returns></returns>
    public bool WriteAsJson<T>(T sendmsg)
    {
        try
        {
            return WriteString(JsonSerializer.Serialize(sendmsg));
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Write JSON object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sendmsg"></param>
    /// <returns></returns>
    public async Task<bool> WriteAsJsonAsync<T>(T sendmsg)
    {
        try
        {
            return await WriteStringAsync(JsonSerializer.Serialize(sendmsg)).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Read JSON object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>deserialized JSON object, and if some error occured it returns null</returns>
    public T ReadAsJson<T>()
    {
        try
        {
            return JsonSerializer.Deserialize<T>(ReadString());
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Read JSON object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>deserialized JSON object, and if some error occured it returns null</returns>
    public async Task<T> ReadAsJsonAsync<T>()
    {
        try
        {
            return JsonSerializer.Deserialize<T>(await ReadStringAsync().ConfigureAwait(false));
        }
        catch
        {
            return default;
        }
    }

}
