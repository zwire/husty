using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Linq;
using OpenCvSharp;

namespace Husty.OpenCvSharp.DepthCamera
{
    /// <summary>
    /// Playback BGRXYZ movie from binary file.
    /// </summary>
    public class BgrXyzPlayer : IDisposable
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

        // ------- Fields ------- //

        private readonly BinaryReader _binReader;
        private readonly long[] _indexes;
        private int _positionIndex;
        private int _interval => 800 / Fps;


        // ------- Properties ------- //

        public int Fps { get; }

        public int FrameCount => _indexes.Length;

        public Size ColorFrameSize { get; }

        public Size DepthFrameSize { get; }


        // ------- Constructor ------- //

        /// <summary>
        /// Player for Movies captured by Depth Camera
        /// </summary>
        /// <param name="filePath"></param>
        public BgrXyzPlayer(string filePath, int fps = 15)
        {
            if (!File.Exists(filePath)) throw new Exception("File doesn't exist!");
            _binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read), Encoding.ASCII);
            var fileFormatCode = Encoding.ASCII.GetString(_binReader.ReadBytes(8));
            if (fileFormatCode is not "HUSTY000") throw new Exception();
            var indexesPos = _binReader.ReadInt64();
            if (indexesPos <= 0) throw new Exception();
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
            ColorFrameSize = new(bgrxyz.BGR.Width, bgrxyz.BGR.Height);
            DepthFrameSize = new(bgrxyz.Depth16.Width, bgrxyz.Depth16.Height);
        }


        // ------- Methods ------- //

        /// <summary>
        /// Please 'Subscribe', which is a Rx function.
        /// </summary>
        /// <param name="position">Starting frame index</param>
        /// <returns>Observable instance contains BgrXyzMat</returns>
        public IObservable<(BgrXyzMat Frame, int Position)> Start(int position)
        {
            if (position > -1 && position < FrameCount) _positionIndex = position;
            var observable = Observable.Range(0, FrameCount - position, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    Thread.Sleep(_interval);
                    GC.Collect();
                    return (Read().Frame, position++);
                })
                .Publish()
                .RefCount();
            return observable;
        }

        /// <summary>
        /// Get Color and Point Cloud frame
        /// </summary>
        /// <param name="position">Frame index</param>
        /// <returns></returns>
        public BgrXyzMat GetOneFrameSet(int position)
        {
            if (position > -1 && position < FrameCount) _positionIndex = position;
            return Read().Frame;
        }

        /// <summary>
        /// Close player.
        /// Must not forget 'Dispose' subscribing instance.
        /// </summary>
        public void Dispose()
        {
            _binReader?.Close();
            _binReader?.Dispose();
        }

        private (BgrXyzMat Frame, long Time) Read()
        {
            _binReader.BaseStream.Seek(_indexes[_positionIndex++], SeekOrigin.Begin);
            var time = _binReader.ReadInt64();
            var bgrDataSize = _binReader.ReadInt32();
            var bgrBytes = _binReader.ReadBytes(bgrDataSize);
            var xyzDataSize = _binReader.ReadInt32();
            var xyzBytes = _binReader.ReadBytes(xyzDataSize);
            return (new BgrXyzMat(bgrBytes, xyzBytes), time);
        }

    }
}
