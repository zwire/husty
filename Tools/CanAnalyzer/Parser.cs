using Husty.Lawicel;

namespace CanAnalyzer;

public static class Parser
{

    public static string GetBaudrate(int value)
    {
        return value switch
        {
            0 => CanUsbOption.BAUD_10K,
            1 => CanUsbOption.BAUD_20K,
            2 => CanUsbOption.BAUD_50K,
            3 => CanUsbOption.BAUD_100K,
            4 => CanUsbOption.BAUD_125K,
            5 => CanUsbOption.BAUD_250K,
            6 => CanUsbOption.BAUD_500K,
            7 => CanUsbOption.BAUD_800K,
            8 => CanUsbOption.BAUD_1M,
            _ => ""
        };
    }

    public static TimeSpan GetFreqTimeSpan(int value)
    {
        return value switch
        {
            0 => TimeSpan.FromMilliseconds(1000),
            1 => TimeSpan.FromMilliseconds(500),
            2 => TimeSpan.FromMilliseconds(200),
            3 => TimeSpan.FromMilliseconds(100),
            4 => TimeSpan.FromMilliseconds(50),
            5 => TimeSpan.FromMilliseconds(20),
            6 => TimeSpan.FromMilliseconds(10),
            _ => throw new NotImplementedException()
        };
    }

    public static CanMessage CreateMessage(string id, string data, bool extend)
    {
        var bytes = new byte[8];
        var count = 0;
        if (data.StartsWith("0b"))
        {
            data = data[2..];
            for (int i = data.Length; i > 0; i -= 8)
            {
                var s = data.Substring(i < 8 ? 0 : i - 8, i < 8 ? i : 8);
                bytes[count++] = Convert.ToByte(s, 2);
            }
        }
        else if (data.StartsWith("0x"))
        {
            data = data[2..];
            for (int i = data.Length; i > 0; i -= 2)
            {
                var s = data.Substring(i is 1 ? 0 : i - 2, i is 1 ? 1 : 2);
                bytes[count++] = Convert.ToByte(s, 16);
            }
        }
        else
        {
            bytes = BitConverter.GetBytes(ulong.Parse(data));
        }
        
        var type = 10;
        if (id.Length > 1)
        {
            if (id[..2] is "0b")
            {
                type = 2;
                id = id[2..];
            }
            else if (id[..2] is "0x")
            {
                type = 16;
                id = id[2..];
            }
        }
        return new(
            Convert.ToUInt32(id, type), 
            BitConverter.ToUInt64(bytes), 
            extend ? CanUsbOption.EXTENDED : CanUsbOption.STANDARD
        );
    }

    public static string[] ParseMessage(CanMessage msg, int type = 16)
    {
        var id = "0x" + Convert.ToString(msg.Id, 16).ToUpper();
        var data = BitConverter.GetBytes(msg.Data).Reverse().Select(x => Convert.ToString(x, type).ToUpper()).ToArray();
        return new[] { id, string.Join('-', data) };
    }

}
