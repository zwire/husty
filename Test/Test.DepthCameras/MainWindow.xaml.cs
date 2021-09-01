using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reactive.Linq;
using Reactive.Bindings;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp.WpfExtensions;
using Husty.OpenCvSharp.DepthCamera;
using Scalar = OpenCvSharp.Scalar;
using Point = OpenCvSharp.Point;

namespace Test.DepthCameras
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private IDepthCamera _camera;
        private IDisposable _cameraConnector;
        private IDisposable _videoConnector;
        private bool _isConnected;
        private string _videoDir = "";
        private VideoPlayer _player;
        //private Scalar red = new Scalar(0, 0, 255);
        //private Point left = new Point(130, 144);
        //private Point right = new Point(190, 144);
        //private Point top = new Point(160, 114);
        //private Point bottom = new Point(160, 174);

        public ReactiveProperty<string> StartButtonFace { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> RecButtonFace { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> SaveDir { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> LR { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> TB { private set; get; } = new ReactiveProperty<string>();
        public ReactiveProperty<BitmapSource> ColorFrame { private set; get; } = new ReactiveProperty<BitmapSource>();
        public ReactiveProperty<BitmapSource> DepthFrame { private set; get; } = new ReactiveProperty<BitmapSource>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _isConnected = false;
            StartButtonFace.Value = "Open";
            ShutterButton.IsEnabled = false;
            if (File.Exists("cache.txt"))
            {
                var lines = File.ReadAllText("cache.txt").Split("\n");
                if (lines.Length > 1)
                {
                    SaveDir.Value = lines[0].TrimEnd();
                    _videoDir = lines[1].TrimEnd();
                }
            }
            if (!Directory.Exists(SaveDir.Value))
                SaveDir.Value = "C:";
            if (!Directory.Exists(_videoDir))
                _videoDir = "C:";
            Closed += (sender, args) =>
            {
                GC.Collect();
                using var sw = new StreamWriter("cache.txt", false);
                sw.WriteLine(SaveDir.Value);
                sw.WriteLine(_videoDir);
                _videoConnector?.Dispose();
                _cameraConnector?.Dispose();
                _camera?.Disconnect();
            };
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            _videoConnector?.Dispose();
            _player?.Dispose();
            if (!_isConnected)
            {
                StartButtonFace.Value = "Close";
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
                        var d8 = imgs.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            //LR.Value = $"↔ {r.Z - l.Z} mm";
                            //TB.Value = $"↕ {b.Z - t.Z} mm";
                            ColorFrame.Value = imgs.BGR.ToBitmapSource();
                            DepthFrame.Value = d8.ToBitmapSource();
                        });
                    });
            }
            else
            {
                StartButtonFace.Value = "Open";
                RecButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                ShutterButton.IsEnabled = false;
                _isConnected = false;
                _cameraConnector?.Dispose();
                _camera?.Disconnect();
                _cameraConnector = null;
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
                        ImageIO.SaveAsZip(SaveDir.Value, "", imgs);
                        return false;
                    })
                    .Subscribe();
            }
            if (_player != null)
            {
                var frame = _player.GetOneFrameSet((int)PlaySlider.Value);
                ImageIO.SaveAsZip(SaveDir.Value, "", frame);
            }
        }

        private void RecButton_Click(object sender, RoutedEventArgs e)
        {
            _videoConnector?.Dispose();
            _player?.Dispose();
            if (!_isConnected)
            {
                RecButtonFace.Value = "Stop";
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
                var filePath = $"{SaveDir.Value}\\Movie_{time.Year}{time.Month:d2}{time.Day:d2}{time.Hour:d2}{time.Minute:d2}{time.Second:d2}{time.Millisecond:d2}.yms";
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
                        using var d8 = imgs.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            //LR.Value = $"↔ {r.Z - l.Z} mm";
                            //TB.Value = $"↕ {b.Z - t.Z} mm";
                            ColorFrame.Value = imgs.BGR.ToBitmapSource();
                            DepthFrame.Value = d8.ToBitmapSource();
                        });
                    });
            }
            else
            {
                RecButtonFace.Value = "Rec";
                StartPauseButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                _isConnected = false;
                _cameraConnector?.Dispose();
                _camera?.Disconnect();
                _cameraConnector = null;
            }
        }

        private void SelectDirButton_Click(object sender, RoutedEventArgs e)
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                InitialDirectory = SaveDir.Value,
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok) SaveDir.Value = cofd.FileName;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            _videoConnector?.Dispose();
            _player?.Dispose();
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
                        using var d8 = ww.Frames.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            ColorFrame.Value = ww.Frames.BGR.ToBitmapSource();
                            DepthFrame.Value = d8.ToBitmapSource();
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
                        using var d8 = ww.Frames.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            ColorFrame.Value = ww.Frames.BGR.ToBitmapSource();
                            DepthFrame.Value = d8.ToBitmapSource();
                            PlaySlider.Value = ww.Position;
                        });
                    });
            }
        }

        private void PlaySlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var imgs = _player.GetOneFrameSet((int)PlaySlider.Value);
            ColorFrame.Value = imgs.BGR.ToBitmapSource();
            DepthFrame.Value = imgs.Depth8(300, 5000).ToBitmapSource();
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
                        DepthMode = DepthMode.NFOV_2x2Binned,
                        SynchronizedImagesOnly = true,
                        CameraFPS = FPS.FPS15
                    }, Kinect.Matching.On);
            }
            catch
            {
                try
                {
                    _camera = new Realsense(640, 360, 640, 360); // D
                    //_camera = new Realsense(640, 480); // L
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
