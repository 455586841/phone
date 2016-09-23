package com.coderbusy.phone;

import java.io.IOException;
import java.io.RandomAccessFile;

/**
 * Hello world!
 *
 */
public class App {
	public static void main(String[] args) throws IOException {
		PhoneDataReader reader = new PhoneDataReader(new RandomAccessFile("D:\\GitHub\\phone\\data\\phone.dat", "r"));
		System.out.println(reader.search("1851105"));
	}
}
