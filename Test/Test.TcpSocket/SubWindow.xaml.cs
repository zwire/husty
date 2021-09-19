using OpenCvSharp;
using Husty.IO;
using Husty.OpenCvSharp;
using System.Threading.Tasks;

namespace Test.TcpSocket
{
    /// <summary>
    /// SubWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SubWindow : System.Windows.Window
    {

        private readonly TcpSocketClient _client;
        private readonly BidirectionalDataStream _stream;

        public SubWindow()
        {
            InitializeComponent();
            _client = new TcpSocketClient("127.0.0.1", 3001, 3000);
            _stream = _client.GetStream();
            Closed += (sender, args) =>
            {
                _stream?.Dispose();
                _client?.Dispose();
            };
        }

        private void SendButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var msg = InputBox.Text;
            InputBox.Text = "";
            Task.Run(() =>
            {
                if (_stream.WriteAsJson(msg))
                {
                    if (msg.Length > 6 && msg.Substring(0, 6) is "image;")
                    {
                        var img = _stream.ReadMat();
                        Cv2.ImShow(" ", img);
                        Cv2.WaitKey();
                    }
                }
            });
        }

        private void Grid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is System.Windows.Input.Key.Enter)
                SendButton_Click(null, null);
        }
    }
}
