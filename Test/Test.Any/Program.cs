using System.Text.Json;
using Husty.Geometry;

var angle = Angle.FromDegree(10);
var json = JsonSerializer.Serialize(angle);
var result = JsonSerializer.Deserialize<Angle>(json);
Console.WriteLine(json);
Console.WriteLine(result.Degree);
