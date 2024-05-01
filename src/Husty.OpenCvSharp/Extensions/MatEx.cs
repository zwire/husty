using System.Numerics;
using OpenCvSharp;

namespace Husty.OpenCvSharp.Extensions;

public static class MatEx
{

  public static Type GetElementType(this Mat input)
  {
    var type = input.Type();
    if (type == MatType.CV_8UC1 || type == MatType.CV_8UC2 || type == MatType.CV_8UC3 || type == MatType.CV_8UC4)
      return typeof(byte);
    else if (type == MatType.CV_8SC1 || type == MatType.CV_8SC2 || type == MatType.CV_8SC3 || type == MatType.CV_8SC4)
      return typeof(sbyte);
    else if (type == MatType.CV_16UC1 || type == MatType.CV_16UC2 || type == MatType.CV_16UC3 || type == MatType.CV_16UC4)
      return typeof(ushort);
    else if (type == MatType.CV_16SC1 || type == MatType.CV_16SC2 || type == MatType.CV_16SC3 || type == MatType.CV_16SC4)
      return typeof(short);
    else if (type == MatType.CV_32SC1 || type == MatType.CV_32SC2 || type == MatType.CV_32SC3 || type == MatType.CV_32SC4)
      return typeof(int);
    else if (type == MatType.CV_32FC1 || type == MatType.CV_32FC2 || type == MatType.CV_32FC3 || type == MatType.CV_32FC4)
      return typeof(float);
    else if (type == MatType.CV_64FC1 || type == MatType.CV_64FC2 || type == MatType.CV_64FC3 || type == MatType.CV_64FC4)
      return typeof(double);
    else
      throw new NotSupportedException();
  }

  public unsafe static Mat Map<T>(this Mat mat, Func<T, T> func) where T : unmanaged, INumber<T>
  {
    if (mat.GetElementType() != typeof(T))
      throw new TypeLoadException(nameof(T));
    var clone = mat.Clone();
    var d = (T*)clone.Data;
    for (int i = 0; i < mat.Width * mat.Height; i++)
      d[i] = func(d[i]);
    return clone;
  }

  /// <summary>
  /// Get points array from binary image
  /// </summary>
  /// <param name="input">must be 1 channel</param>
  /// <returns>array of points</returns>
  public unsafe static Point[] GetNonZeroLocations(this Mat input)
  {

    if (input.Channels() is not 1)
      throw new ArgumentException("Require: channels == 1");

    var length = Cv2.CountNonZero(input);
    var w = input.Width;
    var h = input.Height;
    var type = input.GetElementType();
    if (type == typeof(byte))
      return Do((byte*)input.Data, (byte)0, w, h, length);
    else if (type == typeof(sbyte))
      return Do((sbyte*)input.Data, (sbyte)0, w, h, length);
    else if (type == typeof(ushort))
      return Do((ushort*)input.Data, (ushort)0, w, h, length);
    else if (type == typeof(short))
      return Do((short*)input.Data, (short)0, w, h, length);
    else if (type == typeof(int))
      return Do((int*)input.Data, 0, w, h, length);
    else if (type == typeof(float))
      return Do((float*)input.Data, 0, w, h, length);
    else if (type == typeof(double))
      return Do((double*)input.Data, 0, w, h, length);
    else
      throw new NotSupportedException();

    unsafe static Point[] Do<T>(T* data, T zero, int w, int h, int length) where T : unmanaged, INumber<T>
    {
      var count = 0;
      var p = new Point[length];
      for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
          if (data[y * w + x].CompareTo(zero) > 0)
            p[count++] = new(x, y);
      return p;
    }

  }

  public static double GetAspectRatio(this Mat src, bool belowOne = false)
  {
    return belowOne
        ? Math.Min((float)src.Width / src.Height, (float)src.Height / src.Width)
        : (float)src.Width / src.Height;
  }

  public static Vec2s ToVec2s(this Mat src)
  {
    if (src.Rows is 2 && src.Cols is 1)
      return new(src.At<short>(0, 0), src.At<short>(1, 0));
    else if (src.Rows is 1 && src.Cols is 2)
      return new(src.At<short>(0, 0), src.At<short>(0, 1));
    else
      throw new ArgumentException("Require: source matrix must be 2x1 or 1x2");
  }

  public static Vec2i ToVec2i(this Mat src)
  {
    if (src.Rows is 2 && src.Cols is 1)
      return new(src.At<int>(0, 0), src.At<int>(1, 0));
    else if (src.Rows is 1 && src.Cols is 2)
      return new(src.At<int>(0, 0), src.At<int>(0, 1));
    else
      throw new ArgumentException("Require: source matrix must be 2x1 or 1x2");
  }

  public static Vec2f ToVec2f(this Mat src)
  {
    if (src.Rows is 2 && src.Cols is 1)
      return new(src.At<float>(0, 0), src.At<float>(1, 0));
    else if (src.Rows is 1 && src.Cols is 2)
      return new(src.At<float>(0, 0), src.At<float>(0, 1));
    else
      throw new ArgumentException("Require: source matrix must be 2x1 or 1x2");
  }

  public static Vec2d ToVec2d(this Mat src)
  {
    if (src.Rows is 2 && src.Cols is 1)
      return new(src.At<double>(0, 0), src.At<double>(1, 0));
    else if (src.Rows is 1 && src.Cols is 2)
      return new(src.At<double>(0, 0), src.At<double>(0, 1));
    else
      throw new ArgumentException("Require: source matrix must be 2x1 or 1x2");
  }

  public static Vec3s ToVec3s(this Mat src)
  {
    if (src.Rows is 3 && src.Cols is 1)
      return new(src.At<short>(0, 0), src.At<short>(1, 0), src.At<short>(2, 0));
    else if (src.Rows is 1 && src.Cols is 3)
      return new(src.At<short>(0, 0), src.At<short>(0, 1), src.At<short>(0, 2));
    else
      throw new ArgumentException("Require: source matrix must be 3x1 or 1x3");
  }

  public static Vec3i ToVec3i(this Mat src)
  {
    if (src.Rows is 3 && src.Cols is 1)
      return new(src.At<int>(0, 0), src.At<int>(1, 0), src.At<int>(2, 0));
    else if (src.Rows is 1 && src.Cols is 3)
      return new(src.At<int>(0, 0), src.At<int>(0, 1), src.At<int>(0, 2));
    else
      throw new ArgumentException("Require: source matrix must be 3x1 or 1x3");
  }

  public static Vec3f ToVec3f(this Mat src)
  {
    if (src.Rows is 3 && src.Cols is 1)
      return new(src.At<float>(0, 0), src.At<float>(1, 0), src.At<float>(2, 0));
    else if (src.Rows is 1 && src.Cols is 3)
      return new(src.At<float>(0, 0), src.At<float>(0, 1), src.At<float>(0, 2));
    else
      throw new ArgumentException("Require: source matrix must be 3x1 or 1x3");
  }

  public static Vec3d ToVec3d(this Mat src)
  {
    if (src.Rows is 3 && src.Cols is 1)
      return new(src.At<double>(0, 0), src.At<double>(1, 0), src.At<double>(2, 0));
    else if (src.Rows is 1 && src.Cols is 3)
      return new(src.At<double>(0, 0), src.At<double>(0, 1), src.At<double>(0, 2));
    else
      throw new ArgumentException("Require: source matrix must be 3x1 or 1x3");
  }

  public unsafe static byte[] To1DByteArray(this Mat image)
  {
    var length = image.Rows * image.Cols * image.Channels();
    var d = image.DataPointer;
    var array = new byte[length];
    for (int i = 0; i < length; i++) array[i] = d[i];
    return array;
  }

  public unsafe static short[] To1DShortArray(this Mat image)
  {
    var length = image.Rows * image.Cols * image.Channels();
    var d = (short*)image.Data;
    var array = new short[length];
    for (int i = 0; i < length; i++) array[i] = d[i];
    return array;
  }

  public unsafe static int[] To1DIntArray(this Mat image)
  {
    var length = image.Rows * image.Cols * image.Channels();
    var d = (int*)image.Data;
    var array = new int[length];
    for (int i = 0; i < length; i++) array[i] = d[i];
    return array;
  }

  public unsafe static float[] To1DFloatArray(this Mat image)
  {
    var length = image.Rows * image.Cols * image.Channels();
    var d = (float*)image.Data;
    var array = new float[length];
    for (int i = 0; i < length; i++) array[i] = d[i];
    return array;
  }

  public unsafe static double[] To1DDoubleArray(this Mat image)
  {
    var length = image.Rows * image.Cols * image.Channels();
    var d = (double*)image.Data;
    var array = new double[length];
    for (int i = 0; i < length; i++) array[i] = d[i];
    return array;
  }

  public unsafe static Mat ToMat(this byte[] array, int rows, int cols, int channels)
  {
    var image = new Mat(rows, cols, MatType.CV_8UC(channels));
    var length = rows * cols * channels;
    if (array.Length != length) throw new ArgumentException();
    var d = image.DataPointer;
    for (int i = 0; i < length; i++) d[i] = array[i];
    return image;
  }

  public unsafe static Mat ToMat(this short[] array, int rows, int cols, int channels)
  {
    var image = new Mat(rows, cols, MatType.CV_16SC(channels));
    var length = rows * cols * channels;
    if (array.Length != length) throw new ArgumentException();
    var d = (short*)image.Data;
    for (int i = 0; i < length; i++) d[i] = array[i];
    return image;
  }

  public unsafe static Mat ToMat(this int[] array, int rows, int cols, int channels)
  {
    var image = new Mat(rows, cols, MatType.CV_32SC(channels));
    var length = rows * cols * channels;
    if (array.Length != length) throw new ArgumentException();
    var d = (int*)image.Data;
    for (int i = 0; i < length; i++) d[i] = array[i];
    return image;
  }

  public unsafe static Mat ToMat(this float[] array, int rows, int cols, int channels)
  {
    var image = new Mat(rows, cols, MatType.CV_32FC(channels));
    var length = rows * cols * channels;
    if (array.Length != length) throw new ArgumentException();
    var d = (float*)image.Data;
    for (int i = 0; i < length; i++) d[i] = array[i];
    return image;
  }

  public unsafe static Mat ToMat(this double[] array, int rows, int cols, int channels)
  {
    var image = new Mat(rows, cols, MatType.CV_64FC(channels));
    var length = rows * cols * channels;
    if (array.Length != length) throw new ArgumentException();
    var d = (double*)image.Data;
    for (int i = 0; i < length; i++) d[i] = array[i];
    return image;
  }

}

