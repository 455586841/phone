import sys
import os.path
import struct

class PhoneDataReader:
    __dataFilePath__ = ''
    __header__ = None
    __stream__ = None
    __dataNames__ = None
    __dataCache__ = None
    def __init__(self,dataFilePath):
        self.__dataFilePath__ = dataFilePath
        self.__stream__ = open(self.__dataFilePath__,'rb')
        self.__dataNames__ = ['corp','province','city','areaCode','postCode','telecomOperator','virtualNetworkOperator','card']
        self.__dataCache__ = {}
    def getDataFilePath(self):
        return self.__dataFilePath__
    def getHeader(self):
        if self.__header__ == None :
            self.__header__ = {}
            self.__stream__.seek(0,0)
            data = self.__stream__.read(26)
            prefix,total,indexOffset,groupOffset,groupItemsCount,dataOffset,dataLength,pubDate = struct.unpack('<4siiiBiBi',data)
            self.__header__['prefix'] = prefix
            self.__header__['total'] = total
            self.__header__['indexOffset'] = indexOffset
            self.__header__['groupOffset'] = groupOffset
            self.__header__['groupItemsCount'] = groupItemsCount
            self.__header__['dataOffset'] = dataOffset
            self.__header__['dataLength'] = dataLength
            self.__header__['pubDate'] = pubDate
        return self.__header__
    def search(self,phone):
        if phone == None or isinstance(phone,str) == False or phone.isdigit() == False or len( phone) < 7 or len( phone) > 11:
            return False
        section = phone
        if len(section) > 7:
            section = section[0:7]
        section = int(section)
        header = self.getHeader()
        groupId = self.__searchGroupId__(section,header['indexOffset'],header['groupOffset'])
        if groupId != False:
            groupData = self.__getGroupDatas__(groupId)
            resut = self.__getDataByDataIdList__(groupData)
            return resut
        return False
    def __searchGroupId__(self,phone,startOffset,stopOffset):
        if startOffset <= stopOffset :
            mind = int((stopOffset - startOffset) / 8 / 2) * 8 + startOffset
            self.__stream__.seek(mind,0)
            data = self.__stream__.read(8)
            number,offset,groupId = struct.unpack('<iHH',data)
            if phone >= number and phone <= number + offset :
                return groupId
            elif phone > number :
                return self.__searchGroupId__(phone , mind + 8 , stopOffset )
            elif phone < number :
                return self.__searchGroupId__(phone , startOffset , mind - 8)
        return False
    def __getGroupDatas__(self, groupId):
        header = self.getHeader()
        result = []
        self.__stream__.seek(header["groupOffset"] + groupId * header["groupItemsCount"] * 2,0)
        for i in range(header['groupItemsCount']):
            data = self.__stream__.read(2)
            data = struct.unpack('<H',data)
            result.append(data[0])
        return result
    def __getDataByDataIdList__(self,dataIdList):
        header = self.getHeader()
        dataOffset = header['dataOffset']
        dataLength = header['dataLength']
        result = {}
        for i in range(len(dataIdList)):
            id = dataIdList[i]
            if self.__dataCache__.get(id) == None :
                self.__stream__.seek(dataOffset + id * dataLength,0)
                data = self.__stream__.read(dataLength)
                data = data.decode()
                data = data.split('\x00')[0]
                self.__dataCache__[id] = data
            result[self.__dataNames__[i]]=self.__dataCache__[id]
        return result

if __name__ == '__main__' :
    dataFilePath = os.path.abspath(__file__)
    dataFilePath = os.path.dirname(dataFilePath)
    dataFilePath = os.path.dirname(dataFilePath)
    dataFilePath += os.path.sep + 'data' + os.path.sep + 'phone.dat'
    reader = PhoneDataReader(dataFilePath)
    print(reader.search('1851105'))