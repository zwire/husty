using System;
using static Husty.Lawicel.CANUSB;

namespace Husty.Lawicel
{
    public class CanMessage
    {

        private readonly uint _id;
        private readonly uint _timestamp;
        private readonly byte _flags;
        private readonly byte _len;
        private readonly ulong _data;

        public const string BAUD_BTR_1M = "0x00:0x14";
        public const string BAUD_BTR_500K = "0x00:0x1C";
        public const string BAUD_BTR_250K = "0x01:0x1C";
        public const string BAUD_BTR_125K = "0x03:0x1C";
        public const string BAUD_BTR_100K = "0x43:0x2F";
        public const string BAUD_BTR_50K = "0x47:0x2F";
        public const string BAUD_BTR_20K = "0x53:0x2F";
        public const string BAUD_BTR_10K = "0x67:0x2F";
        public const string BAUD_BTR_5K = "0x7F:0x7F";
        public const string BAUD_1M = "1000";
        public const string BAUD_800K = "800";
        public const string BAUD_500K = "500";
        public const string BAUD_250K = "250";
        public const string BAUD_125K = "125";
        public const string BAUD_100K = "100";
        public const string BAUD_50K = "50";
        public const string BAUD_20K = "20";
        public const string BAUD_10K = "10";
        public const int ERROR_OK = 1;
        public const int ERROR_OPEN_SUBSYSTEM = -2;
        public const int ERROR_COMMAND_SUBSYSTEM = -3;
        public const int ERROR_NOT_OPEN = -4;
        public const int ERROR_TX_FIFO_FULL = -5;
        public const int ERROR_INVALID_PARAM = -6;
        public const int ERROR_NO_MESSAGE = -7;
        public const int ERROR_MEMORY_ERROR = -8;
        public const int ERROR_NO_DEVICE = -9;
        public const int ERROR_TIMEOUT = -10;
        public const int ERROR_INVALID_HARDWARE = -11;
        public const byte EXTENDED = 128;
        public const byte RTR = 64;
        public const byte FLAG_TIMESTAMP = 1;
        public const byte FLAG_QUEUE_REPLACE = 2;
        public const byte FLAG_BLOCK = 4;
        public const byte FLAG_SLOW = 8;
        public const byte FLAG_NO_LOCAL_SEND = 16;
        public const uint ACCEPTANCE_CODE_ALL = 0;
        public const uint ACCEPTANCE_MASK_ALL = 4294967295;
        public const uint FLUSH_WAIT = 0;
        public const uint FLUSH_DONTWAIT = 1;
        public const uint FLUSH_EMPTY_INQUEUE = 2;


        public CanMessage(uint id, uint timestamp, byte flags, byte len, ulong data)
        {
            _id = id;
            _timestamp = timestamp;
            _flags = flags;
            _len = len;
            _data = data;
        }

        public CanMessage(uint id, uint timestamp, byte flags, byte len, byte[] data)
        {
            if (data.Length is not 8) throw new ArgumentException("data length must be 8.");
            _id = id;
            _timestamp = timestamp;
            _flags = flags;
            _len = len;
            _data = BitConverter.ToUInt64(data);
        }

        internal CANMsg ToCANMsg() 
            => new(_id, _timestamp, _flags, _len, _data);

        internal static CanMessage FromCANMsg(CANMsg msg) 
            => new(msg.id, msg.timestamp, msg.flags, msg.len, msg.data);

    }
}
