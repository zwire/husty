using System.Text;

namespace Husty.OpenCvSharp.DepthCamera.IO;

/// <summary>
/// Save BGRXYZ movie as binary file.
/// </summary>
public sealed class BgrXyzRecorder : IDisposable
{

    //
    // Data Structure
    // 
    //   byte        content
    //  
    //    1        Format Code
    //    8       Stream Length
    //    
    //    8        Time Stamp
    //    4         BGR Size
    // BGR Size     BGR Frame
    //    4         XYZ Size
    // XYZ Size     XYZ Frame
    // 
    //    .
    //    .
    //    .
    //    
    //    8      Frame 1 Position
    //    8      Frame 2 Position
    //    8      Frame 3 Position
    //   
    //    .
    //    .
    //    .
    //    

    // ------ fields ------ //

    private readonly BinaryWriter _binWriter;
    private readonly List<long> _indexes = new();
    private readonly DateTimeOffset _firstTime;
    private readonly object _locker = new();
    private bool _isDisposed;


    // ------ constructors ------ //

    /// <summary>
    /// Movie recorder for Depth camera
    /// </summary>
    /// <param name="filePath"></param>
    public BgrXyzRecorder(string filePath)
    {
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            filePath = Path.GetFileName(filePath);
        _binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create), Encoding.ASCII);
        var fileFormatCode = Encoding.ASCII.GetBytes("HUSTY000");
        _binWriter.Write(fileFormatCode);
        _binWriter.Write(-1L);
        _firstTime = DateTimeOffset.Now;
    }


    // ------ public methods ------ //

    /// <summary>
    /// Write frames of the time.
    /// </summary>
    /// <param name="Bgrxyz"></param>
    public void WriteFrame(BgrXyzMat Bgrxyz)
    {
        lock (_locker)
        {
            if (!_isDisposed)
            {
                _indexes.Add(_binWriter.BaseStream.Position);
                _binWriter.Write((DateTimeOffset.Now - _firstTime).Ticks);
                var (bgr, xyz) = Bgrxyz.YmsEncode();
                _binWriter.Write(bgr.Length);
                _binWriter.Write(bgr);
                _binWriter.Write(xyz.Length);
                _binWriter.Write(xyz);
            }
        }
    }

    /// <summary>
    /// Finalize recording
    /// </summary>
    public void Dispose()
    {
        lock (_locker)
        {
            _isDisposed = true;
            _binWriter.Seek(8, SeekOrigin.Begin);
            _binWriter.Write(_binWriter.BaseStream.Length);
            _binWriter.Seek(0, SeekOrigin.End);
            _indexes.ForEach(p => _binWriter.Write(p));
            _binWriter.Flush();
            _binWriter.Close();
            _binWriter.Dispose();
        }
    }

}
