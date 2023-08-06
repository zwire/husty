using System.Text.Json;

namespace Husty.OpenCvSharp.DatasetFormat;

public class MsCoco
{
  public info info { set; get; } = new();
  public List<license> licences { set; get; } = new();
  public List<image> images { set; get; } = new();
  public List<annotation> annotations { set; get; } = new();
  public List<category> categories { set; get; } = new();
  public MsCoco Clone() => JsonSerializer.Deserialize<MsCoco>(JsonSerializer.Serialize(this));
}

public class info
{
  public string description { set; get; } = "";
  public string url { set; get; } = "";
  public string version { set; get; } = "1.0";
  public int year { set; get; } = DateTimeOffset.Now.Year;
  public string contributor { set; get; } = "";
  public string date_created { set; get; } = $"{DateTimeOffset.Now:yyyy/MM/dd}";
}

public class license
{
  public string url { set; get; } = "";
  public int id { set; get; } = 0;
  public string name { set; get; } = "";
}

public class image
{
  public int license { set; get; } = 0;
  public string file_name { set; get; } = "";
  public string coco_url { set; get; } = "";
  public int height { set; get; } = 0;
  public int width { set; get; } = 0;
  public string date_captured { set; get; } = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}";
  public string flickr_url { set; get; } = "";
  public int id { set; get; } = new Random((int)DateTimeOffset.Now.Ticks).Next(10000, 999999);
}

public class annotation
{
  public List<double[]> segmentation { set; get; } = new();
  public int num_keypoints { set; get; } = 0;
  public double area { set; get; } = 0;
  public int iscrowd { set; get; } = 0;
  public List<double[]> keypoints { set; get; } = new();
  public int image_id { set; get; } = 0;
  public List<double> bbox { set; get; } = new();
  public int category_id { set; get; } = 1;
  public int id { set; get; } = new Random((int)DateTimeOffset.Now.Ticks).Next(1000000, int.MaxValue);
  public string caption { set; get; } = "";
}

public class category
{
  public string supercategory { set; get; } = "";
  public int id { set; get; } = 0;
  public string name { set; get; } = "";
  public string[] keypoints { set; get; } = Array.Empty<string>();
  public int[][] skeleton { set; get; } = Array.Empty<int[]>();
}