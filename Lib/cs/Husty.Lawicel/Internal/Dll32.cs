using System.Text;
using System.Runtime.InteropServices;
using static Husty.Lawicel.CANUSB;

namespace Husty.Lawicel;

internal static class Dll32
{
    [DllImport("canusbdrv.dll")]
    internal static extern uint canusb_Open(
      string szID,
      string szBitrate,
      uint acceptance_code,
      uint acceptance_mask,
      uint flags);

    [DllImport("canusbdrv.dll")]
    internal static extern uint canusb_Open(
      IntPtr szID,
      string szBitrate,
      uint acceptance_code,
      uint acceptance_mask,
      uint flags);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_Close(uint handle);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_Read(uint handle, out CANMsg msg);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_ReadFirst(uint h, uint id, uint flags, out CANMsg msg);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_Write(uint handle, ref CANMsg msg);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_Status(uint handle);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_VersionInfo(uint handle, StringBuilder verinfo);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_Flush(uint h, byte flushflags);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_SetTimeouts(uint h, uint receiveTimeout, uint transmitTimeout);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_getFirstAdapter(StringBuilder szAdapter, int size);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_getNextAdapter(StringBuilder szAdapter, int size);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_setReceiveCallBack(
      uint handle,
      CANMsgCallbackDef rxMsgCallback);

    [DllImport("canusbdrv.dll")]
    internal static extern int canusb_setReceiveCallBack(uint handle, IntPtr szID);
}
