using System;
using OpenCvSharp;

namespace Husty.TcpSocket.MatExtensions
{
    public static class MatExtensions
    {

        /// <summary>
        /// Send Mat as encoded byte array
        /// </summary>
        /// <param name="image"></param>
        public static void SendMat(this ITcpSocket socketBase, Mat image)
        {
            Cv2.ImEncode(".png", image, out byte[] buf);
            var data = Convert.ToBase64String(buf);
            var sendmsg = $"data:image/png;base64,{data}";
            socketBase.Send(sendmsg);
        }

        /// <summary>
        /// Receive byte array & convert Mat
        /// </summary>
        /// <returns></returns>
        public static Mat ReceiveMat(this ITcpSocket socketBase)
        {
            var recv = socketBase.Receive<string>();
            var data = recv.Split(',')[1];
            var bytes = Convert.FromBase64String(data);
            return Cv2.ImDecode(bytes, ImreadModes.Unchanged);
        }

    }
}
