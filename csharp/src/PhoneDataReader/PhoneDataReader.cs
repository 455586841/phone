using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhoneDataReader
{
    public class PhoneDataReader : IDisposable
    {
        private readonly Dictionary<ushort, string> _dataCache = new Dictionary<ushort, string>();
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly Stream _stream;

        private PhoneDataHeader _phoneDataHeader;

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

        public PhoneDataHeader GetHeader()
        {
            if (_phoneDataHeader == null)
            {
                _phoneDataHeader = new PhoneDataHeader();
                _stream.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[26];
                _stream.Read(buffer, 0, 26);
                _phoneDataHeader.Prefix = _encoding.GetString(buffer, 0, 4);
                _phoneDataHeader.Total = BitConverter.ToInt32(buffer, 4);
                _phoneDataHeader.IndexOffset = BitConverter.ToInt32(buffer, 8);
                _phoneDataHeader.GroupOffset = BitConverter.ToInt32(buffer, 12);
                _phoneDataHeader.GroupItemsCount = buffer[16];
                _phoneDataHeader.DataOffset = BitConverter.ToInt32(buffer, 17);
                _phoneDataHeader.DataLength = buffer[21];
                _phoneDataHeader.PubDate = BitConverter.ToInt32(buffer, 22);
            }
            return _phoneDataHeader;
        }

        public PhoneData Search(string phone)
        {
            if (string.IsNullOrEmpty(phone) || (phone.Length < 7) || (phone.Length > 11))
                throw new ArgumentNullException(nameof(phone));
            var phoneSection = phone;
            if (phoneSection.Length > 7)
            {
                phoneSection = phone.Substring(0, 7);
            }
            int number;
            if (int.TryParse(phoneSection, out number))
            {
                var header = GetHeader();
                var groupId = SearchGroupId(number, header.IndexOffset, header.GroupOffset);
                if (groupId != null)
                {
                    var dataIdList = GetGroupDatas(groupId.Value);
                    var data = GetDatas(dataIdList);
                    var result = new PhoneData();
                    for (var i = 0; i < data.Count; i++)
                    {
                        var value = data[i];
                        switch (i)
                        {
                            case 0:
                                result.Corp = value;
                                break;
                            case 1:
                                result.Province = value;
                                break;
                            case 2:
                                result.City = value;
                                break;
                            case 3:
                                result.AreaCode = value;
                                break;
                            case 4:
                                result.PostCode = value;
                                break;
                            case 5:
                                result.TelecomOperator = value;
                                break;
                            case 6:
                                result.VirtualNetworkOperator = value;
                                break;
                            case 7:
                                result.Card = value;
                                break;
                        }
                    }
                    return result;
                }
            }
            else
            {
                throw new ArgumentException("手机号码格式不正确", nameof(phone));
            }
            return null;
        }

        private ushort? SearchGroupId(int phone, int startOffset, int stopOffset)
        {
            if (startOffset > stopOffset)
            {
                return null;
            }
            var mind = (stopOffset - startOffset)/8/2*8 + startOffset;
            _stream.Seek(mind, SeekOrigin.Begin);
            var index = new PhoneDataIndex();
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

        private List<ushort> GetGroupDatas(ushort groupId)
        {
            var header = GetHeader();
            _stream.Seek(header.GroupOffset + groupId*header.GroupItemsCount*2, SeekOrigin.Begin);
            var length = _phoneDataHeader.GroupItemsCount;
            var list = new List<ushort>();
            var buffer = new byte[length*2];
            _stream.Read(buffer, 0, buffer.Length);
            for (var i = 0; i < length; i++)
            {
                list.Add(BitConverter.ToUInt16(buffer, i*2));
            }
            return list;
        }

        private List<string> GetDatas(List<ushort> dataIdList)
        {
            var names = new List<string>();
            var header = GetHeader();
            foreach (var id in dataIdList)
            {
                if (!_dataCache.ContainsKey(id))
                {
                    _stream.Seek(header.DataOffset + id*header.DataLength, SeekOrigin.Begin);
                    var buffer = new byte[header.DataLength];
                    _stream.Read(buffer, 0, buffer.Length);
                    var name = _encoding.GetString(buffer).Trim('\0');
                    _dataCache.Add(id, name);
                }
                names.Add(_dataCache[id]);
            }

            return names;
        }
    }
}