using System.Runtime.InteropServices;
using System.Text;

namespace Husty.Lawicel;

internal static class CANUSB
{

  private static bool Process64 = Environment.Is64BitProcess;

  internal static uint canusb_Open(
    string szID,
    string szBitrate,
    uint acceptance_code,
    uint acceptance_mask,
    uint flags)
  {
    if (Process64)
      return Dll64.canusb_Open(szID, szBitrate, acceptance_code, acceptance_mask, flags);
    else
      return Dll32.canusb_Open(szID, szBitrate, acceptance_code, acceptance_mask, flags);
  }

  internal static uint canusb_Open(
    IntPtr szID,
    string szBitrate,
    uint acceptance_code,
    uint acceptance_mask,
    uint flags)
  {
    if (Process64)
      return Dll64.canusb_Open(szID, szBitrate, acceptance_code, acceptance_mask, flags);
    else
      return Dll32.canusb_Open(szID, szBitrate, acceptance_code, acceptance_mask, flags);
  }

  internal static int canusb_Close(uint handle)
  {
    if (Process64)
      return Dll64.canusb_Close(handle);
    else
      return Dll32.canusb_Close(handle);
  }

  internal static int canusb_Read(uint handle, out CANMsg msg)
  {
    if (Process64)
      return Dll64.canusb_Read(handle, out msg);
    else
      return Dll32.canusb_Read(handle, out msg);
  }

  internal static int canusb_ReadFirst(uint h, uint id, uint flags, out CANMsg msg)
  {
    if (Process64)
      return Dll64.canusb_ReadFirst(h, id, flags, out msg);
    else
      return Dll32.canusb_ReadFirst(h, id, flags, out msg);
  }

  internal static int canusb_Write(uint handle, ref CANMsg msg)
  {
    if (Process64)
      return Dll64.canusb_Write(handle, ref msg);
    else
      return Dll32.canusb_Write(handle, ref msg);
  }

  internal static int canusb_Status(uint handle)
  {
    if (Process64)
      return Dll64.canusb_Status(handle);
    else
      return Dll32.canusb_Status(handle);
  }

  internal static int canusb_VersionInfo(uint handle, StringBuilder verinfo)
  {
    if (Process64)
      return Dll64.canusb_VersionInfo(handle, verinfo);
    else
      return Dll32.canusb_VersionInfo(handle, verinfo);
  }

  internal static int canusb_Flush(uint h, byte flushflags)
  {
    if (Process64)
      return Dll64.canusb_Flush(h, flushflags);
    else
      return Dll32.canusb_Flush(h, flushflags);
  }

  internal static int canusb_SetTimeouts(uint h, uint receiveTimeout, uint transmitTimeout)
  {
    if (Process64)
      return Dll64.canusb_SetTimeouts(h, receiveTimeout, transmitTimeout);
    else
      return Dll32.canusb_SetTimeouts(h, receiveTimeout, transmitTimeout);
  }

  internal static int canusb_getFirstAdapter(StringBuilder szAdapter, int size)
  {
    if (Process64)
      return Dll64.canusb_getFirstAdapter(szAdapter, size);
    else
      return Dll32.canusb_getFirstAdapter(szAdapter, size);
  }

  internal static int canusb_getNextAdapter(StringBuilder szAdapter, int size)
  {
    if (Process64)
      return Dll64.canusb_getNextAdapter(szAdapter, size);
    else
      return Dll32.canusb_getNextAdapter(szAdapter, size);
  }

  internal static int canusb_setReceiveCallBack(
    uint handle,
    CANMsgCallbackDef rxMsgCallback)
  {
    if (Process64)
      return Dll64.canusb_setReceiveCallBack(handle, rxMsgCallback);
    else
      return Dll32.canusb_setReceiveCallBack(handle, rxMsgCallback);
  }

  internal static int canusb_setReceiveCallBack(uint handle, IntPtr szID)
  {
    if (Process64)
      return Dll64.canusb_setReceiveCallBack(handle, szID);
    else
      return Dll32.canusb_setReceiveCallBack(handle, szID);
  }



  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal readonly struct CANMsg
  {

    internal readonly uint id;
    internal readonly uint timestamp;
    internal readonly byte flags;
    internal readonly byte len;
    internal readonly ulong data;

    internal CANMsg(uint id, uint timestamp, byte flags, byte len, ulong data) =>
        (this.id, this.timestamp, this.flags, this.len, this.data) = (id, timestamp, flags, len, data);

  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal readonly struct CANMsgEx
  {

    internal readonly uint id;
    internal readonly uint timestamp;
    internal readonly byte flags;
    internal readonly byte len;

    internal CANMsgEx(uint id, uint timestamp, byte flags, byte len) =>
        (this.id, this.timestamp, this.flags, this.len) = (id, timestamp, flags, len);

  }

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  internal struct CANMsgCallback
  {

    internal uint id;
    internal uint timestamp;
    internal byte flags;
    internal byte len;
    internal ulong data;

  }

  internal delegate void CANMsgCallbackDef(ref CANMsgCallback msg);

}
