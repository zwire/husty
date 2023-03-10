using Husty.Filters;
using Husty.Geometry;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenCvSharp;

var canvas = new Mat(300, 300, MatType.CV_8UC3, new Scalar(255, 255, 255));
var lines = File.ReadAllLines("transfer.csv");
var f = new ParticleFilter(new double[3], 2, 2, 300)
{
    NonlinearTransitionFunction = p =>
    {
        var vx = p.U[0];
        var vy = p.U[1];
        var gyro = p.U[2];
        var x = p.X[0];
        var y = p.X[1];
        var yaw = p.X[2];
        var c = Math.Cos(yaw);
        var s = Math.Sin(yaw);
        var dx = (c * vx - s * vy) * p.Dt;
        var dy = (s * vx + c * vy) * p.Dt;
        var dyaw = gyro * p.Dt;
        return DenseVector.OfArray(new[] { x + dx, y + dy, yaw + dyaw });
    },
    NonlinearObservationFunction = p => DenseVector.OfArray(new[] { p.X[0], p.X[1] }),
    Dt = 0.1
};
f.P *= 0;
f.Q[0, 0] = Math.Pow(0.1, 2);                  // Y position standard deviation (m)
f.Q[1, 1] = Math.Pow(0.1, 2);                  // X position standard deviation (m)
f.Q[2, 2] = Math.Pow(3 * Math.PI / 180, 2);    // differential odometry (rad/s)
f.R *= Math.Pow(0.04, 2);                      // initial GNSS noise standard deviation (m)

var x0 = double.Parse(lines[0].Split(',')[0]);
var y0 = double.Parse(lines[0].Split(',')[1]);

var std = 0.0;
var biasX = 0.0;
var biasY = 0.0;
var path = new List<Point2D>();
var traj = new List<Point2D>();
using var sw = new StreamWriter("out.csv");
for (int i = 0; i < lines.Length; i++)
{
    var strs = lines[i].Split(',');
    var px = double.Parse(strs[0]) - x0;
    var py = double.Parse(strs[1]) - y0;
    var x = px;
    var y = py;
    var dev = double.Parse(strs[2]);
    var gyro = double.Parse(strs[3]);
    f.R[0, 0] = dev;
    f.R[1, 1] = dev;

    // noisy
    if (i % 150 is 0)
    {
        std = 4.0;
        biasX = Normal.Sample(0, std - 1);
        biasY = Normal.Sample(0, std - 1);
    }
    if (i / 150 % 2 is 1)
    {
        x += biasX + Normal.Sample(0, 1);
        y += biasY + Normal.Sample(0, 1);
        f.R[0, 0] = Math.Pow(std, 2);
        f.R[1, 1] = Math.Pow(std, 2);
    }

    var predicted = f.Predict(0.7, 0, gyro);
    f.Update(x, y);

    traj.Add(new(predicted[0] + 200, predicted[1] + 200));
    path.Add(new(x + 200, y + 200));
    canvas.At<Vec3b>((int)path.Last().X, (int)path.Last().Y) = new Vec3b(0, 0, 0);
    canvas.At<Vec3b>((int)traj.Last().X, (int)traj.Last().Y) = new Vec3b(0, 255, 0);
    using var view = new Mat(canvas.Height * 2, canvas.Width * 2, MatType.CV_8UC3, new Scalar(255, 255, 255));
    Cv2.Resize(canvas, view, view.Size(), 0, 0, InterpolationFlags.Cubic);
    foreach (var p in f.Particles)
    {
        var z = f.NonlinearObservationFunction(new(p.State));
        view.At<Vec3b>((int)z[0] * 2 + 400, (int)z[1] * 2 + 400) = new Vec3b(0, 0, 255);
    }
    Cv2.ImShow(" ", view);
    Cv2.WaitKey(1);

    Console.WriteLine(
        $"corrected: ({predicted[0]:f2} {predicted[1]:f2}), " +
        $"noisy: ({x:f2} {y:f2}), " +
        $"real: ({px:f2} {py:f2}), " +
        $"gyro: {gyro * 180 / Math.PI:f3}"
    );
    sw.WriteLine(string.Join(',', new[] { predicted[0], predicted[1], x, y, px, py, gyro }));

}
Console.ReadKey();