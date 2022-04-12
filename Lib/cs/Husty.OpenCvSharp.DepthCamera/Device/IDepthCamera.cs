using System;
using OpenCvSharp;
using Husty.OpenCvSharp.ImageStream;

namespace Husty.OpenCvSharp.DepthCamera.Device
{
    public interface IDepthCamera : IImageStream<BgrXyzMat>
    {

        public Mat ReadBgr();

        public Mat ReadXyz();

        public IObservable<Mat> GetBgrStream();

        public IObservable<Mat> GetXyzStream();

    }
}
