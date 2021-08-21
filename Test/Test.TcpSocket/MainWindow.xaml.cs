using System.Threading.Tasks;
using System.Windows;
using System.IO;
using OpenCvSharp;
using Husty.TcpSocket;
using Husty.TcpSocket.MatExtensions;

namespace Test.TcpSocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private ITcpSocket _server;

        public MainWindow()
        {
            InitializeComponent();
            Task.Run(() => _server = new Server("127.0.0.1", 3000));
            Closed += (sender, args) => _server.Close();
        }

        private async void StartClientButton_Click(object sender, RoutedEventArgs e)
        {
            StartClientButton.Visibility = Visibility.Collapsed;
            new SubWindow().Show();
            while (true)
            {
                await Task.Run(() =>
                {
                    var recv = _server.Receive<string>();
                    if(recv.Length > 6 && recv.Substring(0, 6) == "image;")
                    {
                        var path = recv.Split(";")[1];
                        if (File.Exists(path))
                        {
                            var img = Cv2.ImRead(path);
                            _server.SendMat(img);
                        }
                    }
                    recv = recv.Replace(",", "\n");
                    Dispatcher.Invoke(() => OutputLabel.Content = recv);
                });
            }
        }

    }
}
