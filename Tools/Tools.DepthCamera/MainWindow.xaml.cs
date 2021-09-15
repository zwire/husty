using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reactive.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp.WpfExtensions;
using Husty.OpenCvSharp.DepthCamera;
using Scalar = OpenCvSharp.Scalar;
using Point = OpenCvSharp.Point;

namespace Tools.DepthCamera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {

        private IDepthCamera _camera;
        private IDisposable _cameraConnector;
        private IDisposable _videoConnector;
        private bool _isConnected;
        private string _saveDir = "";
        private string _videoDir = "";
        private VideoPlayer _player;
        private BgrXyzMat _framesPool;
        private readonly object _lockobj = new();
        //private Scalar red = new Scalar(0, 0, 255);
        //private Scalar red = new Scalar(0, 0, 255);
        //private Point left = new Point(130, 144);
        //private Point right = new Point(190, 144);
        //private Point top = new Point(160, 114);
        //private Point bottom = new Point(160, 174);

        private readonly StreamWriter _log;
        private bool _isKinect;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _isConnected = false;
            StartPauseButton.Content = "Open";
            ShutterButton.IsEnabled = false;
            if (File.Exists("cache.txt"))
            {
                var lines = File.ReadAllText("cache.txt").Split("\n");
                if (lines.Length > 1)
                {
                    _saveDir = lines[0].TrimEnd();
                    _videoDir = lines[1].TrimEnd();
                }
            }
            var t = DateTimeOffset.Now;
            var name = $"log_{t.Year}{t.Month:d2}{t.Day:d2}_{t.Hour:d2}{t.Minute:d2}{t.Second:d2}.csv";
            _log = new($"{_saveDir}\\{name}");
            _log.WriteLine("device,u,v,x,y,z,tx,ty,tz");
            if (!Directory.Exists(_saveDir))
                _saveDir = "C:";
            if (!Directory.Exists(_videoDir))
                _videoDir = "C:";
            SaveDir.Content = _saveDir;
            Closed += (sender, args) =>
            {
                GC.Collect();
                _log.Close();
                _log.Dispose();
                using var sw = new StreamWriter("cache.txt", false);
                sw.WriteLine(_saveDir);
                sw.WriteLine(_videoDir);
                _videoConnector?.Dispose();
                _cameraConnector?.Dispose();
                _cameraConnector = null;
                _camera?.Disconnect();
                _camera = null;
            };
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            _videoConnector?.Dispose();
            _videoConnector = null;
            _player?.Dispose();
            _player = null;
            if (!_isConnected)
            {
                StartPauseButton.Content = "Close";
                StartPauseButton.Background = Brushes.Red;
                RecButton.IsEnabled = false;
                PlayButton.IsEnabled = false;
                PlayPauseButton.IsEnabled = false;
                PlaySlider.IsEnabled = false;
                PlayPauseButton.Visibility = Visibility.Hidden;
                PlaySlider.Visibility = Visibility.Hidden;
                _isConnected = AttemptConnection();
                if (!_isConnected) new Exception("Couldn't connect device!");

                ShutterButton.IsEnabled = true;
                _cameraConnector = _camera.Connect()
                    .Subscribe(imgs =>
                    {
                        //var l = imgs.GetPointInfo(left);
                        //var r = imgs.GetPointInfo(right);
                        //var t = imgs.GetPointInfo(top);
                        //var b = imgs.GetPointInfo(bottom);
                        lock (_lockobj)
                            _framesPool = imgs;
                        var d8 = imgs.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            //LR.Value = $"↔ {r.Z - l.Z} mm";
                            //TB.Value = $"↕ {b.Z - t.Z} mm";
                            ColorFrame.Source = imgs.BGR.ToBitmapSource();
                            DepthFrame.Source = d8.ToBitmapSource();
                        });
                    });
            }
            else
            {
                StartPauseButton.Content = "Open";
                StartPauseButton.Background = Brushes.DarkGray;
                RecButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                ShutterButton.IsEnabled = false;
                _isConnected = false;
                _cameraConnector?.Dispose();
                _cameraConnector = null;
                _camera?.Disconnect();
                _camera = null;
            }
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cameraConnector != null)
            {
                _camera.Connect()
                    .TakeWhile(imgs =>
                    {
                        if (imgs.Empty()) return true;
                        ImageIO.SaveAsZip(_saveDir, "", imgs);
                        return false;
                    })
                    .Subscribe();
            }
            if (_player != null)
            {
                var frame = _player.GetOneFrameSet((int)PlaySlider.Value);
                ImageIO.SaveAsZip(_saveDir, "", frame);
            }
        }

        private void RecButton_Click(object sender, RoutedEventArgs e)
        {
            _videoConnector?.Dispose();
            _videoConnector = null;
            _player?.Dispose();
            _player = null;
            if (!_isConnected)
            {
                RecButton.Content = "Stop";
                RecButton.Background = Brushes.Red;
                StartPauseButton.IsEnabled = false;
                PlayButton.IsEnabled = false;
                PlayPauseButton.IsEnabled = false;
                PlaySlider.IsEnabled = false;
                PlayPauseButton.Visibility = Visibility.Hidden;
                PlaySlider.Visibility = Visibility.Hidden;
                _isConnected = AttemptConnection();
                if (!_isConnected) new Exception("Couldn't connect device!");
                //var fileNumber = 0;
                //while (File.Exists($"{SaveDir.Value}\\Movie_{fileNumber:D4}.yms")) fileNumber++;

                var time = DateTimeOffset.Now;
                var filePath = $"{_saveDir}\\Movie_{time.Year}{time.Month:d2}{time.Day:d2}{time.Hour:d2}{time.Minute:d2}{time.Second:d2}{time.Millisecond:d2}.yms";
                var writer = new VideoRecorder(filePath);
                _cameraConnector = _camera.Connect()
                    .Finally(() => writer?.Dispose())
                    .Subscribe(imgs =>
                    {
                        writer?.WriteFrames(imgs);
                        //var l = imgs.GetPointInfo(left);
                        //var r = imgs.GetPointInfo(right);
                        //var t = imgs.GetPointInfo(top);
                        //var b = imgs.GetPointInfo(bottom);
                        lock (_lockobj)
                            _framesPool = imgs;
                        using var d8 = imgs.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            //LR.Value = $"↔ {r.Z - l.Z} mm";
                            //TB.Value = $"↕ {b.Z - t.Z} mm";
                            ColorFrame.Source = imgs.BGR.ToBitmapSource();
                            DepthFrame.Source = d8.ToBitmapSource();
                        });
                    });
            }
            else
            {
                RecButton.Content = "Rec";
                RecButton.Background = Brushes.DarkGray;
                StartPauseButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                _isConnected = false;
                _cameraConnector?.Dispose();
                _cameraConnector = null;
                _camera?.Disconnect();
                _cameraConnector = null;
            }
        }

        private void SelectDirButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = _saveDir,
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok) _saveDir = cofd.FileName;
            SaveDir.Content = _saveDir;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            _videoConnector?.Dispose();
            _videoConnector = null;
            _player?.Dispose();
            _player = null;
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "動画を選択してください",
                InitialDirectory = _videoDir,
                IsFolderPicker = false,
            };
            cofd.Filters.Add(new("YMSファイル", "*.yms"));
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _videoDir = Path.GetDirectoryName(cofd.FileName);
                _isConnected = true;
                PlayPauseButton.Content = "| |";
                PlayPauseButton.IsEnabled = true;
                PlayPauseButton.Visibility = Visibility.Visible;
                PlaySlider.Visibility = Visibility.Visible;
                _player = new VideoPlayer(cofd.FileName);
                PlaySlider.Maximum = _player.FrameCount;
                _videoConnector = _player.Start(0)
                    .Subscribe(ww =>
                    {
                        lock (_lockobj)
                            _framesPool = ww.Frames;
                        using var d8 = ww.Frames.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            ColorFrame.Source = ww.Frames.BGR.ToBitmapSource();
                            DepthFrame.Source = d8.ToBitmapSource();
                            PlaySlider.Value = ww.Position;
                        });
                    });
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isConnected)
            {
                _videoConnector.Dispose();
                _videoConnector = null;
                _isConnected = false;
                PlayPauseButton.Content = "▶";
                PlaySlider.IsEnabled = true;
                ShutterButton.IsEnabled = true;
            }
            else
            {
                _isConnected = true;
                PlaySlider.IsEnabled = false;
                PlayPauseButton.Content = "| |";
                ShutterButton.IsEnabled = false;
                _videoConnector = _player.Start((int)PlaySlider.Value)
                    .Subscribe(ww =>
                    {
                        lock (_lockobj)
                            _framesPool = ww.Frames;
                        using var d8 = ww.Frames.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            ColorFrame.Source = ww.Frames.BGR.ToBitmapSource();
                            DepthFrame.Source = d8.ToBitmapSource();
                            PlaySlider.Value = ww.Position;
                        });
                    });
            }
        }

        private void PlaySlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var imgs = _player.GetOneFrameSet((int)PlaySlider.Value);
            ColorFrame.Source = imgs.BGR.ToBitmapSource();
            DepthFrame.Source = imgs.Depth8(300, 5000).ToBitmapSource();
        }

        private void ColorFrame_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var p = e.GetPosition(ColorFrame);
            ImageClicked((int)p.X, (int)p.Y);
        }

        private void DepthFrame_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var p = e.GetPosition(DepthFrame);
            ImageClicked((int)p.X, (int)p.Y);
        }

        private void ImageClicked(int x, int y)
        {
            Vector3 info;
            if (_framesPool != null)
            {
                lock (_lockobj)
                {
                    info = _framesPool.GetPointInfo(new(x, y)).Vector3;
                }
                UV.Content = $"UV ({x}, {y})";
                XYZ1.Content = $"XYZ ({info.X}, {info.Y}, {info.Z})";

                var xx = info.Z;
                var yy = (short)-info.X;
                var zz = (short)-info.Y;
                if (xx == 0)
                {
                    XYZ2.Content = $"Transform (--, --, --)";
                    return;
                };

                if (_isKinect)
                {
                    // Kinect
                    var xxx = (short)(0.80567 * xx + 0.020847 * yy + 0.462340 * zz + 313.58);
                    var yyy = (short)(-0.04243 * xx + 1.008600 * yy + 0.041622 * zz + 1.3942);
                    var zzz = (short)(-0.36180 * xx - 0.050545 * yy + 0.882870 * zz - 110.03);
                    XYZ2.Content = $"Transform ({xxx}, {yyy}, {zzz})";
                    _log.WriteLine($"kinect,{x},{y},{info.X},{info.Y},{info.Z},{xxx},{yyy},{zzz}");
                }
                else
                {
                    // Realsense
                    var xxx = (short)(0.7355 * xx - 0.0130 * yy + 0.7006 * zz + 196.1);
                    var yyy = (short)(0.0099 * xx + 1.0359 * yy + 0.0012 * zz - 12.30) + 55;
                    var zzz = (short)(-0.700 * xx - 0.0052 * yy + 0.7701 * zz - 51.10);
                    XYZ2.Content = $"Transform ({xxx}, {yyy}, {zzz})";
                    _log.WriteLine($"realsense,{x},{y},{info.X},{info.Y},{info.Z},{xxx},{yyy},{zzz}");
                }
            }
        }

        private bool AttemptConnection()
        {
            try
            {
                _camera = new Kinect(
                    new DeviceConfiguration
                    {
                        ColorFormat = ImageFormat.ColorBGRA32,
                        ColorResolution = ColorResolution.R720p,
                        DepthMode = DepthMode.WFOV_2x2Binned,
                        SynchronizedImagesOnly = true,
                        CameraFPS = FPS.FPS15
                    });
                _isKinect = true;
            }
            catch
            {
                try
                {
                    _camera = new Realsense(new(640, 360), 30); // D
                    //_camera = new Realsense(new(640, 480)); // L
                    _isKinect = false;
                }
                catch
                {
                    throw new Exception("Couldn't connect device!");
                }
            }
            return true;
        }


    }
}
