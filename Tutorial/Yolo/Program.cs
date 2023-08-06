using System.Diagnostics;
using Husty.OpenCvSharp.Extensions;
using Husty.OpenCvSharp.Yolo;
using OpenCvSharp;

// model files
var cfg = "..\\..\\..\\model\\_.cfg";
var weights = "..\\..\\..\\model\\_.weights";
var names = "..\\..\\..\\model\\_.names";
var onnx = "..\\..\\..\\model\\yolov7-tiny.onnx";

// initialize detector.
// blob width and height must be multiple of 32.
//var detector = new Yolov3v4(cfg, weights, names, new Size(384, 256));
//var detector = new YoloX(onnx, names);
var detector = new Yolov7(onnx, names);
var colors = Enumerable.Range(0, 80).Select(_ => new Scalar(new Random().Next(255), new Random().Next(255), new Random().Next(255))).ToArray();

// input image path
var img = Cv2.ImRead("..\\..\\..\\sample2.jpg");

// execute inference session
detector.Run(img); // initialize?
var watch = new Stopwatch();
watch.Start();
var results = detector.Run(img);
watch.Stop();
Console.WriteLine(watch.ElapsedMilliseconds + " ms");

// show results
foreach (var r in results)
{
  r.DrawBox(img, new(0, 0, 255), 2);
  Console.WriteLine($"{r.Label} : {r.Confidence * 100:f0}%");
}
Cv2.ImShow(" ", img);
Cv2.WaitKey();