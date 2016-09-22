<?php
include('PhoneDataReader.class.php');
$dateFilePath = dirname(__DIR__) . DIRECTORY_SEPARATOR . 'data' . DIRECTORY_SEPARATOR . 'phone.dat';
$phoneDataReader = new PhoneDataReader($dateFilePath);
var_dump($phoneDataReader->search('1851105'));