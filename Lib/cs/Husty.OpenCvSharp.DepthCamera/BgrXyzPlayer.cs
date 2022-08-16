using System.Text;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using OpenCvSharp;
using Husty.OpenCvSharp.ImageStream;

namespace Husty.OpenCvSharp.DepthCamera;

/// <summary>
/// Playback BGRXYZ movie from binary file.
/// </summary>
public sealed class BgrXyzPlayer : IVideoStream<BgrXyzMat>
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

    private readonly long[] _indexes;
    private readonly BinaryReader _binReader;
    private readonly ObjectPool<BgrXyzMat> _bgrXyzPool;
    private int _positionIndex;
    private long _prevTime;


    // ------ properties ------ //

    public int Fps { get; }

    public int Channels => 6;

    public Size FrameSize { get; }

    public bool HasFrame { private set; get; }

    public int FrameCount => _indexes.Length;

    public int CurrentPosition => _positionIndex;


    // ------ constructors ------ //

    /// <summary>
    /// Player for video captured by depth camera
    /// </summary>
    /// <param name="filePath"></param>
    public BgrXyzPlayer(string filePath)
    {
        if (!File.Exists(filePath))
            throw new Exception("File doesn't exist!");
        _binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read), Encoding.ASCII);
        var fileFormatCode = Encoding.ASCII.GetString(_binReader.ReadBytes(8));
        if (fileFormatCode is not "HUSTY000") throw new Exception();
        var indexesPos = _binReader.ReadInt64();
        if (indexesPos <= 0)
            throw new Exception("Index positions are invalid.");
        _binReader.BaseStream.Position = indexesPos;
        var indexes = new List<long>();
        while (_binReader.BaseStream.Position < _binReader.BaseStream.Length)
            indexes.Add(_binReader.ReadInt64());
        if (indexes.Count < 5)
            throw new Exception("frame count is too small");
        _indexes = indexes.ToArray();

        _binReader.BaseStream.Position = 0;
        _binReader.BaseStream.Seek(_indexes[0], SeekOrigin.Begin);
        var ticks = _binReader.ReadInt64();
        var bgrDataSize = _binReader.ReadInt32();
        var bgrBytes = _binReader.ReadBytes(bgrDataSize);
        var xyzDataSize = _binReader.ReadInt32();
        var xyzBytes = _binReader.ReadBytes(xyzDataSize);
        using var bgrxyz = new BgrXyzMat(bgrBytes, xyzBytes);

        for (int i = 1; i < 5; i++)
        {
            ticks = _binReader.ReadInt64();
            _binReader.ReadInt32();
            _binReader.ReadBytes(bgrDataSize);
            _binReader.ReadInt32();
            _binReader.ReadBytes(xyzDataSize);
        }
        ticks /= 5;

        Fps = 1000 / TimeSpan.FromTicks(ticks).Milliseconds;
        FrameSize = new(bgrxyz.BGR.Width, bgrxyz.BGR.Height);

        _bgrXyzPool = new(2, () => new(
            new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_8UC3),
            new Mat(FrameSize.Height, FrameSize.Width, MatType.CV_16UC3))
        );
    }


    // ------ public methods ------ //

    public BgrXyzMat Read()
    {
        var frame = _bgrXyzPool.GetObject();
        if (TryRead(frame))
            return frame;
        else
            return null;
    }

    public bool TryRead(BgrXyzMat frame)
    {
        if (_positionIndex == FrameCount - 1)
            return false;
        _binReader.BaseStream.Seek(_indexes[_positionIndex++], SeekOrigin.Begin);
        var time = _binReader.ReadInt64();
        var ticks = time - _prevTime > 0 ? time - _prevTime : 0;
        Thread.Sleep(TimeSpan.FromTicks(ticks));
        _prevTime = time;
        var bgrDataSize = _binReader.ReadInt32();
        var bgrBytes = _binReader.ReadBytes(bgrDataSize);
        var xyzDataSize = _binReader.ReadInt32();
        var xyzBytes = _binReader.ReadBytes(xyzDataSize);
        frame.CopyFrom(bgrBytes, xyzBytes);
        HasFrame = true;
        return true;
    }

    public IObservable<BgrXyzMat> GetStream()
    {
        return Observable.Repeat(0, ThreadPoolScheduler.Instance)
            .Where(_ => _positionIndex < FrameCount)
            .Select(_ => Read())
            .TakeUntil(x => x is null)
            .Where(x => x is not null && !x.IsDisposed && !x.Empty())
            .Publish().RefCount();
    }

    public void Seek(int position)
    {
        if (position > -1 && position < FrameCount) _positionIndex = position;
        _binReader.BaseStream.Seek(_indexes[_positionIndex], SeekOrigin.Begin);
        _prevTime = _binReader.ReadInt64();
    }

    public void Dispose()
    {
        HasFrame = false;
        _binReader?.Close();
        _binReader?.Dispose();
    }

}
