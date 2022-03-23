namespace Husty.OpenCvSharp.ImageStream
{
    public interface IVideoStream<TImage> : IImageStream<TImage>
    {

        public int FrameCount { get; }

        public int CurrentPosition { get; }

        public void Seek(int position);

    }
}
