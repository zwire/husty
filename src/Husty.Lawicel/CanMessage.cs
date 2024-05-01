using static Husty.Lawicel.CANUSB;

namespace Husty.Lawicel;

public class CanMessage
{

  public uint Id { get; }

  public uint Timestamp { get; }

  public byte Flags { get; }

  public byte Length { get; }

  public ulong Data { get; }


  public CanMessage(uint id, ulong data, byte flags = CanUsbOption.EXTENDED, byte len = 8, uint timestamp = 0)
  {
    Id = id;
    Timestamp = timestamp;
    Flags = flags;
    Length = len;
    Data = data;
  }

  public CanMessage(uint id, byte[] data, byte flags = CanUsbOption.EXTENDED, byte len = 8, uint timestamp = 0)
  {
    if (data.Length is not 8) throw new ArgumentException("data length must be 8.");
    Id = id;
    Timestamp = timestamp;
    Flags = flags;
    Length = len;
    Data = BitConverter.ToUInt64(data);
  }

  public override string ToString()
  {
    var id = Id.ToString("X");
    var data = BitConverter.GetBytes(Data).Reverse().Select(x => x.ToString("X2")).ToArray();
    return $"0x{id}:{string.Join('-', data)}";
  }

  internal CANMsg ToCANMsg()
      => new(Id, Timestamp, Flags, Length, Data);

  internal static CanMessage FromCANMsg(CANMsg msg)
      => new(msg.id, msg.data, msg.flags, msg.len, msg.timestamp);

}
