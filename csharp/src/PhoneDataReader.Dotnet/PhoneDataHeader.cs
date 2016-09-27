using System;

namespace PhoneDataReader
{
    public class PhoneDataHeader
    {
        public string Prefix { get; set; }
        public int Total { get; set; }
        public int IndexOffset { get; set; }
        public int GroupOffset { get; set; }
        public Byte GroupItemsCount { get; set; }
        public int DataOffset { get; set; }
        public byte DataLength { get; set; }
        public int PubDate { get; set; }
    }
}