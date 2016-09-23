package com.coderbusy.phone;

import java.io.IOException;
import java.io.RandomAccessFile;
import java.lang.Short;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class PhoneDataReader {
	private RandomAccessFile _file;
	private PhoneDataHeader _header;
	private Map<Integer,String> _dataCache;

	public PhoneDataReader(RandomAccessFile file) {
		this._file = file;
		this._dataCache = new HashMap<Integer,String>();
	}

	public PhoneDataHeader getHeader() throws IOException {
		if (this._header == null) {
			this._header = new PhoneDataHeader();
			this.fullHeaderData(this._header);
		}
		return this._header;
	}

	public PhoneData search(String phone) throws IOException {
		if (phone == null || phone.isEmpty() || phone.length() < 7 || phone.length() > 11) {
			return null;
		}
		String section = phone;
		if (section.length() > 7) {
			section = section.substring(0, 7);
		}
		int number = Integer.parseInt(section);
		PhoneDataHeader header = this.getHeader();
		int groupId = this.searchGroupId(number, header.getIndexOffset(), header.getGroupOffset());
		if (groupId >= 0) {
			List<Integer> groupData = this.getGroupDatas(groupId);
			return this.readData(groupData);
		}
		return null;
	}

	private PhoneData readData(List<Integer> groupData) throws IOException {
		PhoneData data = new PhoneData();
		PhoneDataHeader header = this.getHeader();
		for (int i = 0; i < groupData.size(); i++) {
			int id = groupData.get(i);
			if(!this._dataCache.containsKey(id)){
				this._dataCache.put(id, id+"");
			}
			data.setAreaCode(this._dataCache.get(id));
		}
		return data;
	}

	private int searchGroupId(int phone, int startOffset, int stopOffset) throws IOException {
		if (startOffset <= stopOffset) {
			int mind = (stopOffset - startOffset) / 8 / 2 * 8 + startOffset;
			this._file.seek(mind);
			int number = this.readInt();
			int offset = this.readUnsignedShort();
			int groupId = this.readUnsignedShort();
			if (phone >= number && phone <= number + offset) {
				return groupId;
			}
			if (phone < number) {
				return this.searchGroupId(phone, startOffset, mind - 8);
			}
			if (phone > number) {
				return this.searchGroupId(phone, mind + 8, stopOffset);
			}
		}
		return -1;
	}

	private void fullHeaderData(PhoneDataHeader header) throws IOException {
		this._file.seek(0);
		header.setPrefix(this.readString(4));
		header.setTotal(this.readInt());
		header.setIndexOffset(this.readInt());
		header.setGroupOffset(this.readInt());
		header.setGroupItemsCount(this._file.readByte());
		header.setDataOffset(this.readInt());
		header.setDataLength(this._file.readByte());
		header.setPubDate(this.readInt());
	}

	private List<Integer> getGroupDatas(int groupId) throws IOException {
		List<Integer> list = new ArrayList<Integer>();
		PhoneDataHeader header = this.getHeader();
		this._file.seek(header.getGroupOffset() + groupId * header.getGroupItemsCount() * 2);
		int length = header.getGroupItemsCount();
		for (int i = 0; i < length; i++) {
			list.add(this.readUnsignedShort());
		}
		return list;
	}

	private int readInt() throws IOException {
		byte[] buffer = new byte[4];
		this._file.read(buffer, 0, buffer.length);
		return (buffer[0] & 0xff) | ((buffer[1] << 8) & 0xff00) | ((buffer[2] << 24) >>> 8) | (buffer[3] << 24);
	}

	private int readUnsignedShort() throws IOException {
		return this._file.readUnsignedShort();
	}

	private String readString(int length) throws IOException {
		byte[] buffer = new byte[length];
		this._file.read(buffer, 0, length);
		return new String(buffer);
	}
}
