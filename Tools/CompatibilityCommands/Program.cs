// zipconvert, ymsconvert
using System.IO.Compression;
using System.Text;
using Husty.OpenCvSharp.ThreeDimensionalImaging;
using OpenCvSharp;

var command = args[0];
// src file / folder path
var srcPath = args[1];
// dst file / folder path
var dstPath = args[2];

Console.WriteLine($"executing {command}: {srcPath} --> {dstPath} ...");

if (command is "zipconvert")
{
  ExecuteProcess(".zip", ConvertZipFile);
}
else if (command is "ymsconvert")
{
  ExecuteProcess(".yms", ConvertYmsFile);
}

void ExecuteProcess(string ext, Action<string, string> action)
{
  if (File.Exists(srcPath) && Path.GetExtension(srcPath) == ext)
  {
    action(srcPath, dstPath);
  }
  else if (Directory.Exists(srcPath) && Directory.Exists(dstPath))
  {
    foreach (var file in Directory.GetFiles(srcPath))
    {
      if (Path.GetExtension(file) == ext)
      {
        var dirs = file.Split("\\");
        dirs[^1] = "new_" + dirs[^1];
        var dstFilePath = string.Join("\\", dirs);
        action(file, dstFilePath);
      }
    }
  }
  else
  {
    throw new FileNotFoundException();
  }
}

void ConvertZipFile(string srcPath, string dstPath)
{
  try
  {
    var frame = BgrXyzImage.FromZip(srcPath);
    Console.WriteLine("src zip file version is latest.");
  }
  catch
  {
    try
    {
      using var archive = ZipFile.OpenRead(srcPath);
      archive.GetEntry("C.png").ExtractToFile("C.png", true);
      archive.GetEntry("P.png").ExtractToFile("P.png", true);
      var c = new Mat("C.png");
      var p = new Mat("P.png", ImreadModes.Unchanged).Split();
      File.Delete("C.png");
      File.Delete("P.png");
      var frame = new BgrXyzImage(c, p[0], p[1], p[2]);
      frame.SaveAsZip(dstPath);
      Console.WriteLine($"Successfully saved file as {dstPath}");
    }
    catch
    {
      Console.WriteLine("Error: invalid file type");
    }
  }
}

void ConvertYmsFile(string srcPath, string dstPath)
{
  try
  {
    var frame = new VideoStream(srcPath);
    Console.WriteLine("src yms file version is latest.");
  }
  catch
  {
    try
    {
      var file = File.Open(srcPath, FileMode.Open, FileAccess.ReadWrite);
      var binReader = new BinaryReader(file, Encoding.ASCII);
      var fileFormatCode = Encoding.ASCII.GetString(binReader.ReadBytes(8));
      if (fileFormatCode is "HUSTY001")
      {
        binReader.ReadInt64();
      }
      else if (fileFormatCode is not "HUSTY000")
      {
        throw new Exception("invalid file format");
      }
      var indexesPos = binReader.ReadInt64();
      var indexes = new List<long>();
      if (indexesPos is -1)
      {
        binReader.BaseStream.Position = fileFormatCode is "HUSTY000" ? 16 : 24;
        while (true)
        {
          indexes.Add(binReader.BaseStream.Position);
          if (binReader.BaseStream.Position + 8 > binReader.BaseStream.Length - 1) break;
          binReader.BaseStream.Position += 8;
          var len0 = binReader.ReadInt32();
          if (binReader.BaseStream.Position + len0 > binReader.BaseStream.Length - 1) break;
          binReader.BaseStream.Position += len0;
          var len1 = binReader.ReadInt32();
          if (binReader.BaseStream.Position + len1 > binReader.BaseStream.Length - 1) break;
          binReader.BaseStream.Position += len1;
        }
        indexes.RemoveAt(indexes.Count - 1);

        var writer = new BinaryWriter(file, Encoding.ASCII);
        writer.Seek(fileFormatCode is "HUSTY000" ? 8 : 16, SeekOrigin.Begin);
        writer.Write(writer.BaseStream.Length);
        writer.Seek(0, SeekOrigin.End);
        indexes.ForEach(writer.Write);
      }
      else
      {
        binReader.BaseStream.Position = indexesPos;
        while (binReader.BaseStream.Position < binReader.BaseStream.Length)
          indexes.Add(binReader.ReadInt64());
        if (indexes.Count < 5)
          throw new Exception("frame count is too small");
      }
      indexes = indexes.ToList();

      binReader.BaseStream.Position = 0;
      binReader.BaseStream.Seek(indexes[0], SeekOrigin.Begin);
      binReader.ReadInt64();

      using var binWriter = new BinaryWriter(File.Open(dstPath, FileMode.Create), Encoding.ASCII);
      binWriter.Write(Encoding.ASCII.GetBytes("HUSTY002"));
      binWriter.Write(0L);
      binWriter.Write(-1L);
      var wIndexes = new List<long>();
      var positionIndex = 0;
      while (true)
      {
        if (positionIndex == indexes.Count - 1) break;
        binReader.BaseStream.Seek(indexes[positionIndex++], SeekOrigin.Begin);
        var time = binReader.ReadInt64();
        var bgrDataSize = binReader.ReadInt32();
        var bgrBytes = binReader.ReadBytes(bgrDataSize);
        var xyzDataSize = binReader.ReadInt32();
        var xyzBytes = binReader.ReadBytes(xyzDataSize);
        using var xyzMat = Cv2.ImDecode(xyzBytes, ImreadModes.Unchanged);
        var mats = xyzMat.Split();
        var xBytes = mats[0].ImEncode();
        var yBytes = mats[1].ImEncode();
        var zBytes = mats[2].ImEncode();
        wIndexes.Add(binWriter.BaseStream.Position);
        binWriter.Write(time);
        binWriter.Write((ushort)0);
        binWriter.Write(sizeof(int) * 4 + bgrBytes.Length + xBytes.Length + yBytes.Length + zBytes.Length);
        binWriter.Write(bgrBytes.Length);
        binWriter.Write(bgrBytes);
        binWriter.Write(xBytes.Length);
        binWriter.Write(xBytes);
        binWriter.Write(yBytes.Length);
        binWriter.Write(yBytes);
        binWriter.Write(zBytes.Length);
        binWriter.Write(zBytes);
      }
      binWriter.Seek(16, SeekOrigin.Begin);
      binWriter.Write(binWriter.BaseStream.Length);
      binWriter.Seek(0, SeekOrigin.End);
      wIndexes.ForEach(binWriter.Write);
      binWriter.Flush();
      binWriter.Close();
      binWriter.Dispose();
      Console.WriteLine($"Successfully saved file as {dstPath}");
    }
    catch
    {
      Console.WriteLine("Error: invalid file type");
    }
  }
}