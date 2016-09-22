<?php

class PhoneDataReader
{
    private $dataFilePath, $header, $stream, $indexFormat, $dataCache, $dataNames;

    public function __construct($dataFilePath)
    {
        $this->dataNames = array(
            "corp",
            "province",
            "city",
            "areaCode",
            "postCode",
            "telecomOperator",
            "virtualNetworkOperator",
            "card"
        );
        $this->dataCache = array();
        $this->indexFormat = implode('/', array(
            'inumber',//号码
            'Soffset',//偏移量
            'SgroupId'//分组ID
        ));
        $this->dataFilePath = $dataFilePath;
        $this->stream = fopen($dataFilePath, 'rb');
    }

    function __destruct()
    {
        fclose($this->stream);
    }

    public function getDataFilePath()
    {
        return $this->dataFilePath;
    }

    public function getHeader()
    {
        if (!$this->header) {
            fseek($this->stream, 0, SEEK_SET);
            $data = fread($this->stream, 26);
            $format = implode('/', array(
                'a4prefix',//前缀
                'itotal',//数据量
                'iindexOffset',//索引区偏移量
                'igroupOffset',//分组区偏移量
                'CgroupItemsCount',//分组数据项目数
                'idataOffset',//数据区偏移量
                'CdataLength',//数据块大小
                'ipubDate'//文件发布日期
            ));
            $this->header = unpack($format, $data);
        }
        return $this->header;
    }

    public function search($phone)
    {
        if (!is_string($phone) || !is_numeric($phone) || strlen($phone) < 7 || strlen($phone) > 11) {
            return false;
        }
        $phoneSection = $phone;
        if (strlen($phoneSection) > 7) {
            $phoneSection = substr($phoneSection, 0, 7);
        }
        $phoneSection = intval($phoneSection);
        $header = $this->getHeader();
        $groupId = $this->searchGroupId($phoneSection, $header['indexOffset'], $header['groupOffset']);
        if ($groupId) {
            $grouData = $this->getGroupDatas($groupId);
            $data = $this->getDataByDataIdList($grouData);
            return $data;
        }
        return false;
    }

    private function searchGroupId($phone, $startOffset, $stopOffset)
    {
        if ($startOffset > $stopOffset) {
            return false;
        }
        $mind = intval(($stopOffset - $startOffset) / 8 / 2) * 8 + $startOffset;
        fseek($this->stream, $mind, SEEK_SET);
        $data = fread($this->stream, 8);
        $index = unpack($this->indexFormat, $data);
        $number = $index['number'];
        $offset = $index['offset'];
        if ($phone >= $number && $phone <= $number + $offset) {
            return $index['groupId'];
        }
        if ($phone > $number) {
            return $this->searchGroupId($phone, $mind + 8, $stopOffset);
        }
        if ($phone < $number) {
            return $this->searchGroupId($phone, $startOffset, $mind - 8);
        }
        return false;
    }

    private function getGroupDatas($groupId)
    {
        $header = $this->getHeader();
        fseek($this->stream, $header["groupOffset"] + $groupId * $header["groupItemsCount"] * 2, SEEK_SET);
        $format = "Sid";
        $array = array();
        for ($i = 0; $i < $header['groupItemsCount']; $i++) {
            $data = fread($this->stream, 2);
            array_push($array, unpack($format, $data)['id']);
        }
        return $array;
    }

    private function getDataByDataIdList($dataIdList)
    {
        $header = $this->getHeader();
        $dataOffset = $header['dataOffset'];
        $dataLength = $header['dataLength'];
        $array = array();

        foreach ($dataIdList as $key => $id) {
            if (!array_key_exists($id, $this->dataCache)) {
                fseek($this->stream, $dataOffset + $id * $dataLength, SEEK_SET);
                $data = fread($this->stream, $dataLength);
                $this->dataCache[$id] = trim($data);
            }
            $array[$this->dataNames[$key]] = $this->dataCache[$id];
        }
        return $array;
    }
}