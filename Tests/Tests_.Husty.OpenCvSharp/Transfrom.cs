using OpenCvSharp;
using Husty.OpenCvSharp.CameraCalibration;
using Husty.OpenCvSharp.Transform;
using Xunit.Abstractions;

namespace Tests_.Husty.OpenCvSharp;

public class Transfrom
{

    private readonly ITestOutputHelper _output;

    public Transfrom(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public unsafe void PerspectiveTransform()
    {
        var inParam = IntrinsicCameraParameters.Load("..\\..\\..\\intrinsic.json");
        var exParam = ExtrinsicCameraParameters.Load("..\\..\\..\\extrinsic.json");
        var transformer = new PerspectiveTransformer(inParam.CameraMatrix, exParam);
        var dps = new Point2f[] { new(200, 200), new(300, 200), new(430, 150), new(100, 300) };

        var wps = transformer.ConvertToWorldCoordinate(dps);
        var rdps = transformer.ConvertToDisplayCoordinate(wps);

        foreach (var (a, b, c) in dps.Zip(wps, rdps))
        {
            _output.WriteLine($"{a} {b} {c}");
            Assert.Equal(a.ToPoint(), c.ToPoint());
        }

        

    }
}