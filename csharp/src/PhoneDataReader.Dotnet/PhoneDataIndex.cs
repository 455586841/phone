using System;
using System.IO;

namespace PhoneDataReader
{
    public class PhoneDataIndex
    {
        public Int32 Number { get; set; }
        public ushort Offset { get; set; }
        public ushort GroupId { get; set; }

        public void Read(Stream stream)
        {
            var buffer = new Byte[8];
            if (stream.Read(buffer, 0, 8) == 8)
            {
                this.Number = BitConverter.ToInt32(buffer, 0);
                this.Offset = BitConverter.ToUInt16(buffer, 4);
                this.GroupId = BitConverter.ToUInt16(buffer, 6);
            }
        }
    }
}