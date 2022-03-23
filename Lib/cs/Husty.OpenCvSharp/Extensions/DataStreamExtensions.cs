using System;
using System.Threading.Tasks;
using OpenCvSharp;
using Husty.IO;

namespace Husty.OpenCvSharp.Extensions
{
    public static class DataStreamExtensions
    {

        /// <summary>
        /// Write Mat as encoded array
        /// </summary>
        /// <param name="image"></param>
        public static bool WriteMat(this BidirectionalDataStream stream, Mat image)
        {
            return WriteMatAsync(stream, image).Result;
        }

        /// <summary>
        /// Write Mat as encoded array
        /// </summary>
        /// <param name="image"></param>
        public static async Task<bool> WriteMatAsync(this BidirectionalDataStream stream, Mat image)
        {
            Cv2.ImEncode(".png", image, out byte[] buf);
            var data = Convert.ToBase64String(buf);
            var sendmsg = $"data:image/png;base64,{data}";
            return await stream.WriteStringAsync(sendmsg);
        }

        /// <summary>
        /// Read and convert to Mat
        /// </summary>
        /// <returns></returns>
        public static Mat ReadMat(this BidirectionalDataStream stream)
        {
            return ReadMatAsync(stream).Result;
        }

        /// <summary>
        /// Read and convert to Mat
        /// </summary>
        /// <returns></returns>
        public static async Task<Mat> ReadMatAsync(this BidirectionalDataStream stream)
        {
            var recv = await stream.ReadStringAsync();
            var data = recv.Split(',')[1];
            var bytes = Convert.FromBase64String(data);
            return Cv2.ImDecode(bytes, ImreadModes.Unchanged);
        }

    }
}
