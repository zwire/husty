using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Husty.OpenCvSharp;

namespace Test.CameraCalibration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private int _count;
        private bool _cameraOn;
        private bool _isSampling;
        private VideoCapture _cap;
        private IDisposable _cameraConnector;
        private Mat _frame;
        private List<(Point2f, Point3f)> _points;
        private IntrinsicCameraParameters _paramIn;
        private ExtrinsicCameraParameters _paramEx;
        private Transformer _trs;

        public MainWindow()
        {
            InitializeComponent();
            ApplyButton.IsEnabled = false;
            ShutterButton.Content = "SaveLoadIntrinstic";
        }

        private void OpenCameraButton_Click(object sender, RoutedEventArgs e)
        {
            _isSampling = false;
            if (!_cameraOn)
            {
                _cap = new(0);
                if (_cap.IsOpened())
                {
                    OpenCameraButton.Content = "CloseCamera";
                    ShutterButton.IsEnabled = true;
                    ShutterButton.Content = "Shutter";
                    _cameraOn = true;
                    _frame = new Mat();
                    _cameraConnector = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                        .Where(_ => (bool)_cap?.Read(_frame))
                        .Subscribe(_ =>
                        {
                            if (_paramIn != null)
                            {
                                _frame = _frame.Undistort(_paramIn.CameraMatrix, _paramIn.DistortionCoeffs);
                            }
                            Dispatcher.Invoke(() =>
                            {
                                Image.Width = _frame.Width;
                                Image.Height = _frame.Height;
                                Image.Source = _frame.ToBitmapSource();
                            });
                        });
                }
            }
            else
            {
                OpenCameraButton.Content = "OpenCamera";
                ShutterButton.Content = "SaveLoadIntrinstic";
                _cameraConnector?.Dispose();
                _cap?.Dispose();
                _cameraOn = false;
            }
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cameraOn)
            {
                if (_frame != null && !_frame.Empty())
                {
                    while (File.Exists($"{_count:d2}.png")) _count++;
                    Cv2.ImWrite($"{_count:d2}.png", _frame);
                }
            }
            else
            {
                var dir = Directory.GetCurrentDirectory();
                var files = Directory.GetFiles(dir, "*.png");
                var chess = new Chessboard(7, 10, 32.5f);
                _paramIn = IntrinsticCameraCalibrator.CalibrateWithChessboardImages(chess, files);
                _paramIn.Save("intrinstic.txt");
                foreach (var f in files)
                {
                    var img = Cv2.ImRead(f, ImreadModes.Grayscale);
                    var corners = chess.FindCorners(img);
                    chess.DrawCorners(img, corners);
                    Cv2.ImShow(" ", img);
                    Cv2.WaitKey(500);
                }
                Cv2.DestroyAllWindows();
            }
        }

        private void SamplingSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSampling)
            {
                _points = new();
                _isSampling = true;
                ApplyButton.IsEnabled = true;
                SamplingSaveButton.Content = "Save";
            }
            else
            {
                _paramIn = IntrinsicCameraParameters.Load("intrinstic.txt");
                _paramEx = ExtrinsticCameraCalibrator.CalibrateWithGroundCoordinates(_points, _paramIn);
                _paramEx.Save("extrinstic.txt");
                _trs = new(_paramIn, _paramEx);
                _isSampling = false;
                ApplyButton.IsEnabled = false;
                SamplingSaveButton.Content = "Sampling";
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(X2d.Content.ToString(), out var x2) &&
                int.TryParse(Y2d.Content.ToString(), out var y2) &&
                int.TryParse(X3d.Text, out var x3) &&
                int.TryParse(Y3d.Text, out var y3))
            {
                _points.Add((new(x2, y2), new (x3, y3, 0)));
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(X2d.Content.ToString(), out var x2) &&
                int.TryParse(Y2d.Content.ToString(), out var y2))
            {
                _paramIn = IntrinsicCameraParameters.Load("intrinstic.txt");
                _paramEx = ExtrinsicCameraParameters.Load("extrinstic.txt");
                _trs = new(_paramIn, _paramEx);
                var p = _trs.ConvertToWorldCoordinate(new(x2, y2));
                X3d.Text = $"{(int)p.X}";
                Y3d.Text = $"{(int)p.Y}";
            }
        }

        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(Image);
            X2d.Content = $"{(int)p.X}";
            Y2d.Content = $"{(int)p.Y}";
            if (_frame != null && !_frame.Empty())
            {
                Cv2.Circle(_frame, (int)p.X, (int)p.Y, 3, new(0, 0, 255), 3);
                Image.Source = _frame.ToBitmapSource();
            }
        }
    }
}
