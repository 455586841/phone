# 手机号码归属地查询组件

本项目为 [码农很忙淘宝店](https://shop112613936.taobao.com/shop/view_shop.htm?tracelog=twddp&user_number_id=2046715486) 下属商品 [手机号码归属地数据库](https://item.taobao.com/item.htm?id=538955320855) 的附加源码。

本项目实现了手机号码归属地的快速查询功能，且数据文件超小，适合附带在网络分发或大量查询的场景中。

# PHP版本使用帮助

### 准备工作

- 引入 PhoneDataReader.class.php 文件
- 初始化 PhoneDataReader 传入 phone.dat 所在路径

### 搜索数据


通过执行 实例的 search 方法来搜索数据，如果搜索结果为空，则search方法返回false。

```php
<?php
include('PhoneDataReader.class.php');
$dateFilePath = dirname(__DIR__) . DIRECTORY_SEPARATOR . 'data' . DIRECTORY_SEPARATOR . 'phone.dat';
$phoneDataReader = new PhoneDataReader($dateFilePath);
var_dump($phoneDataReader->search('1851105'));

```

### 获取记录数

执行 实例的 getHeader 方法可以获得 phone.dat 文件的文件头信息。改方法返回一个字典，名为 total 的键值对代表总记录数。
```php
<?php
include('PhoneDataReader.class.php');
$dateFilePath = dirname(__DIR__) . DIRECTORY_SEPARATOR . 'data' . DIRECTORY_SEPARATOR . 'phone.dat';
$phoneDataReader = new PhoneDataReader($dateFilePath);
var_dump($phoneDataReader->getHeader()['total']);

```