using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Husty.IO
{

    public class BidirectionalDataStream : IDisposable
    {

        // ------- Fields ------- //

        private readonly Stream _writingStream;
        private readonly Stream _readingStream;


        // ------- Properties ------- //

        /// <summary>
        /// If streams are available
        /// </summary>
        public bool Available => _writingStream is not null && _readingStream is not null;

        
        // ------- Constructors ------- //

        public BidirectionalDataStream(Stream writingStream, Stream readingStream, int writeTimeout, int readTimeout)
        {
            _writingStream = writingStream;
            _readingStream = readingStream;
            if (_writingStream.CanTimeout)
                _writingStream.WriteTimeout = writeTimeout;
            if (_readingStream.CanTimeout)
                _readingStream.ReadTimeout = readTimeout;
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
        public bool WriteBinary(byte[] bytes)
        {
            try
            {
                _writingStream.Write(bytes, 0, bytes.Length);
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
        public async Task<bool> WriteBinaryAsync(byte[] bytes)
        {
            try
            {
                await _writingStream.WriteAsync(bytes, 0, bytes.Length);
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
        public byte[] ReadBinary()
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
        public async Task<byte[]> ReadBinaryAsync()
        {
            try
            {
                using var ms = new MemoryStream();
                var bytes = new byte[2048];
                var size = 0;
                do
                {
                    size = await _readingStream.ReadAsync(bytes, 0, bytes.Length);
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
                return WriteBinary(Encoding.UTF8.GetBytes(sendmsg));
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
                return await WriteBinaryAsync(Encoding.UTF8.GetBytes(sendmsg));
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
                return Encoding.UTF8.GetString(ReadBinary());
            }
            catch
            {
                return "";
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
                return Encoding.UTF8.GetString(await ReadBinaryAsync());
            }
            catch
            {
                return "";
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
                return await WriteStringAsync(JsonSerializer.Serialize(sendmsg));
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
                return JsonSerializer.Deserialize<T>(await ReadStringAsync());
            }
            catch
            {
                return default;
            }
        }

    }
}
