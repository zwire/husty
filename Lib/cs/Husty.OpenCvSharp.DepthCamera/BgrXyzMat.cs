using OpenCvSharp;

namespace Husty.OpenCvSharp.DepthCamera;

/// <summary>
/// Point Cloud with Color
/// </summary>
public sealed class BgrXyzMat : IDisposable
{

    // ------ properties ------ //

    /// <summary>
    /// Color image
    /// </summary>
    public Mat BGR { private set; get; }

    /// <summary>
    /// Point Cloud image
    /// </summary>
    public Mat XYZ { private set; get; }

    public int Width => BGR.Width;

    public int Height => BGR.Height;

    public int Rows => BGR.Rows;

    public int Cols => BGR.Cols;

    public bool IsDisposed => BGR.IsDisposed || XYZ.IsDisposed;

    public BgrXyzMat this[Rect box]
    {
        set 
        {
            BGR[box] = value.BGR;
            XYZ[box] = value.XYZ;
        }
        get 
        {
            return new(BGR[box], XYZ[box]);
        }
    }


    // ------ constructors ------ //

    /// <summary>
    /// Create empty instance.
    /// </summary>
    public BgrXyzMat()
    {
        BGR = new();
        XYZ = new();
    }

    /// <summary>
    /// Hold Point Cloud with Color.
    /// </summary>
    /// <param name="bgr">Color image</param>
    /// <param name="xyz">Point Cloud image (must be same size of Color image)</param>
    public BgrXyzMat(Mat bgr, Mat xyz)
    {
        BGR = bgr;
        XYZ = xyz;
    }

    /// <summary>
    /// Decode from byte array.
    /// </summary>
    /// <param name="bgrBytes"></param>
    /// <param name="xyzBytes"></param>
    public BgrXyzMat(byte[] bgrBytes, byte[] xyzBytes)
    {
        BGR = Cv2.ImDecode(bgrBytes, ImreadModes.Unchanged);
        XYZ = Cv2.ImDecode(xyzBytes, ImreadModes.Unchanged);
        if (BGR.Width != XYZ.Width || BGR.Height != XYZ.Height)
            throw new InvalidOperationException("Require: BGR size == XYZ size");
    }


    // ------ public methods ------ //


    public void Dispose()
    {
        BGR?.Dispose();
        XYZ?.Dispose();
    }

    public void Deconstruct(out Mat bgr, out Mat xyz)
    {
        bgr = BGR;
        xyz = XYZ;
    }

    /// <summary>
    /// Same function as constructor.
    /// </summary>
    /// <param name="bgr">Color Image</param>
    /// <param name="xyz">Point Cloud Image (must be same size of Color Image)</param>
    /// <returns></returns>
    public static BgrXyzMat Create(Mat bgr, Mat xyz)
        => new(bgr, xyz);

    /// <summary>
    /// Decode from byte array.
    /// </summary>
    /// <param name="bgrBytes"></param>
    /// <param name="xyzBytes"></param>
    /// <returns></returns>
    public static BgrXyzMat YmsDecode(byte[] bgrBytes, byte[] xyzBytes)
        => new(Cv2.ImDecode(bgrBytes, ImreadModes.Unchanged), Cv2.ImDecode(xyzBytes, ImreadModes.Unchanged));

    /// <summary>
    /// Output encoded byte array.
    /// </summary>
    /// <returns></returns>
    public (byte[] BGRBytes, byte[] XYZBytes) YmsEncode()
        => (BGR.ImEncode(), XYZ.ImEncode());

    /// <summary>
    /// Check if it's empty.
    /// </summary>
    /// <returns></returns>
    public bool Empty() => BGR.Empty() || XYZ.Empty();

    /// <summary>
    /// Create deep copy of this object.
    /// </summary>
    /// <returns></returns>
    public BgrXyzMat Clone() => new(BGR.Clone(), XYZ.Clone());

    public void CopyFrom(Mat bgr, Mat xyz)
    {
        if (bgr.Width != BGR.Width || bgr.Height != BGR.Height ||
            xyz.Width != XYZ.Width || xyz.Height != XYZ.Height
        ) throw new InvalidOperationException("Require: src size == dst size");
        bgr.CopyTo(BGR);
        xyz.CopyTo(XYZ);
    }

    public void CopyFrom(byte[] bgrBytes, byte[] xyzBytes)
    {
        using var bgr = Cv2.ImDecode(bgrBytes, ImreadModes.Unchanged);
        using var xyz = Cv2.ImDecode(xyzBytes, ImreadModes.Unchanged);
        if (bgr.Width != BGR.Width || bgr.Height != BGR.Height ||
            xyz.Width != XYZ.Width || xyz.Height != XYZ.Height
        ) throw new InvalidOperationException("Require: src size == dst size");
        bgr.CopyTo(BGR);
        xyz.CopyTo(XYZ);
    }

    public BgrXyzMat Resize(Size size, InterpolationFlags flags = InterpolationFlags.Linear)
    {
        Cv2.Resize(BGR, BGR, size, 0, 0, flags);
        Cv2.Resize(XYZ, XYZ, size, 0, 0, flags);
        return this;
    }

    public BgrXyzMat Resize(double fx, double fy, InterpolationFlags flags = InterpolationFlags.Linear)
    {
        Cv2.Resize(BGR, BGR, Size.Zero, fx, fy, flags);
        Cv2.Resize(XYZ, XYZ, Size.Zero, fx, fy, flags);
        return this;
    }

    /// <summary>
    /// Get Depth image (Normalize value in 0-255)
    /// </summary>
    /// <param name="minDistance">(mm)</param>
    /// <param name="maxDistance">(mm)</param>
    /// <returns></returns>
    public unsafe Mat Depth8(int minDistance, int maxDistance)
    {
        var d = Depth16();
        Cv2.Threshold(d, d, maxDistance, maxDistance, ThresholdTypes.TozeroInv);
        Cv2.Threshold(d, d, minDistance, minDistance, ThresholdTypes.Tozero);
        Cv2.Normalize(d, d, 0, 255, NormTypes.MinMax, MatType.CV_8U);
        return d;
    }

    /// <summary>
    /// Depth image (0-65535)(mm)
    /// </summary>
    public unsafe Mat Depth16()
    {
        var d16 = new Mat();
        Cv2.ExtractChannel(XYZ, d16, 2);
        return d16;
    }

    /// <summary>
    /// Get information of where you input.
    /// </summary>
    /// <param name="point">Pixel Coordinate</param>
    /// <returns>Real 3D coordinate with color</returns>
    public unsafe BGRXYZ GetPointInfo(Point point)
    {
        if (BGR.Width != XYZ.Width || BGR.Height != XYZ.Height)
            throw new InvalidOperationException("Require: BGR size == XYZ size");
        var index = (point.Y * BGR.Cols + point.X) * 3;
        var bgr = BGR.DataPointer;
        var xyz = (short*)XYZ.Data;
        var b = bgr[index + 0];
        var g = bgr[index + 1];
        var r = bgr[index + 2];
        var x = xyz[index + 0];
        var y = xyz[index + 1];
        var z = xyz[index + 2];
        return new BGRXYZ(b, g, r, x, y, z);
    }

    /// <summary>
    /// Move all Point Cloud.
    /// </summary>
    /// <param name="delta">3D vector of translation (mm)</param>
    public unsafe BgrXyzMat Move(Vec3s delta)
    {
        var s = (short*)XYZ.Data;
        var index = 0;
        for (int i = 0; i < XYZ.Rows * XYZ.Cols; i++)
        {
            s[index++] += delta.Item0;
            s[index++] += delta.Item1;
            s[index++] += delta.Item2;
        }
        return this;
    }

    /// <summary>
    /// Change Point Cloud scale.
    /// </summary>
    /// <param name="delta">Scale of XYZ direction</param>
    public unsafe BgrXyzMat Scale(Vec3s delta)
    {
        var s = (short*)XYZ.Data;
        var index = 0;
        for (int i = 0; i < XYZ.Rows * XYZ.Cols; i++)
        {
            s[index++] *= delta.Item0;
            s[index++] *= delta.Item1;
            s[index++] *= delta.Item2;
        }
        return this;
    }

    /// <summary>
    /// Rotate 3D
    /// </summary>
    /// <param name="rotationMat">Rotation Matrix</param>
    /// <returns></returns>
    public unsafe BgrXyzMat Rotate(Mat rotationMat)
    {
        var d = (float*)rotationMat.Data;
        var s = (short*)XYZ.Data;
        for (int i = 0; i < XYZ.Rows * XYZ.Cols * 3; i += 3)
        {
            var x = s[i + 0];
            var y = s[i + 1];
            var z = s[i + 2];
            s[i + 0] = (short)(d[0] * x + d[1] * y + d[2] * z);
            s[i + 1] = (short)(d[3] * x + d[4] * y + d[5] * z);
            s[i + 2] = (short)(d[6] * x + d[7] * y + d[8] * z);
        }
        return this;
    }

}


/// <summary>
/// Record of Point and Color
/// </summary>
public record BGRXYZ(byte B, byte G, byte R, short X, short Y, short Z)
{

    public Vec3s Vector3 { get; } = new(X, Y, Z);

}

