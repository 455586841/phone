using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhoneDataReader
{
    public class PhoneDataReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly Dictionary<ushort, String> _dataCache = new Dictionary<ushort, string>();

        private Header _header;

        public PhoneDataReader(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            _stream = stream;
        }

        public PhoneDataReader(string filename) : this(File.OpenRead(filename))
        {
        }

        public void Dispose()
        {
        }

        public Header GetHeader()
        {
            if (_header == null)
            {
                _header = new Header();
                _stream.Seek(0, SeekOrigin.Begin);
                Byte[] buffer = new byte[26];
                _stream.Read(buffer, 0, 26);
                _header.Prefix = _encoding.GetString(buffer, 0, 4);
                _header.Total = BitConverter.ToInt32(buffer, 4);
                _header.IndexOffset = BitConverter.ToInt32(buffer, 8);
                _header.GroupOffset = BitConverter.ToInt32(buffer, 12);
                _header.GroupItemsCount = buffer[16];
                _header.DataOffset = BitConverter.ToInt32(buffer, 17);
                _header.DataLength = buffer[21];
                _header.PubDate = BitConverter.ToInt32(buffer, 22);
            }
            return _header;
        }

        public List<string> Search(string phone)
        {
            if (string.IsNullOrEmpty(phone) || (phone.Length < 7) || (phone.Length > 11))
                throw new ArgumentNullException(nameof(phone));
            var phoneSection = phone;
            if (phoneSection.Length > 7)
            {
                phoneSection = phone.Substring(0, 7);
            }
            Int32 number;
            if (Int32.TryParse(phoneSection, out number))
            {
                var header = this.GetHeader();
                var groupId = this.SearchGroupId(number, header.IndexOffset, header.GroupOffset);
                if (groupId != null)
                {
                    var dataIdList = this.GetGroupDatas(groupId.Value);
                    return this.GetDatas(dataIdList);
                }
            }
            else
            {
                throw new ArgumentException("手机号码格式不正确", nameof(phone));
            }
            return null;
        }

        private ushort? SearchGroupId(Int32 phone, Int32 startOffset, Int32 stopOffset)
        {
            if (startOffset > stopOffset)
            {
                return null;
            }
            var mind = (stopOffset - startOffset) / 8 / 2 * 8 + startOffset;
            _stream.Seek(mind, SeekOrigin.Begin);
            var index = new Index();
            index.Read(_stream);
            if (phone >= index.Number && phone <= index.Number + index.Offset)
            {
                return index.GroupId;
            }
            if (phone < index.Number)
            {
                return SearchGroupId(phone, startOffset, mind - 8);
            }
            if (phone > index.Number)
            {
                return SearchGroupId(phone, mind + 8, stopOffset);
            }
            return null;
        }

        private List<ushort> GetGroupDatas(UInt16 groupId)
        {
            var header = this.GetHeader();
            _stream.Seek(header.GroupOffset + groupId * header.GroupItemsCount * 2, SeekOrigin.Begin);
            var length = _header.GroupItemsCount;
            var list = new List<ushort>();
            var buffer = new Byte[length * 2];
            _stream.Read(buffer, 0, buffer.Length);
            for (int i = 0; i < length; i++)
            {
                list.Add(BitConverter.ToUInt16(buffer, i * 2));
            }
            return list;
        }

        private List<String> GetDatas(List<ushort> dataIdList)
        {
            var names = new List<String>();
            var header = this.GetHeader();
            foreach (var id in dataIdList)
            {
                if (!_dataCache.ContainsKey(id))
                {
                    _stream.Seek(header.DataOffset + id * header.DataLength, SeekOrigin.Begin);
                    var buffer = new Byte[header.DataLength];
                    _stream.Read(buffer, 0, buffer.Length);
                    var name = _encoding.GetString(buffer).Trim('\0');
                    _dataCache.Add(id, name);
                }
                names.Add(_dataCache[id]);
            }

            return names;
        }

        public class Header
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
        public class Index
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
}