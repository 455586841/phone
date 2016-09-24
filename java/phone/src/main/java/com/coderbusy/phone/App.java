package com.coderbusy.phone;

import java.io.IOException;
import java.io.RandomAccessFile;

/**
 * Hello world!
 *
 */
public class App {
	public static void main(String[] args) throws IOException {
		final String dataHome = "D:\\GitHub\\phone";
		final String dataFilePath = dataHome + "\\data\\phone.dat";
		PhoneDataReader reader = new PhoneDataReader(new RandomAccessFile(dataFilePath, "r"));
		PhoneData data = reader.search("1851105");
		if(data!=null){
			System.out.println("��Ӫ��:"+data.getTelecomOperator());
			System.out.println("ʡ��:"+data.getProvince());
			System.out.println("����:"+data.getCity());
		}else{
			System.out.println("����δ�ҵ�");
		}
	}
}
