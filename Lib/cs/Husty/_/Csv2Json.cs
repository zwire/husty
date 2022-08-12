namespace Husty;

public static class Csv2Json
{
    public static string[] Convert(string allText, params string[] stringPropertyNames)
    {
        var lines = allText.Split('\n');
        var header = new List<string[]>(lines[0].Split(',').Select(x => x.Split(':').ToArray()));
        if (!header.Any()) return null;
        lines = lines[1..];
        var txts = new List<string>();
        foreach (var line in lines)
        {
            var txt = "{ ";
            var strs = line.Split(',');
            if (strs.Length != header.Count)
                continue;
            for (int i = 0; i < header[0].Length; i++)
            {
                txt += $"\"{header[0][i]}\": ";
                if (stringPropertyNames?.Any(x => header[0].Contains(x)) is true)
                    txt += $"\"{strs[0]}\"";
                else if (bool.TryParse(strs[0], out bool bl))
                    txt += bl.ToString().ToLower();
                else if (double.TryParse(strs[0], out double num))
                    txt += num.ToString();
                else if (strs[0] is "")
                    txt += "";
                else
                    txt += $"\"{strs[0]}\"";
                if (i != header[0].Length - 1)
                    txt += "{ ";
            }
            for (int i = 1; i < header.Count; i++)
            {
                var h1 = header[i - 1];
                var h2 = header[i];
                var same = 0;
                for (int j = 0; j < Math.Min(h1.Length, h2.Length); j++)
                {
                    if (h1[j] != h2[j]) break;
                    same++;
                }
                for (int j = 1; j < h1.Length - same; j++)
                {
                    txt += " }";
                }
                txt += ", ";
                for (int j = same; j < h2.Length; j++)
                {
                    txt += $"\"{h2[j]}\": ";
                    if (j != h2.Length - 1)
                        txt += "{ ";
                }
                if (stringPropertyNames?.Any(x => header[i].Contains(x)) is true)
                    txt += $"\"{strs[i]}\"";
                else if (bool.TryParse(strs[i], out bool bl))
                    txt += bl.ToString().ToLower();
                else if (double.TryParse(strs[i], out double num))
                    txt += num.ToString();
                else if (strs[i] is "")
                    txt += "";
                else
                    txt += $"\"{strs[i]}\"";
                if (i == header.Count - 1)
                {
                    foreach (var h in h2)
                    {
                        txt += " }";
                    }
                }
            }
            txts.Add(txt);
        }
        return txts.ToArray();
    }

}
