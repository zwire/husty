using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Husty;
using Husty.OpenCvSharp;
using Path = System.IO.Path;

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
        private string _boardImageDir = "";
        private string _imageSourceDir = "";
        private string _videoSourceDir = "";
        private IDisposable _streamConnector;
        private OpenCvSharp.Size _size;
        private readonly Channel<Mat> _channel;
        private List<string> _imgFilesIn;
        private VideoCapture _cap;
        private List<(Point2f, Point3f)> _points;
        private IntrinsicCameraParameters _paramIn;
        private ExtrinsicCameraParameters _paramEx;
        private PerspectiveTransformer _trs;
        private readonly UserSetting<Setting> _setting;

        public record Setting(string BoardImageDir, string ImageSourceDir, string VIdeoSourceDir, int Width, int Height);

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
            _setting = new(new("C:", "C:", "C:", 640, 480));
            var val = _setting.Load();
            _boardImageDir = val.BoardImageDir;
            _imageSourceDir = val.ImageSourceDir;
            _videoSourceDir = val.VIdeoSourceDir;
            _size = new(val.Width, val.Height);
            SizeTx.Text = $"{_size.Width},{_size.Height}";
            _channel = new();
            Closed += (s, e) =>
            {
                _setting.Save(new(_boardImageDir, _imageSourceDir, _videoSourceDir, _size.Width, _size.Height));
            };
        }

        private void ImageStreamButton_Click(object sender, RoutedEventArgs e)
        {
            _isSampling = false;
            if (!_streamAlive)
            {
                if (int.TryParse(SizeTx.Text.Split(",")[0], out var w) &&
                    int.TryParse(SizeTx.Text.Split(",")[1], out var h))
                        _size = new(w, h);
                using var cofd = new CommonOpenFileDialog()
                {
                    InitialDirectory = _videoSourceDir,
                    IsFolderPicker = false
                };
                cofd.Filters.Add(new CommonFileDialogFilter("Video", "*.mp4;*.avi"));
                if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
                {
                    _cap = new(cofd.FileName);
                    _cap.Set(VideoCaptureProperties.FrameWidth, _size.Width);
                    _cap.Set(VideoCaptureProperties.FrameHeight, _size.Height);
                    _cap.Set(VideoCaptureProperties.Fps, 30);
                    _videoSourceDir = Path.GetDirectoryName(cofd.FileName);
                }
                else
                {
                    try
                    {
                        _cap = new(1);
                        _cap.Set(VideoCaptureProperties.FrameWidth, _size.Width);
                        _cap.Set(VideoCaptureProperties.FrameHeight, _size.Height);
                        _cap.Set(VideoCaptureProperties.Fps, 30);
                    }
                    catch { }
                }
                if (_cap.IsOpened())
                {
                    ImageStreamButton.Content = "Stream OFF";
                    ShutterButton.IsEnabled = true;
                    _streamAlive = true;
                    _streamConnector = Observable.Repeat(0, ThreadPoolScheduler.Instance)
                        .Subscribe(async _ =>
                        {
                            var frame = new Mat();
                            var suc = false;
                            suc = (bool)_cap?.Read(frame);
                            if (!frame.Empty())
                            {
                                // 田植え機用
                                Cv2.Flip(frame, frame, FlipMode.XY);
                                try
                                {
                                    if (_paramIn is not null && (_testInOn || _testExOn))
                                    {
                                        frame = frame?.Undistort(_paramIn.CameraMatrix, _paramIn.DistortionCoeffs);
                                    }
                                    await _channel.WriteAsync(frame.Clone());
                                    Dispatcher.Invoke(() =>
                                    {
                                        Image.Width = frame.Width;
                                        Image.Height = frame.Height;
                                        Image.Source = frame.ToBitmapSource();
                                    });
                                    frame.Dispose();
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
                if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
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

        private async void OpenImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_exMode)
            {
                if (int.TryParse(SizeTx.Text.Split(",")[0], out var w) &&
                    int.TryParse(SizeTx.Text.Split(",")[1], out var h))
                        _size = new(w, h);
                using var cofd = new CommonOpenFileDialog()
                {
                    InitialDirectory = _imageSourceDir,
                    IsFolderPicker = false
                };
                if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
                {
                    if (File.Exists(cofd.FileName))
                    {
                        _imageSourceDir = Path.GetDirectoryName(cofd.FileName);
                        var frame = Cv2.ImRead(cofd.FileName);
                        Cv2.Resize(frame, frame, _size);
                        if (File.Exists("intrinsic.json"))
                        {
                            _paramIn = IntrinsicCameraParameters.Load("intrinsic.json");
                            DebugMat2D(_paramIn.CameraMatrix, "Camera");
                            DebugMat2D(_paramIn.DistortionCoeffs, "Distortion");
                            frame = frame.Undistort(_paramIn.CameraMatrix, _paramIn.DistortionCoeffs);
                        }
                        await _channel.WriteAsync(frame.Clone());
                        Image.Width = frame.Width;
                        Image.Height = frame.Height;
                        Image.Source = frame.ToBitmapSource();
                        frame.Dispose();
                        _points = new();
                        Count2D.Content = "0";
                    }
                }
            }
        }

        private async void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_exMode)
            {
                var count = 0;
                while (File.Exists($"{count:d2}.png")) count++;
                var frame = (await _channel.ReadAsync()).Value;
                if (frame is not null && !frame.Empty())
                {
                    Cv2.ImWrite($"{count:d2}.png", frame);
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
                if (_imgFilesIn is null || _imgFilesIn.Count is 0) return;
                if (int.TryParse(SizeTx.Text.Split(",")[0], out var w) &&
                    int.TryParse(SizeTx.Text.Split(",")[1], out var h))
                        _size = new(w, h);
                var chess = new Chessboard(7, 10, 32.5f);
                _paramIn = IntrinsicCameraCalibrator.CalibrateWithChessboardImages(chess, _imgFilesIn);
                DebugMat2D(_paramIn.CameraMatrix, "Camera");
                DebugMat2D(_paramIn.DistortionCoeffs, "Distortion");
                _paramIn.Save("intrinsic.json");
                TestIntrinsicButton.IsEnabled = true;
                Task.Run(() =>
                {
                    foreach (var f in _imgFilesIn)
                    {
                        using var frame = Cv2.ImRead(f, ImreadModes.Grayscale);
                        Cv2.Resize(frame, frame, _size);
                        var corners = chess.FindCorners(frame);
                        chess.DrawCorners(frame, corners);
                        Dispatcher.Invoke(() =>
                        {
                            Image.Width = frame.Width;
                            Image.Height = frame.Height;
                            Image.Source = frame.ToBitmapSource();
                            Thread.Sleep(500);
                        });
                    }
                });
            }
            else
            {
                if (!File.Exists("intrinsic.json"))
                    throw new Exception("Please prepare intrinsic params file!");
                _paramIn = IntrinsicCameraParameters.Load("intrinsic.json");
                _paramEx = ExtrinsicCameraCalibrator.CalibrateWithGroundCoordinates(_points, _paramIn.WithoutDistCoeffs);
                DebugMat2D(_paramEx.RotationMatrix, "Rotation");
                DebugMat2D(_paramEx.TranslationVector, "Translation");
                _paramEx.Save("extrinsic.json");
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
            if (DoCalibrationButton is not null)
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
            if (DoCalibrationButton is not null)
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

        private async void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSampling && !_testExOn) return;
            var p = e.GetPosition(Image);
            X2D.Content = $"{(int)p.X}";
            Y2D.Content = $"{(int)p.Y}";
            X3D.IsEnabled = true;
            Y3D.IsEnabled = true;
            var frame = (await _channel.ReadAsync()).Value;
            if (frame is not null && !frame.Empty())
            {
                Cv2.Circle(frame, (int)p.X, (int)p.Y, 3, new(0, 0, 255), 3);
                Image.Width = frame.Width;
                Image.Height = frame.Height;
                Image.Source = frame.ToBitmapSource();
                await _channel.WriteAsync(frame);
            }
            if (_trs is not null && _testExOn)
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
            if (File.Exists("intrinsic.json"))
            {
                _paramIn = IntrinsicCameraParameters.Load("intrinsic.json");
                DebugMat2D(_paramIn.CameraMatrix, "Camera");
                DebugMat2D(_paramIn.DistortionCoeffs, "Distortion");
                TestIntrinsicButton.IsEnabled = true;
            }
        }

        private void LoadExtrinsicButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("intrinsic.json"))
            {
                _paramIn = IntrinsicCameraParameters.Load("intrinsic.json");
                DebugMat2D(_paramIn.CameraMatrix, "Camera");
                DebugMat2D(_paramIn.DistortionCoeffs, "Distortion");
                TestIntrinsicButton.IsEnabled = true;
            }
            if (File.Exists("extrinsic.json"))
            {
                _paramEx = ExtrinsicCameraParameters.Load("extrinsic.json");
                DebugMat2D(_paramEx.RotationMatrix, "Rotation");
                DebugMat2D(_paramEx.TranslationVector, "Translation");
                _trs = new(_paramIn.CameraMatrix, _paramEx);
                TestExtrinsicButton.IsEnabled = true;
            }
        }

        // for debug
        private void DebugMat2D(Mat mat, string name)
        {
            Debug.WriteLine(name);
            var r = mat.Rows;
            var c = mat.Cols;
            for (int y = 0; y < r; y++)
            {
                for (int x = 0; x < c; x++)
                {
                    Debug.WriteLine($"Row {y} : Col {x} : {mat.At<double>(y, x)}");
                }
            }
            Debug.WriteLine("\n");
        }

    }
}
