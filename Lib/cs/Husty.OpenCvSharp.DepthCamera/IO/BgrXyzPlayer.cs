using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;
using OpenCvSharp;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Playback BGRXYZ movie from binary file.
    /// </summary>
    public class BgrXyzPlayer : IVideoStream<BgrXyzMat>
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
        private int _positionIndex;


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
        public BgrXyzPlayer(string filePath, int fps = 5)
        {
            if (!File.Exists(filePath)) throw new Exception("File doesn't exist!");
            _binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read), Encoding.ASCII);
            var fileFormatCode = Encoding.ASCII.GetString(_binReader.ReadBytes(8));
            if (fileFormatCode is not "HUSTY000") throw new Exception();
            var indexesPos = _binReader.ReadInt64();
            if (indexesPos <= 0) throw new Exception("Index positions are invalid.");
            _binReader.BaseStream.Position = indexesPos;
            var indexes = new List<long>();
            while (_binReader.BaseStream.Position < _binReader.BaseStream.Length) indexes.Add(_binReader.ReadInt64());
            _indexes = indexes.ToArray();
            _binReader.BaseStream.Position = 0;
            Fps = fps;
            _binReader.BaseStream.Seek(_indexes[0], SeekOrigin.Begin);
            _binReader.ReadInt64();
            var bgrDataSize = _binReader.ReadInt32();
            var bgrBytes = _binReader.ReadBytes(bgrDataSize);
            var xyzDataSize = _binReader.ReadInt32();
            var xyzBytes = _binReader.ReadBytes(xyzDataSize);
            var bgrxyz = new BgrXyzMat(bgrBytes, xyzBytes);
            FrameSize = new(bgrxyz.BGR.Width, bgrxyz.BGR.Height);
        }


        // ------ public methods ------ //

        public BgrXyzMat Read()
        {
            GC.Collect();
            if (_positionIndex == FrameCount - 1) _positionIndex--;
            _binReader.BaseStream.Seek(_indexes[_positionIndex++], SeekOrigin.Begin);
            var time = _binReader.ReadInt64();
            var bgrDataSize = _binReader.ReadInt32();
            var bgrBytes = _binReader.ReadBytes(bgrDataSize);
            var xyzDataSize = _binReader.ReadInt32();
            var xyzBytes = _binReader.ReadBytes(xyzDataSize);
            var frame = new BgrXyzMat(bgrBytes, xyzBytes);
            HasFrame = true;
            return frame.Clone();
        }

        public IObservable<BgrXyzMat> GetStream()
        {
            return Observable.Interval(TimeSpan.FromMilliseconds(1000 / Fps), ThreadPoolScheduler.Instance)
                .Where(_ => _positionIndex < FrameCount)
                .Select(_ => Read())
                .Publish().RefCount();
        }

        public void Seek(int position)
        {
            if (position > -1 && position < FrameCount) _positionIndex = position;
        }

        public void Dispose()
        {
            HasFrame = false;
            _binReader?.Close();
            _binReader?.Dispose();
        }

    }
}
