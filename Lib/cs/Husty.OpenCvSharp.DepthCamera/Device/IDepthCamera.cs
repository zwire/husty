using OpenCvSharp;
using Reactive.Bindings;
using System;

namespace Husty.OpenCvSharp.DepthCamera
{

    public enum AlignBase { Color, Depth }

    /// <summary>
    /// Now supporting for Microsoft Azure Kinect & Intel RealSense D415 - 455, L515
    /// </summary>
    public interface IDepthCamera : IDisposable
    {

        public int Fps { get; }

        public Size FrameSize { get; }

        public ReactivePropertySlim<BgrXyzMat> ReactiveFrame { get; }

        /// <summary>
        /// Start streaming
        /// </summary>
        /// <returns></returns>
        public IObservable<BgrXyzMat> Connect();

        /// <summary>
        /// Stop streaming
        /// </summary>
        public void Dispose();

        /// <summary>
        /// Get current frame synchronously
        /// </summary>
        /// <returns></returns>
        public BgrXyzMat Read();

    }

}
