using System.Threading.Channels;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Reactive.Linq;
using Reactive.Bindings;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Azure.Kinect.Sensor;
using OpenCvSharp.WpfExtensions;
using Husty;
using Husty.OpenCvSharp.ImageStream;
using Husty.OpenCvSharp.DepthCamera;

namespace DepthCamera;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
{

    private IImageStream<BgrXyzMat> _camera;
    private BgrXyzPlayer _player;
    private bool _isConnected;
    private string _saveDir = "";
    private string _videoDir = "";
    private readonly Channel<BgrXyzMat> _channel;

    private record Preset(string SaveDir, string VideoDir);

    private ReadOnlyReactivePropertySlim<BgrXyzMat> ReactiveFrame { set; get; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _isConnected = false;
        StartPauseButton.Content = "Open";
        ShutterButton.IsEnabled = false;
        var settings = new Preset<Preset>(new("C:", "C:"));
        var preset = settings.Load();
        _saveDir = preset.SaveDir;
        _videoDir = preset.VideoDir;
        var t = DateTimeOffset.Now;
        SaveDir.Content = _saveDir;
        _channel = Channel.CreateBounded<BgrXyzMat>(
            new BoundedChannelOptions(1)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = true,
            });
        Closed += (sender, args) =>
        {
            GC.Collect();
            settings.Save(new(_saveDir, _videoDir));
            _camera?.Dispose();
            _camera = null;
            _player?.Dispose();
            _player = null;
        };
    }

    private void StartPauseButton_Click(object sender, RoutedEventArgs e)
    {
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
            ReactiveFrame = _camera.GetStream().ToReadOnlyReactivePropertySlim();
            ReactiveFrame
                .Where(frame => frame is not null)
                .Subscribe(frame =>
                {
                    try
                    {
                        _channel.Writer.TryWrite(frame);
                        using var d8 = frame.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            ColorFrame.Source = frame.BGR.ToBitmapSource();
                            DepthFrame.Source = d8.ToBitmapSource();
                        });
                    }
                    catch { }
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
            ReactiveFrame?.Dispose();
            _camera?.Dispose();
            _camera = null;
        }
    }

    private void ShutterButton_Click(object sender, RoutedEventArgs e)
    {
        if (_camera is not null && ReactiveFrame.Value is not null)
        {
            var frame = ReactiveFrame.Value;
            if (frame.Empty()) return;
            BgrXyzImageIO.SaveAsZip(_saveDir, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}", frame);
        }
        if (_player != null)
        {
            _player.Seek((int)PlaySlider.Value);
            var frame = _player.Read();
            BgrXyzImageIO.SaveAsZip(_saveDir, $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}", frame);
        }
    }

    private void RecButton_Click(object sender, RoutedEventArgs e)
    {
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
            if (!_isConnected) throw new Exception("Couldn't connect device!");

            var filePath = $"{_saveDir}\\Movie_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.yms";
            var writer = new BgrXyzRecorder(filePath);
            ReactiveFrame = _camera.GetStream().ToReadOnlyReactivePropertySlim();
            ReactiveFrame
                .Where(frame => frame is not null)
                .Finally(() => writer?.Dispose())
                .Subscribe(frame =>
                {
                    try
                    {
                        writer?.WriteFrame(frame);
                        _channel.Writer.TryWrite(frame);
                        using var d8 = frame.Depth8(300, 5000);
                        Dispatcher.Invoke(() =>
                        {
                            ColorFrame.Source = frame.BGR.ToBitmapSource();
                            DepthFrame.Source = d8.ToBitmapSource();
                        });
                    }
                    catch { }
                });
        }
        else
        {
            RecButton.Content = "Rec";
            RecButton.Background = Brushes.DarkGray;
            StartPauseButton.IsEnabled = true;
            PlayButton.IsEnabled = true;
            _isConnected = false;
            ReactiveFrame?.Dispose();
            _camera?.Dispose();
            _camera = null;
        }
    }

    private void SelectDirButton_Click(object sender, RoutedEventArgs e)
    {
        using var cofd = new CommonOpenFileDialog()
        {
            Title = "Select Saving Folder",
            InitialDirectory = _saveDir,
            IsFolderPicker = true,
        };
        if (cofd.ShowDialog() is CommonFileDialogResult.Ok) _saveDir = cofd.FileName;
        SaveDir.Content = _saveDir;
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        _player?.Dispose();
        _player = null;
        using var cofd = new CommonOpenFileDialog()
        {
            Title = "Select Video File",
            InitialDirectory = _videoDir,
            IsFolderPicker = false,
        };
        cofd.Filters.Add(new("YMS file", "*.yms"));
        if (cofd.ShowDialog() is CommonFileDialogResult.Ok)
        {
            _videoDir = Path.GetDirectoryName(cofd.FileName);
            _isConnected = true;
            PlayPauseButton.Content = "| |";
            PlayPauseButton.IsEnabled = true;
            PlayPauseButton.Visibility = Visibility.Visible;
            PlaySlider.Visibility = Visibility.Visible;
            _player = new BgrXyzPlayer(cofd.FileName);
            PlaySlider.Maximum = _player.FrameCount;
            ReactiveFrame = _player.GetStream().ToReadOnlyReactivePropertySlim();
            ReactiveFrame
                .Where(frame => frame is not null && !frame.IsDisposed && !frame.Empty())
                .Subscribe(frame =>
                {
                    _channel.Writer.TryWrite(frame);
                    using var d8 = frame.Depth8(300, 5000);
                    Dispatcher.Invoke(() =>
                    {
                        ColorFrame.Source = frame.BGR.ToBitmapSource();
                        DepthFrame.Source = d8.ToBitmapSource();
                        PlaySlider.Value = _player.CurrentPosition;
                    });
                });
        }
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isConnected)
        {
            _isConnected = true;
            PlaySlider.IsEnabled = false;
            PlayPauseButton.Content = "| |";
            ShutterButton.IsEnabled = false;
            _player.Seek((int)PlaySlider.Value);
            ReactiveFrame = _player.GetStream().ToReadOnlyReactivePropertySlim();
            ReactiveFrame
                .Where(frame => frame is not null && !frame.Empty())
                .Subscribe(frame =>
                {
                    _channel.Writer.TryWrite(frame);
                    using var d8 = frame.Depth8(300, 5000);
                    Dispatcher.Invoke(() =>
                    {
                        ColorFrame.Source = frame.BGR.ToBitmapSource();
                        DepthFrame.Source = d8.ToBitmapSource();
                        PlaySlider.Value = _player.CurrentPosition;
                    });
                });
        }
        else
        {
            ReactiveFrame?.Dispose();
            _isConnected = false;
            PlayPauseButton.Content = "▶";
            PlaySlider.IsEnabled = true;
            ShutterButton.IsEnabled = true;
        }
    }

    private void PlaySlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _player?.Seek((int)PlaySlider.Value);
        var frame = _player?.Read();
        ColorFrame.Source = frame?.BGR.ToBitmapSource();
        DepthFrame.Source = frame?.Depth8(300, 5000).ToBitmapSource();
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

    private async void ImageClicked(int x, int y)
    {
        if (_isConnected)
        {
            var frame = await _channel.Reader.ReadAsync();
            var info = frame.GetPointInfo(new(x, y));
            UV.Content = $"UV = ({x}, {y})";
            XYZ1.Content = $"XYZ = ({info.X}, {info.Y}, {info.Z})";
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
                }, 0, AlignBase.Color);
        }
        catch
        {
            try
            {
                _camera = new Realsense(new(640, 360)); // D
                //_camera = new Realsense(new(640, 480)); // L
            }
            catch
            {
                throw new Exception("Couldn't connect device!");
            }
        }
        return true;
    }


}
