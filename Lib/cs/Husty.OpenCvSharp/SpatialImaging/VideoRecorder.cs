using System.Text;

namespace Husty.OpenCvSharp.SpatialImaging;

public class VideoRecorder : IDisposable
{

    // ------ fields ------ //

    private readonly BinaryWriter _binWriter;
    private readonly List<long> _indexes = new();
    private readonly DateTime _firstTime;
    private readonly object _locker = new();
    private bool _isDisposed;


    // ------ constructors ------ //

    public VideoRecorder(string filePath)
    {
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            filePath = Path.GetFileName(filePath);
        _binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create), Encoding.ASCII);
        var fileFormatCode = Encoding.ASCII.GetBytes("HUSTY001");
        _firstTime = DateTime.Now.ToUniversalTime();
        _binWriter.Write(fileFormatCode);
        _binWriter.Write(_firstTime.ToBinary());
        _binWriter.Write(-1L);
    }


    // ------ public methods ------ //

    public void Write(SpatialImage frame)
    {
        lock (_locker)
        {
            if (!_isDisposed)
            {
                _indexes.Add(_binWriter.BaseStream.Position);
                _binWriter.Write((DateTime.Now.ToUniversalTime() - _firstTime).Ticks);
                var (bgr, xyz) = frame;
                var bgrBytes = bgr.ImEncode();
                var xyzBytes = xyz.ImEncode();
                _binWriter.Write(bgrBytes.Length);
                _binWriter.Write(bgrBytes);
                _binWriter.Write(xyzBytes.Length);
                _binWriter.Write(xyzBytes);
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
            _indexes.ForEach(p => _binWriter.Write(p));
            _binWriter.Flush();
            _binWriter.Close();
            _binWriter.Dispose();
        }
    }

}
