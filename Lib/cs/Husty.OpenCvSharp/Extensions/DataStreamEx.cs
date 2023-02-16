using OpenCvSharp;
using Husty.IO;

namespace Husty.OpenCvSharp.Extensions;

public static class DataStreamEx
{

    public static async Task<bool> WriteMatAsync(this TcpDataTransporter stream, Mat image)
    {
        Cv2.ImEncode(".png", image, out byte[] buf);
        var data = Convert.ToBase64String(buf);
        var sendmsg = $"data:image/png;base64,{data}";
        return await stream.TryWriteLineAsync(sendmsg);
    }

    public static async Task<Mat> ReadMatAsync(this TcpDataTransporter stream)
    {
        var (success, rcv) = await stream.TryReadLineAsync();
        if (!success) return null;
        var data = rcv.Split(',')[1];
        var bytes = Convert.FromBase64String(data);
        return Cv2.ImDecode(bytes, ImreadModes.Unchanged);
    }

}
