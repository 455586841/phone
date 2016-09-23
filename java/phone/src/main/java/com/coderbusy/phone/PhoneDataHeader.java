package com.coderbusy.phone;

import java.io.IOException;
import java.io.RandomAccessFile;

public class PhoneDataHeader {
	private String prefix;
	private int total;
	private int indexOffset;
	private int groupOffset;
	private byte groupItemsCount;
	private int dataOffset;
	private byte dataLength;
	private int pubDate;

	public String getPrefix() {
		return prefix;
	}

	public void setPrefix(String prefix) {
		this.prefix = prefix;
	}

	public int getTotal() {
		return total;
	}

	public void setTotal(int total) {
		this.total = total;
	}

	public int getIndexOffset() {
		return indexOffset;
	}

	public void setIndexOffset(int indexOffset) {
		this.indexOffset = indexOffset;
	}

	public int getGroupOffset() {
		return groupOffset;
	}

	public void setGroupOffset(int groupOffset) {
		this.groupOffset = groupOffset;
	}

	public byte getGroupItemsCount() {
		return groupItemsCount;
	}

	public void setGroupItemsCount(byte groupItemsCount) {
		this.groupItemsCount = groupItemsCount;
	}

	public int getDataOffset() {
		return dataOffset;
	}

	public void setDataOffset(int dataOffset) {
		this.dataOffset = dataOffset;
	}

	public byte getDataLength() {
		return dataLength;
	}

	public void setDataLength(byte dataLength) {
		this.dataLength = dataLength;
	}

	public int getPubDate() {
		return pubDate;
	}

	public void setPubDate(int pubDate) {
		this.pubDate = pubDate;
	}

}
