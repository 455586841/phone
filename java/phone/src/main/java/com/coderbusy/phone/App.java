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
			System.out.println("集团:"+data.getCorp());
			System.out.println("省份:"+data.getProvince());
			System.out.println("城市:"+data.getCity());
			System.out.println("区号:"+data.getAreaCode());
			System.out.println("邮编:"+data.getPostCode());
			System.out.println("运营商:"+data.getTelecomOperator());
			System.out.println("虚拟运营商:"+data.getVirtualNetworkOperator());
			System.out.println("卡类型:"+data.getCard());
		}else{
			System.out.println("数据未找到");
		}
	}
}
