namespace Husty.OpenCvSharp
{
    public interface IVideoStream<TImage> : IImageStream<TImage>
    {

        public int FrameCount { get; }

        public int CurrentPosition { get; }

        public void Pause();

        public void Restart();

        public void Seek(int position);

    }
}
