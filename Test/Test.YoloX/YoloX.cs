using System;
using System.Collections.Generic;
using System.Linq;
using Husty;
using Husty.OnnxRuntime;
using Husty.OpenCvSharp;
using OpenCvSharp;

namespace Test.YoloX
{

    public class YoloX : OnnxBase<Mat, IEnumerable<(int Label, Rect Box)>>
    {

        public YoloX(string model) : base(model, Provider.CPU, OptimizationLevel.Off) { }


        public override IEnumerable<(int Label, Rect Box)> Run(Mat frame)
        {

            var xRatio = frame.Width / 416.0;
            var yRatio = frame.Height / 416.0;
            var inputArray = frame.Resize(new Size(416, 416)).To1DByteArray().AsFloatArray();
            var outputArray = base.Run(inputArray)["output"].To2DJaggedArray(3549, 85);

            foreach (var stride in new[] { 8, 16, 32 })
            {
                var xSize = 416 / stride;
                var ySize = 416 / stride;
                for (int i = 0; i < ySize; i++)
                {
                    for (int j = 0; j < xSize; j++)
                    {
                        var r = outputArray[i * xSize + j];
                        var index = r.Skip(5).ToArray().ArgMax(out var prob);
                        if (r[4] * prob > 0.001)
                        {
                            var x = (r[0] + j) * stride * xRatio;
                            var y = (r[1] + i) * stride * yRatio;
                            var w = Math.Exp(r[2]) * stride * xRatio;
                            var h = Math.Exp(r[3]) * stride * yRatio;
                            yield return(index, new Rect2d(x - w / 2, y - h / 2, w, h).ToRect());
                        }
                    }
                }
            }
        }

    }
}
