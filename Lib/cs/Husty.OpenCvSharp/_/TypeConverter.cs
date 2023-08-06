using OpenCvSharp;
using static OpenCvSharp.MatType;

namespace Husty.OpenCvSharp;

public static class TypeConverter
{

  public static Type ToElementType(this MatType type)
  {
    if (type == CV_8UC1 || type == CV_8UC2 || type == CV_8UC3 || type == CV_8UC4)
      return typeof(byte);
    else if (type == CV_8SC1 || type == CV_8SC2 || type == CV_8SC3 || type == CV_8SC4)
      return typeof(sbyte);
    else if (type == CV_16UC1 || type == CV_16UC2 || type == CV_16UC3 || type == CV_16UC4)
      return typeof(ushort);
    else if (type == CV_16SC1 || type == CV_16SC2 || type == CV_16SC3 || type == CV_16SC4)
      return typeof(short);
    else if (type == CV_32SC1 || type == CV_32SC2 || type == CV_32SC3 || type == CV_32SC4)
      return typeof(int);
    else if (type == CV_32FC1 || type == CV_32FC2 || type == CV_32FC3 || type == CV_32FC4)
      return typeof(float);
    else if (type == CV_64FC1 || type == CV_64FC2 || type == CV_64FC3 || type == CV_64FC4)
      return typeof(double);
    else
      throw new NotSupportedException();
  }

  public static MatType CreateMatType(Type type, int channels)
  {
    if (channels < 1)
      throw new ArgumentException("Require: channels >= 1");
    if (type == typeof(byte))
      return CV_8UC(channels);
    else if (type == typeof(sbyte))
      return CV_8SC(channels);
    else if (type == typeof(ushort))
      return CV_16UC(channels);
    else if (type == typeof(short))
      return CV_16SC(channels);
    else if (type == typeof(int))
      return CV_32SC(channels);
    else if (type == typeof(float))
      return CV_32FC(channels);
    else if (type == typeof(double))
      return CV_64FC(channels);
    else
      throw new NotSupportedException();
  }

}
