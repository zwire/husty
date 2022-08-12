using System.Text.Json;

namespace Husty;

public static class Json2Csv
{

    // ------ public methods ------ //

    public static string GetHeader(string jstr)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jstr);
        return ExtractHeader("", dict) + "\n";
    }

    public static string GetRow(string jstr)
    {
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jstr);
        return ExtractRow(dict) + "\n";
    }


    // ------ private methods ------ //

    private static string ExtractHeader(string parent, Dictionary<string, object> dict)
    {
        var cstr = "";
        var first = true;
        foreach (var pair in dict)
        {
            if (pair.Value is null)
                continue;
            if (!first)
                cstr += ",";
            first = false;
            var jstr = pair.Value.ToString();
            if (jstr.StartsWith("{") && jstr.EndsWith("}") && jstr.Contains(':'))
                cstr += ExtractHeader(parent + pair.Key + ':', JsonSerializer.Deserialize<Dictionary<string, object>>(jstr));
            else
                cstr += (parent + pair.Key);
        }
        return cstr;
    }

    static string ExtractRow(Dictionary<string, object> dict)
    {
        var cstr = "";
        var first = true;
        foreach (var pair in dict)
        {
            if (pair.Value is null)
                continue;
            if (!first)
                cstr += ",";
            first = false;
            var jstr = pair.Value.ToString();
            if (jstr.StartsWith("{") && jstr.EndsWith("}") && jstr.Contains(':'))
                cstr += ExtractRow(JsonSerializer.Deserialize<Dictionary<string, object>>(jstr));
            else
                cstr += pair.Value;
        }
        return cstr;
    }

}
