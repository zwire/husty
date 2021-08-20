using System;
using OpenCvSharp;
using Husty.TcpSocket;

namespace Husty.OpenCvSharp.TcpSocketExtensions
{
    public static class TcpSocketExtensions
    {

        /// <summary>
        /// Send 'Mat' image as encoded byte array
        /// </summary>
        /// <param name="image"></param>
        public static void SendImage(this TcpSocketBase socketBase, Mat image)
        {
            Cv2.ImEncode(".png", image, out byte[] buf);
            var data = Convert.ToBase64String(buf);
            var sendmsg = $"data:image/png;base64,{data}";
            socketBase.Send(sendmsg);
        }

        /// <summary>
        /// Receive byte array & convert 'Mat' image
        /// </summary>
        /// <returns></returns>
        public static Mat ReceiveImage(this TcpSocketBase socketBase)
        {
            var recv = socketBase.Receive<string>();
            var data = recv.Split(',')[1];
            var bytes = Convert.FromBase64String(data);
            return Cv2.ImDecode(bytes, ImreadModes.Unchanged);
        }

    }
}
