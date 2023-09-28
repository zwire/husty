using Husty.Communication;
using OpenCvSharp;

namespace Husty.OpenCvSharp.Extensions;

public static class DataStreamEx
{

  public static async Task<bool> WriteMatAsync(this DataTransporter stream, Mat image)
  {
    Cv2.ImEncode(".png", image, out byte[] buf);
    var data = Convert.ToBase64String(buf);
    var sendmsg = $"data:image/png;base64,{data}";
    return await stream.TryWriteLineAsync(sendmsg);
  }

  public static async Task<Mat> ReadMatAsync(this DataTransporter stream)
  {
    var result = await stream.TryReadLineAsync();
    if (!result.IsOk) return null;
    var data = result.Unwrap().Split(',')[1];
    var bytes = Convert.FromBase64String(data);
    return Cv2.ImDecode(bytes, ImreadModes.Unchanged);
  }

}
