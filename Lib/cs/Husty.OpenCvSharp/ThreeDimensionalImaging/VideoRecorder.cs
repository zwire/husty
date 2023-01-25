using System.Text;

namespace Husty.OpenCvSharp.ThreeDimensionalImaging;

public class VideoRecorder : IDisposable
{

    // ------ fields ------ //

    private readonly BinaryWriter _binWriter;
    private readonly List<long> _indexes = new();
    private readonly object _locker = new();
    private readonly DateTime _firstTime;
    private bool _isDisposed;


    // ------ constructors ------ //

    public VideoRecorder(string filePath)
    {
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            filePath = Path.GetFileName(filePath);
        _binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create), Encoding.ASCII);
        var fileFormatCode = Encoding.ASCII.GetBytes("HUSTY002");
        _binWriter.Write(fileFormatCode);
        _binWriter.Write(0L);
        _binWriter.Write(-1L);
        _firstTime = DateTime.Now;
    }


    // ------ public methods ------ //

    public void Write(BgrXyzImage frame, byte[]? userData = null)
    {
        lock (_locker)
        {
            if (!_isDisposed)
            {
                _indexes.Add(_binWriter.BaseStream.Position);
                _binWriter.Write((DateTime.Now - _firstTime).Ticks);
                if (userData is null || userData.Length is 0)
                {
                    _binWriter.Write((ushort)0);
                }
                else
                {
                    _binWriter.Write((ushort)userData.Length);
                    _binWriter.Write(userData);
                }
                var (bgr, x, y, z) = frame;
                var bgrBytes = bgr.ImEncode();
                var xBytes = x.ImEncode();
                var yBytes = y.ImEncode();
                var zBytes = z.ImEncode();
                _binWriter.Write(sizeof(int) * 4 + bgrBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length);
                _binWriter.Write(bgrBytes.Length);
                _binWriter.Write(bgrBytes);
                _binWriter.Write(xBytes.Length);
                _binWriter.Write(xBytes);
                _binWriter.Write(yBytes.Length);
                _binWriter.Write(yBytes);
                _binWriter.Write(zBytes.Length);
                _binWriter.Write(zBytes);
            }
        }
    }

    public void Dispose()
    {
        lock (_locker)
        {
            _isDisposed = true;
            _binWriter.Seek(16, SeekOrigin.Begin);
            _binWriter.Write(_binWriter.BaseStream.Length);
            _binWriter.Seek(0, SeekOrigin.End);
            _indexes.ForEach(_binWriter.Write);
            _binWriter.Flush();
            _binWriter.Close();
            _binWriter.Dispose();
        }
    }

}
