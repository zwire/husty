using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Husty.OpenCvSharp;
using Path = System.IO.Path;
using Point = OpenCvSharp.Point;

namespace Tools.CameraCalibration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {

        private bool _streamAlive;
        private bool _isSampling;
        private bool _exMode;
        private bool _testInOn;
        private bool _testExOn;
        private readonly object _locker = new();
        private string _boardImageDir = "";
        private string _imageSourceDir = "";
        private string _videoSourceDir = "";
        private IDisposable _streamConnector;
        private Mat _frame;
        private List<string> _imgFilesIn;
        private VideoCapture _cap;
        private List<(Point2f, Point3f)> _points;
        private IntrinsicCameraParameters _paramIn;
        private ExtrinsicCameraParameters _paramEx;
        private PerspectiveTransformer _trs;


        public MainWindow()
        {
            InitializeComponent();
            ShutterButton.IsEnabled = false;
            X3D.IsEnabled = false;
            Y3D.IsEnabled = false;
            DoCalibrationButton.IsEnabled = false;
            AppendButton.IsEnabled = false;
            TestIntrinsicButton.IsEnabled = false;
            TestExtrinsicButton.IsEnabled = false;
            OpenFolderButton.Visibility = Visibility.Visible;
            OpenImageButton.Visibility = Visibility.Hidden;
            ShutterButton.Visibility = Visibility.Visible;
            SamplingModeButton.Visibility = Visibility.Hidden;
            TestIntrinsicButton.Visibility = Visibility.Visible;
            TestExtrinsicButton.Visibility = Visibility.Hidden;
            AppendButton.Visibility = Visibility.Hidden;
            if (File.Exists("cache.txt"))
            {
                var lines = File.ReadAllText("cache.txt").Split("\n");
                if (lines.Length > 2)
                {
                    _boardImageDir = lines[0].TrimEnd();
                    _imageSourceDir = lines[1].TrimEnd();
                    _videoSourceDir = lines[2].TrimEnd();
                }
            }
            Closed += (s, e) =>
            {
                using var sw = new StreamWriter("cache.txt", false);
                sw.WriteLine(_boardImageDir);
                sw.WriteLine(_imageSourceDir);
                sw.WriteLine(_videoSourceDir);
            };
        }

        private void ImageStreamButton_Click(object sender, RoutedEventArgs e)
        {
            _isSampling = false;
            if (!_streamAlive)
            {
                using var cofd = new CommonOpenFileDialog()
                {
                    InitialDirectory = _videoSourceDir,
                    IsFolderPicker = false
                };
                cofd.Filters.Add(new CommonFileDialogFilter("Video", "*.mp4;*.avi"));
                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    _cap = new(cofd.FileName);
                    _videoSourceDir = Path.GetDirectoryName(cofd.FileName);
                }
                else
                {
                    try
                    {
                        _cap = new(0);
                    }
                    catch { }
                }
                if (_cap.IsOpened())
                {
                    ImageStreamButton.Content = "Stream OFF";
                    ShutterButton.IsEnabled = true;
                    _streamAlive = true;
                    _frame = new Mat();
                    _streamConnector = Observable.Range(0, int.MaxValue, ThreadPoolScheduler.Instance)
                        .Where(_ =>
                        {
                            var suc = false;
                            lock (_locker)
                            {
                                suc = (bool)_cap?.Read(_frame);
                            }
                            return suc;
                        })
                        .Subscribe(_ =>
                        {
                            lock (_locker)
                            {
                                try
                                {
                                    if (_paramIn != null && _testInOn)
                                    {
                                        _frame = _frame?.Undistort(_paramIn.CameraMatrix, _paramIn.DistortionCoeffs);
                                    }
                                    Dispatcher.Invoke(() =>
                                    {
                                        Image.Width = _frame.Width;
                                        Image.Height = _frame.Height;
                                        Image.Source = _frame.ToBitmapSource();
                                    });
                                    Thread.Sleep(1000 / (int)_cap.Fps);
                                }
                                catch { }
                            }
                        });
                }
            }
            else
            {
                ImageStreamButton.Content = "Stream ON";
                _streamConnector?.Dispose();
                _cap?.Dispose();
                _streamAlive = false;
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_exMode)
            {
                using var cofd = new CommonOpenFileDialog()
                {
                    InitialDirectory = _boardImageDir,
                    IsFolderPicker = true
                };
                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    _imgFilesIn = new(Directory.GetFiles(cofd.FileName, "*.png"));
                    _boardImageDir = Path.GetDirectoryName(cofd.FileName);
                    if (_imgFilesIn.Count > 0)
                    {
                        DoCalibrationButton.IsEnabled = true;
                    }
                }
            }
        }

        private void OpenImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_exMode)
            {
                using var cofd = new CommonOpenFileDialog()
                {
                    InitialDirectory = _imageSourceDir,
                    IsFolderPicker = false
                };
                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    if (File.Exists(cofd.FileName))
                    {
                        _imageSourceDir = Path.GetDirectoryName(cofd.FileName);
                        _frame = Cv2.ImRead(cofd.FileName);
                        Image.Width = _frame.Width;
                        Image.Height = _frame.Height;
                        Image.Source = _frame.ToBitmapSource();
                        _points = new();
                        Count2D.Content = "0";
                    }
                }
            }
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_exMode)
            {
                var count = 0;
                while (File.Exists($"{count:d2}.png")) count++;
                lock (_locker)
                {
                    if (_frame != null && !_frame.Empty())
                    {
                        Cv2.ImWrite($"{count:d2}.png", _frame);
                    }
                }
            }
        }

        private void SamplingModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_exMode)
            {
                if (!_isSampling)
                {
                    _isSampling = true;
                }
                else
                {
                    _isSampling = false;
                }
                _points = new();
                Count2D.Content = "0";
            }
        }

        private void DoCalibrationButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_exMode)
            {
                if (_imgFilesIn == null || _imgFilesIn.Count == 0) return;
                var chess = new Chessboard(7, 10, 32.5f);
                _paramIn = IntrinsicCameraCalibrator.CalibrateWithChessboardImages(chess, _imgFilesIn);
                _paramIn.Save("intrinsic.txt");
                TestIntrinsicButton.IsEnabled = true;
                Task.Run(() =>
                {
                    foreach (var f in _imgFilesIn)
                    {
                        _frame = Cv2.ImRead(f, ImreadModes.Grayscale);
                        var corners = chess.FindCorners(_frame);
                        chess.DrawCorners(_frame, corners);
                        Dispatcher.Invoke(() =>
                        {
                            Image.Width = _frame.Width;
                            Image.Height = _frame.Height;
                            Image.Source = _frame.ToBitmapSource();
                            Thread.Sleep(500);
                        });
                    }
                });
            }
            else
            {
                if (!File.Exists("intrinsic.txt"))
                    throw new Exception("Please prepare intrinsic params file!");
                _paramIn = IntrinsicCameraParameters.Load("intrinsic.txt");
                _paramEx = ExtrinsicCameraCalibrator.CalibrateWithGroundCoordinates(_points, _paramIn);
                _paramEx.Save("extrinsic.txt");
                _trs = new(_paramIn.CameraMatrix, _paramEx);
                TestIntrinsicButton.IsEnabled = true;
                TestExtrinsicButton.IsEnabled = true;
                _points = new();
                Count2D.Content = "0";
            }
        }

        private void TestIntrinsicButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_exMode)
            {
                _testInOn = !_testInOn;
            }
        }

        private void TestExtrinsicButton_Click(object sender, RoutedEventArgs e)
        {
            if (_exMode)
            {
                _testExOn = !_testExOn;
                _points = new();
                Count2D.Content = "0";
            }
        }

        private void AppendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_exMode)
            {
                if (
                int.TryParse(X2D.Content.ToString(), out var x2) &&
                int.TryParse(Y2D.Content.ToString(), out var y2) &&
                float.TryParse(X3D.Text, out var x3) &&
                float.TryParse(Y3D.Text, out var y3))
                {
                    _points.Add((new(x2, y2), new(x3, y3, 0)));
                    Count2D.Content = $"{_points.Count}";
                    X2D.Content = "";
                    Y2D.Content = "";
                    X3D.Text = "";
                    Y3D.Text = "";
                    X3D.IsEnabled = false;
                    Y3D.IsEnabled = false;
                }
                if (_points.Count > 3)
                {
                    DoCalibrationButton.IsEnabled = true;
                }
            }
        }

        private void RadioIn_Checked(object sender, RoutedEventArgs e)
        {
            _exMode = false;
            _points = new();
            if (DoCalibrationButton != null)
            {
                DoCalibrationButton.IsEnabled = false;
                OpenFolderButton.Visibility = Visibility.Visible;
                OpenImageButton.Visibility = Visibility.Hidden;
                ShutterButton.Visibility = Visibility.Visible;
                SamplingModeButton.Visibility = Visibility.Hidden;
                TestIntrinsicButton.Visibility = Visibility.Visible;
                TestExtrinsicButton.Visibility = Visibility.Hidden;
                AppendButton.Visibility = Visibility.Hidden;
            }
        }

        private void RadioEx_Checked(object sender, RoutedEventArgs e)
        {
            _exMode = true;
            _points = new();
            Count2D.Content = "0";
            if (DoCalibrationButton != null)
            {
                DoCalibrationButton.IsEnabled = false;
                AppendButton.IsEnabled = false;
                OpenFolderButton.Visibility = Visibility.Hidden;
                OpenImageButton.Visibility = Visibility.Visible;
                ShutterButton.Visibility = Visibility.Hidden;
                SamplingModeButton.Visibility = Visibility.Visible;
                TestIntrinsicButton.Visibility = Visibility.Hidden;
                TestExtrinsicButton.Visibility = Visibility.Visible;
                AppendButton.Visibility = Visibility.Visible;
            }
        }

        private void X3D_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_exMode)
            {
                if (_isSampling && float.TryParse(X3D.Text, out var x) && float.TryParse(Y3D.Text, out var y))
                {
                    AppendButton.IsEnabled = true;
                }
                else
                {
                    AppendButton.IsEnabled = false;
                }
            }
        }

        private void Y3D_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_exMode)
            {
                if (_isSampling && float.TryParse(X3D.Text, out var x) && float.TryParse(Y3D.Text, out var y))
                {
                    AppendButton.IsEnabled = true;
                }
                else
                {
                    AppendButton.IsEnabled = false;
                }
            }
        }

        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSampling && !_testExOn) return;
            var p = e.GetPosition(Image);
            X2D.Content = $"{(int)p.X}";
            Y2D.Content = $"{(int)p.Y}";
            X3D.IsEnabled = true;
            Y3D.IsEnabled = true;
            lock (_locker)
            {
                if (_frame != null && !_frame.Empty())
                {
                    Cv2.Circle(_frame, (int)p.X, (int)p.Y, 3, new(0, 0, 255), 3);
                    Image.Width = _frame.Width;
                    Image.Height = _frame.Height;
                    Image.Source = _frame.ToBitmapSource();
                }
            }
            if (_trs != null && _testExOn)
            {
                X2D.Content = $"{(int)p.X}";
                Y2D.Content = $"{(int)p.Y}";
                var p3 = _trs.ConvertToWorldCoordinate(new((int)p.X, (int)p.Y));
                X3D.Text = $"{p3.X:f1}";
                Y3D.Text = $"{p3.Y:f1}";
            }
        }

        private void LoadIntrinsicButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("intrinsic.txt"))
            {
                _paramIn = IntrinsicCameraParameters.Load("intrinsic.txt");
                TestIntrinsicButton.IsEnabled = true;
            }
        }

        private void LoadExtrinsicButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("intrinsic.txt"))
            {
                _paramIn = IntrinsicCameraParameters.Load("intrinsic.txt");
                TestIntrinsicButton.IsEnabled = true;
            }
            if (File.Exists("extrinsic.txt"))
            {
                _paramEx = ExtrinsicCameraParameters.Load("extrinsic.txt");
                _trs = new(_paramIn.CameraMatrix, _paramEx);
                TestExtrinsicButton.IsEnabled = true;
            }
        }

    }
}
