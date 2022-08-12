using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;

namespace DataArranger;

public class J2Mask : IFunction
{

    public string GetFunctionExplanation()
    {
        return "convert json annotation file to binary image";
    }

    public string[] GetArgsExplanation()
    {
        return new[]
        {
            "input: input json folder path",
            "output: output folder path"
        };
    }

    public void Run(string input, string output, string[] args)
    {
        var files = Directory.GetFiles(input);
        foreach (var file in files)
        {
            if (Path.GetExtension(file) is not ".json") continue;
            var json = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(file));
            var w = (int)json["imageWidth"];
            var h = (int)json["imageHeight"];
            var img = new Mat(h, w, MatType.CV_8U, 0);
            foreach (var j in json["shapes"])
            {
                var poly = new List<Point>();
                foreach (var p in j["points"])
                    poly.Add(new Point((int)p[0], (int)p[1]));
                Cv2.FillPoly(img, new List<Point>[] { poly }, 255);
            }
            Cv2.ImWrite($"{output}\\{Path.GetFileNameWithoutExtension(file)}.png", img);
        }
    }
}
