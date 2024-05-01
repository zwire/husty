using Husty.Filters;
using Husty.Geometry;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenCvSharp;
using static System.Math;

var lines = File.ReadAllLines("transfer.csv");
var stdX = 0.05;        // longitudinal position standard deviation in transition (m)
var stdY = 0.01;        // lateral position standard deviation in transition (m)
var stdYaw = 0.01;      // angle standard deviation in transition (rad)
var yaw = 0.0;

var canvas = new Mat(300, 300, MatType.CV_8UC3, new Scalar(255, 255, 255));
var f = new ParticleFilter(new double[] { 0, 0, yaw }, 2, 3, 300)
{
  TransitionFunc = p =>
  {
    var (vx, vy, vr) = (p.U[0], p.U[1], p.U[2]);
    var (x, y, r) = (p.X[0], p.X[1], p.X[2]);
    var (dx, dy) = new Vector2D(vx, vy).Rotate(Angle.FromRadian(r));
    return DenseVector.OfArray(new[] { x + dx * p.Dt, y + dy * p.Dt, r + vr * p.Dt });
  },
  ObservationFunc = p => DenseVector.OfArray(new[] { p.X[0], p.X[1] }),
  Dt = 0.1
};
f.P *= 0;
f.Q[0, 0] = Pow(stdX, 2);
f.Q[1, 1] = Pow(stdY, 2);
f.Q[2, 2] = Pow(stdYaw, 2);

var vx = 0.7;
var vy = 0.0;

var x0 = double.Parse(lines[0].Split(',')[0]);
var y0 = double.Parse(lines[0].Split(',')[1]);

var stddev = 3.0;
var biasX = 0.0;
var biasY = 0.0;
var path = new List<Point2D>();
var traj = new List<Point2D>();
using var sw = new StreamWriter("log.csv");
sw.WriteLine("predicted,,observed,,");
sw.WriteLine("x,y,x,y,yawrate");
for (int i = 0; i < lines.Length; i++)
{
  var strs = lines[i].Split(',');
  var px = double.Parse(strs[0]) - x0;
  var py = double.Parse(strs[1]) - y0;
  var x = px;
  var y = py;
  var dev = double.Parse(strs[2]);
  var vr = double.Parse(strs[3]);
  var qx = Cos(yaw) * stdX - Sin(yaw) * stdY;
  var qy = Sin(yaw) * stdX + Cos(yaw) * stdY;
  f.Q[0, 0] = qx * qx;
  f.Q[0, 1] = qx * qy;
  f.Q[1, 0] = qx * qy;
  f.Q[1, 1] = qy * qy;
  f.R[0, 0] = dev;
  f.R[1, 1] = dev;

  // add noise on observation
  if (i % 150 is 0)
  {
    biasX = Normal.Sample(0, 1);
    biasY = Normal.Sample(0, 1);
  }
  if (i / 150 % 2 is 1)
  {
    x += biasX + Normal.Sample(0, stddev - 1);
    y += biasY + Normal.Sample(0, stddev - 1);
    f.R[0, 0] = Pow(stddev, 2);
    f.R[1, 1] = Pow(stddev, 2);
  }

  // Jacobian of transition (for EKF)
  f.A[0, 2] = (-vx * Sin(yaw) - vy * Cos(yaw)) * f.Dt;
  f.A[1, 2] = (+vx * Cos(yaw) - vy * Sin(yaw)) * f.Dt;

  // update filter
  var predicted = f.Predict(0.7, 0, vr);
  yaw = predicted[2];
  f.Update(x, y);

  // visualize
  traj.Add(new(predicted[0] + 200, 300 - (predicted[1] + 200)));
  path.Add(new(x + 200, 300 - (y + 200)));
  canvas.At<Vec3b>((int)path.Last().Y, (int)path.Last().X) = new Vec3b(0, 0, 0);
  canvas.At<Vec3b>((int)traj.Last().Y, (int)traj.Last().X) = new Vec3b(0, 255, 0);
  using var view = new Mat(canvas.Height * 2, canvas.Width * 2, MatType.CV_8UC3, new Scalar(255, 255, 255));
  Cv2.Resize(canvas, view, view.Size(), 0, 0, InterpolationFlags.Cubic);
  foreach (var p in f.Particles)
    view.At<Vec3b>(600 - ((int)p.State[1] * 2 + 400), (int)p.State[0] * 2 + 400) = new Vec3b(0, 0, 255);
  Cv2.ImShow(" ", view);
  Cv2.WaitKey(1);

  Console.WriteLine(
      $"predicted: ({predicted[0]:f2} {predicted[1]:f2}), " +
      $"observed: ({x:f2} {y:f2}), " +
      $"yawrate: {vr * 180 / PI:f3}"
  );
  sw.WriteLine(string.Join(',', new[] { predicted[0], predicted[1], x, y, vr }));

}
Console.ReadKey();