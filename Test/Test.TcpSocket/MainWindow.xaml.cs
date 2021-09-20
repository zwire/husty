using System.IO;
using System.Windows;
using System.Threading.Tasks;
using OpenCvSharp;
using Husty.IO;
using Husty.OpenCvSharp;

namespace Test.TcpSocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private readonly TcpSocketServer _server;
        private BidirectionalDataStream _stream;

        public MainWindow()
        {
            InitializeComponent();
            _server = new TcpSocketServer(3000, 3001);
            Task.Run(async () => _stream = await _server.GetStreamAsync());
            Closed += (sender, args) =>
            {
                _stream?.Dispose();
                _server?.Dispose();
            };
        }

        private async void StartClientButton_Click(object sender, RoutedEventArgs e)
        {
            StartClientButton.Visibility = Visibility.Collapsed;
            new SubWindow().Show();
            while (true)
            {
                await Task.Run(async () =>
                {
                    var recv = await _stream.ReadAsJsonAsync<string>();
                    if (recv is not null)
                    {
                        if (recv.Length > 6 && recv.Substring(0, 6) is "image;")
                        {
                            var path = recv.Split(";")[1];
                            if (File.Exists(path))
                            {
                                using var img = Cv2.ImRead(path);
                                if (!await _stream.WriteMatAsync(img))
                                {
                                    throw new System.Exception("failed to send image!");
                                }
                            }
                        }
                        recv = recv.Replace(",", "\n");
                        Dispatcher.Invoke(() => OutputLabel.Content = recv);
                    }
                    else
                    {
                        Dispatcher.Invoke(() => Close());
                    }
                });
            }
        }

    }
}
