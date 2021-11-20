using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenCvSharp;
using Husty.OpenCvSharp;

namespace Tools.Yolo_Validation
{
    // see https://qiita.com/cv_carnavi/items/08e11426e2fac8433fed
    internal class BackgroundProcess
    {

        private YoloDetector _detector;
        private List<Data> _dataSet;
        private string _className;
        private readonly int _width;
        private readonly int _height;
        private readonly double _probThresh;
        private readonly double _iouThresh;


        public BackgroundProcess(string modelFolder, int width, int height, int classNum, double probThresh, double iouThresh, string imgDir, string labDir)
        {
            _width = width;
            _height = height;
            _probThresh = probThresh;
            _iouThresh = iouThresh;
            Load(modelFolder, classNum, imgDir, labDir);
        }

        public Results Go()
        {

            var totalTp = 0;
            var totalFp = 0;
            var totalFn = 0;
            var totalTime = 0;
            var lines = new List<string>();
            var predictionList = new List<(float Probability, bool Correct)>();

            foreach (var ds in _dataSet)
            {

                // inference
                var img = ds.Image;
                var watch = new Stopwatch();
                watch.Start();
                var results = _detector.Run(img)
                    .Where(r => r.Label == _className)
                    .Where(r => r.Probability >= _probThresh)
                    .ToArray();
                watch.Stop();
                var time = watch.ElapsedMilliseconds;

                // scan predicted results and ground truth
                var tp = 0;
                foreach (var r in results)
                {
                    var correct = false;
                    var iou = 0.0;
                    foreach (var l in ds.Labels)
                    {
                        iou = CalcIou(r.Box, new(new(l.Center.X - l.Size.Width / 2.0, l.Center.Y - l.Size.Height / 2.0), l.Size));
                        if (iou >= _iouThresh)
                        {
                            tp++;
                            correct = true;
                            break;
                        }
                    }
                    predictionList.Add(((float)(r.Probability * iou), correct));
                }

                var fp = results.Length - tp;
                var fn = ds.Labels.Length - tp;
                lines.Add($"{ds.Name},{time},{tp},{fp},{fn}");
                totalTp += tp;
                totalFp += fp;
                totalFn += fn;
                totalTime += (int)time;
            }

            var truthCount = 0;
            foreach (var ds in _dataSet) truthCount += ds.Labels.Length;
            var ap = CalcAp(predictionList, truthCount);
            var avgTime = totalTime / _dataSet.Count;
            var precision = (double)totalTp / (totalTp + totalFp);
            var recall = (double)totalTp / (totalTp + totalFn);

            // save
            var t = DateTimeOffset.Now;
            using var sw = new StreamWriter($"validation_{t.Year}{t.Month:d2}{t.Day:d2}_{t.Hour:d2}{t.Minute:d2}{t.Second:d2}.csv");
            sw.WriteLine($"Width,{_width},Height,{_height}");
            sw.WriteLine("");
            sw.WriteLine("TotalTime,AverageTime,TotalTp,TotalFp,TotalFn");
            sw.WriteLine($"{totalTime},{avgTime},{totalTp},{totalFp},{totalFn}");
            sw.WriteLine("");
            sw.WriteLine("Precision,Recall,Ap");
            sw.WriteLine($"{precision},{recall},{ap}");
            sw.WriteLine("");
            sw.WriteLine("FileName,Time,Tp,Fp,Fn");
            lines.ForEach(l => sw.WriteLine(l));

            return new(_className, totalTime, avgTime, totalTp, totalFp, totalFn, precision, recall, ap);

        }

        private double CalcAp(List<(float Probability, bool Correct)> list, int truthCount)
        {
            list = list.OrderByDescending(l => l.Probability).ToList();
            var tpCount = 0;
            var fpCount = 0;
            var rpSet = new List<(double Recall, double Precision)>();
            foreach (var (probability, correct) in list)
            {
                if (!correct)
                {
                    fpCount++;
                }
                else
                {
                    tpCount++;
                    var recall = (double)tpCount / truthCount;
                    var precision = (double)tpCount / (tpCount + fpCount);
                    rpSet.Add((recall, precision));
                }
            }

            // max precision to the right
            rpSet.Reverse();
            var maxPrecision = 0.0;
            var smoothList = new List<(double Recall, double Precision)>();
            foreach(var rp in rpSet)
            {
                if (maxPrecision < rp.Precision) maxPrecision = rp.Precision;
                smoothList.Add((rp.Recall, maxPrecision));
            }
            smoothList.Reverse();

            // integral area
            var preR = 0.0;
            var totalArea = 0.0;
            foreach (var (recall, precision) in smoothList)
            {
                totalArea += (recall - preR) * precision;
                preR = recall;
            }

            return totalArea;
        }

        private void Load(string modelFolder, int classNum, string imgDir, string labelDir)
        {

            // load model folder
            var cfg = "";
            var names = "";
            var weights = "";
            foreach (var file in Directory.GetFiles(modelFolder))
            {
                if (Path.GetExtension(file) is ".cfg") cfg = file;
                else if (Path.GetExtension(file) is ".weights") weights = file;
                else if (Path.GetExtension(file) is ".names") names = file;
            }
            if (cfg is "" || names is "" || weights is "") throw new Exception("invalid model files path");
            _className = File.ReadAllText(names).Split("\n")[classNum];
            if (_iouThresh < 0.0 || _iouThresh > 1.0) throw new Exception("invalid IoU threshold");
            try { _detector = new(cfg, weights, names, new Size(_width, _height), (float)_probThresh); }
            catch { throw new Exception("failed to load model files!"); }

            // load images and labels
            var imgPaths = new List<string>();
            imgPaths.AddRange(Directory.GetFiles(imgDir, "*.png"));
            imgPaths.AddRange(Directory.GetFiles(imgDir, "*.PNG"));
            imgPaths.AddRange(Directory.GetFiles(imgDir, "*.jpg"));
            imgPaths.AddRange(Directory.GetFiles(imgDir, "*.JPG"));
            imgPaths.AddRange(Directory.GetFiles(imgDir, "*.jpeg"));
            imgPaths.AddRange(Directory.GetFiles(imgDir, "*.JPEG"));
            var labelPaths = new List<string>(Directory.GetFiles(labelDir, "*.txt"));
            try
            {
                _dataSet = new();
                foreach (var imgPath in imgPaths)
                {
                    var img = new Mat(imgPath);
                    var name = Path.GetFileNameWithoutExtension(imgPath);
                    foreach (var labelPath in labelPaths)
                    {
                        if (name == Path.GetFileNameWithoutExtension(labelPath))
                        {
                            var lines = File.ReadAllText(labelPath).Split("\n");
                            var labelList = new List<GroundTruth>();
                            foreach (var line in lines)
                            {
                                var s = line.Split(" ");
                                if (int.TryParse(s[0], out var label) &&
                                    double.TryParse(s[1], out var x) &&
                                    double.TryParse(s[2], out var y) &&
                                    double.TryParse(s[3], out var w) &&
                                    double.TryParse(s[4], out var h)
                                )
                                {
                                    labelList.Add(new(label, new(x, y), new(w, h)));
                                }
                            }
                            _dataSet.Add(new(name, img, labelList.ToArray()));
                            break;
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("failed to match image and label files");
            }
        }

        private static double CalcIou(Rect2d r1, Rect2d r2)
        {
            var area1 = r1.Width * r1.Height;
            var area2 = r2.Width * r2.Height;
            if (r1.Left > r2.Right || r2.Left > r1.Right || r1.Top > r2.Bottom || r2.Top > r1.Bottom) return 0.0;
            var left = Math.Max(r1.Left, r2.Left);
            var right = Math.Min(r1.Right, r2.Right);
            var top = Math.Max(r1.Top, r2.Top);
            var bottom = Math.Min(r1.Bottom, r2.Bottom);
            var union = (right - left) * (bottom - top);
            return (double)union / (area1 + area2 - union);
        }

    }

    internal record Results(
        string ClassName,
        int TotalTime,
        int AvgTime,
        int Tp,
        int Fp,
        int Fn,
        double Precision,
        double Recall,
        double Ap
    );

    internal record Data(string Name, Mat Image, GroundTruth[] Labels);

    internal record GroundTruth(int LabelId, Point2d Center, Size2d Size);


}
