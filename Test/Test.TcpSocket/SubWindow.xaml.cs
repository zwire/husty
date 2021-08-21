using OpenCvSharp;
using Husty.TcpSocket;
using Husty.TcpSocket.MatExtensions;

namespace Test.TcpSocket
{
    /// <summary>
    /// SubWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SubWindow : System.Windows.Window
    {

        private readonly ITcpSocket _client;

        public SubWindow()
        {
            InitializeComponent();
            _client = new Client("127.0.0.1", 3000);
            Closed += (sender, args) => _client.Close();
        }

        private void SendButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var msg = InputBox.Text;
            _client.Send(msg);
            if (msg.Length > 6 && msg.Substring(0, 6) == "image;")
            {
                var img = _client.ReceiveImage();
                Cv2.ImShow(" ", img);
                Cv2.WaitKey(0);
            }
        }
    }
}
