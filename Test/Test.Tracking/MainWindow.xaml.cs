using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Husty.OpenCvSharp;

namespace Test.Tracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private IDisposable _connector;
        private readonly YoloDetector _detector;

        public MainWindow()
        {
            InitializeComponent();
            _detector = new(
                "..\\..\\..\\..\\Test.YoloModel-tiny\\yolov4-tiny.cfg",
                "..\\..\\..\\..\\Test.YoloModel-tiny\\coco.names",
                "..\\..\\..\\..\\Test.YoloModel-tiny\\yolov4-tiny.weights",
                new OpenCvSharp.Size(640, 480)
                );
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            _connector?.Dispose();
            var op = new OpenFileDialog { Filter = "Video(*.mp4, *.avi)|*.mp4;*.avi" };
            if (op.ShowDialog() == true)
            {
                var tracker = new MultiTracker(OutputType.Predict, 0.2f, 7, 3, 0.1, 5);
                _connector = PlayVideo(op.FileName)
                    .Subscribe(frame =>
                    {
                        var w = 512;
                        var h = 288;
                        Cv2.Resize(frame, frame, new OpenCvSharp.Size(w, h));
                        var results = _detector.Run(frame).Select(r => (r.Label, r.ScaledCenter(w, h), new OpenCvSharp.Size(r.Box.Width, r.Box.Height)));
                        tracker.Update(ref frame, results, out var _);
                        Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                    });
            }
        }

        private void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            _connector?.Dispose();
            var tracker = new MultiTracker(OutputType.Predict, 0.2f, 7, 3, 0.1, 5);
            _connector = ConnectCamera(0)
                    .Subscribe(frame =>
                    {
                        var w = 512;
                        var h = 288;
                        Cv2.Resize(frame, frame, new OpenCvSharp.Size(w, h));
                        var results = _detector.Run(frame).Select(r => (r.Label, r.ScaledCenter(w, h), new OpenCvSharp.Size(r.Box.Width, r.Box.Height))).ToList();
                        tracker.Update(ref frame, results, out var _);
                        Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                    });
        }

        private IObservable<Mat> PlayVideo(string path)
        {
            var frame = new Mat();
            var cap = new VideoCapture(path);
            var observable = Observable.Range(0, cap.FrameCount, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    cap.Read(frame);
                    return frame;
                })
                .Where(frame => !frame.Empty())
                .Publish()
                .RefCount();
            return observable;
        }

        private IObservable<Mat> ConnectCamera(int index = 0)
        {
            var frame = new Mat();
            var cap = new VideoCapture(index);
            var observable = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                .Select(i =>
                {
                    cap.Read(frame);
                    return frame;
                })
                .Where(frame => !frame.Empty())
                .Publish()
                .RefCount();
            return observable;
        }

    }
}
