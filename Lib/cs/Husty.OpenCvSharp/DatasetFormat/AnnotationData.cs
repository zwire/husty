using System.Text.Json;
using Husty.OpenCvSharp.Extensions;
using OpenCvSharp;

namespace Husty.OpenCvSharp.DatasetFormat;

public class AnnotationData
{

    // ------ fields ------ //
 
    private readonly MsCoco _data;
    private double _scale = 1;


    // ------- constructors ------ //

    private AnnotationData(MsCoco data, double scale)
    {
        _scale = scale;
        _data = data.Clone();
    }

    public AnnotationData(string filePath)
    {
        if (File.Exists(filePath))
        {
            _data = JsonSerializer.Deserialize<MsCoco>(File.ReadAllText(filePath));
        }
        else
        {
            throw new ArgumentException("filePath must be exist!");
        }
    }

    public AnnotationData(string filePath, IEnumerable<string> imagePaths, IEnumerable<string> labelData)
    {
        if (File.Exists(filePath))
        {
            _data = JsonSerializer.Deserialize<MsCoco>(File.ReadAllText(filePath));
            for (int i = 0; i < _data.images.Count; i++)
            {
                if (imagePaths.All(x => !x.Contains(_data.images[i].file_name)))
                {
                    for (int j = 0; j < _data.annotations.Count; j++)
                    {
                        if (_data.annotations[j].image_id == _data.images[i].id)
                        {
                            _data.annotations.RemoveAt(j);
                            j--;
                        }
                    }
                    _data.images.RemoveAt(i);
                    i--;
                }
            }
            _data.images.AddRange(imagePaths
                .Where(p => _data.images.All(x => !p.Contains(x.file_name)))
                .Select(p =>
                {
                    using var img = Cv2.ImRead(p);
                    return new image()
                    {
                        file_name = Path.GetFileName(p),
                        width = img.Width,
                        height = img.Height
                    };
                })
            );
            var index = _data.categories.Count;
            _data.categories.AddRange(labelData
                .Where(label => _data.categories.All(c => c.name != label))
                .Select((label, i) => new category() { id = index + i + 1, name = label })
            );
        }
        else
        {
            _data = new()
            {
                images = imagePaths
                .Select(x =>
                {
                    using var img = Cv2.ImRead(filePath);
                    return new image()
                    {
                        file_name = Path.GetFileName(x),
                        width = img.Width,
                        height = img.Height
                    };
                }).ToList(),
                categories = labelData.Select((label, i) => new category() { id = i + 1, name = label }).ToList()
            };
        }
    }


    // ------ public methods ------ //

    public AnnotationData Clone()
    {
        return new(_data, _scale);
    }

    public string ExportAsJson()
    {
        return JsonSerializer.Serialize(_data);
    }

    public void SetScale(double scale)
    {
        _scale = scale;
    }

    public void SetImageData(string fileName, int width, int height, out int id)
    {
        _data.images.Add(new() { file_name = fileName, width = width, height = height });
        id = _data.images.Last().id;
    }

    public bool TryGetImageId(string fileName, out int id)
    {
        id = _data.images.Where(i => i.file_name == fileName).Select(i => i.id).FirstOrDefault();
        return id != default;
    }

    public void AddBoxData(int imageId, int labelId, Rect box, out int annotationId)
    {
        _data.annotations.Add(new() 
        {
            image_id = imageId, 
            category_id = labelId, 
            bbox = new() { box.Left * _scale, box.Top * _scale, box.Width * _scale, box.Height * _scale }
        });
        annotationId = _data.annotations.Last().id;
    }

    public void AddPolygonData(int imageId, int labelIndex, IEnumerable<IEnumerable<Point>> polygon, out int annotationId)
    {
        _data.annotations.Add(new()
        {
            image_id = imageId,
            category_id = labelIndex,
            segmentation = polygon.Select(pts => pts.Select(p => new double[] { p.X * _scale, p.Y * _scale }).SelectMany(x => x).ToArray()).ToList()
        });
        annotationId = _data.annotations.Last().id;
    }

    public void AddPolygonData(int imageId, int labelIndex, IEnumerable<Point> polygon, out int annotationId)
    {
        AddPolygonData(imageId, labelIndex, new[] { polygon }, out annotationId);
    }

    public void AddPointData(int imageId, int labelIndex, Point point, out int annotationId)
    {
        AddPolygonData(imageId, labelIndex, new[] { new[] { point } }, out annotationId);
    }

    public void AddLineData(int imageId, int labelIndex, Point pt1, Point pt2, out int annotationId)
    {
        AddPolygonData(imageId, labelIndex, new[] { new[] { pt1, pt2 } }, out annotationId);
    }

    public void AddKeyPointsData(int imageId, int labelIndex, IEnumerable<IEnumerable<int>> keyPoints, out int annotationId)
    {
        _data.annotations.Add(new()
        {
            image_id = imageId,
            category_id = labelIndex,
            keypoints = keyPoints.Select(x => x.Select(i => i * _scale).ToArray()).ToList()
        });
        annotationId = _data.annotations.Last().id;
    }

    public void SetBoxData(int imageId, int labelId, Rect box, int annotationId)
    {
        RemoveAnnotationData(annotationId);
        _data.annotations.Add(new()
        {
            image_id = imageId,
            category_id = labelId,
            bbox = new() { box.Left * _scale, box.Top * _scale, box.Width * _scale, box.Height * _scale },
            id = annotationId
        });
    }

    public void SetPolygonData(int imageId, int labelIndex, IEnumerable<IEnumerable<Point>> polygon, int annotationId)
    {
        RemoveAnnotationData(annotationId);
        _data.annotations.Add(new()
        {
            image_id = imageId,
            category_id = labelIndex,
            segmentation = polygon.Select(pts => pts.Select(p => new double[] { p.X * _scale, p.Y * _scale }).SelectMany(x => x).ToArray()).ToList(),
            id = annotationId
        });
    }

    public void SetPolygonData(int imageId, int labelIndex, IEnumerable<Point> polygon, int annotationId)
    {
        SetPolygonData(imageId, labelIndex, new[] { polygon }, annotationId);
    }

    public void SetPointData(int imageId, int labelIndex, Point point, int annotationId)
    {
        SetPolygonData(imageId, labelIndex, new[] { new[] { point } }, annotationId);
    }

    public void SetLineData(int imageId, int labelIndex, Point pt1, Point pt2, int annotationId)
    {
        SetPolygonData(imageId, labelIndex, new[] { new[] { pt1, pt2 } }, annotationId);
    }

    public Dictionary<int, (int Label, Rect Box)> GetBoxData(int imageId)
    {
        return _data.annotations
            .Where(x => x.image_id == imageId)
            .Where(x => x.bbox.Count is 4)
            .Select(x => (Id: x.id, Label: x.category_id, Box: new Rect((int)x.bbox[0], (int)x.bbox[1], (int)x.bbox[2], (int)x.bbox[3])))
            .ToDictionary(x => x.Id, x => (x.Label, x.Box.Scale(1 / _scale, 1 / _scale)));
    }

    public Dictionary<int, (int Label, Point[][] Points)> GetPolygonData(int imageId)
    {
        return getPolygonData(imageId)
            .Where(x => x.Points.All(y => y.Length > 2))
            .ToDictionary(x => x.Id, x => (x.Label, x.Points));
    }

    public Dictionary<int, (int Label, Point Point)> GetPointData(int imageId)
    {
        return getPolygonData(imageId)
            .Where(x => x.Points.All(y => y.Length is 1))
            .ToDictionary(x => x.Id, x => (x.Label, x.Points.FirstOrDefault().FirstOrDefault()));
    }

    public Dictionary<int, (int Label, Point P1, Point P2)> GetLineData(int imageId)
    {
        return getPolygonData(imageId)
            .Where(x => x.Points.All(y => y.Length is 2))
            .ToDictionary(x => x.Id, x => (x.Label, x.Points.FirstOrDefault()[0], x.Points.FirstOrDefault()[1]));
    }

    public void RemoveAnnotationData(int annotationId)
    {
        var index = _data.annotations.FindIndex(i => i.id == annotationId);
        if (index is not -1)
            _data.annotations.RemoveAt(index);
    }


    // ------ private methods ------ //

    private IEnumerable<(int Id, int Label, Point[][] Points)> getPolygonData(int imageId)
    {
        return _data.annotations
            .Where(x => x.image_id == imageId)
            .Where(x => x.segmentation.Any())
            .Select(x => (Id: x.id, Label: x.category_id,
                Points: x
                    .segmentation
                    .Select(z =>
                    {
                        var points = new Point[z.Length / 2];
                        for (int i = 0; i < z.Length / 2; i++)
                            points[i] = new Point(z[i * 2] / _scale, z[i * 2 + 1] / _scale);
                        return points;
                    })
                    .ToArray()
            ));
    }

}
