using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Husty.OpenCvSharp;

namespace Test.Yolo_tiny
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        VideoCapture cap;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var detector = new YoloDetector(
                "..\\..\\..\\..\\Test.YoloModel-tiny\\yolov4-tiny.cfg",
                "..\\..\\..\\..\\Test.YoloModel-tiny\\coco.names", 
                "..\\..\\..\\..\\Test.YoloModel-tiny\\yolov4-tiny.weights", 
                new OpenCvSharp.Size(640, 480)
            );
            var op = new OpenFileDialog { Filter = "Image or Video(*.png, *.jpg, *.mp4, *.avi)|*.png;*.jpg;*mp4;*.avi" };
            if (op.ShowDialog() == true)
            {
                cap?.Dispose();
                var file = op.FileName;
                if(Path.GetExtension(file) == ".mp4" || Path.GetExtension(file) == ".avi")
                {
                    cap = new VideoCapture(file);
                    Task.Run(() =>
                    {
                        var frame = new Mat();
                        while (cap.Read(frame))
                        {
                            detector
                                .Run(frame)
                                .Where(r => r.Label == "person")
                                .Where(r => r.Confidence > 0.8)
                                .ToList()
                                .ForEach(r => r.DrawRect(frame, new Scalar(0, 0, 255)));
                            Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                        }
                        
                    });
                }
                else
                {
                    var img = Cv2.ImRead(file);
                    detector
                        .Run(img)
                        .ToList()
                        .ForEach(r => r.DrawRect(img, new Scalar(0, 0, 255)));
                    Image.Source = img.ToBitmapSource();
                }
            }
            else
            {
                cap?.Dispose();
                cap = new VideoCapture(0);
                Task.Run(() =>
                {
                    var frame = new Mat();
                    while (cap.Read(frame))
                    {
                        detector
                            .Run(frame)
                            .Where(r => r.Label == "person")
                            .Where(r => r.Confidence > 0.8)
                            .ToList()
                            .ForEach(r => r.DrawRect(frame, new Scalar(0, 0, 255)));
                        Dispatcher.Invoke(() => Image.Source = frame.ToBitmapSource());
                    }

                });
            }
        }
    }
}
