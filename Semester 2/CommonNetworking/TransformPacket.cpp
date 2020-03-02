#include "TransformPacket.h"

TransformPacket::TransformPacket(int8_t networkID, int8_t objID)
	: Packet(networkID, objID)
{
	_data[MSG_TYPE_POS] = MessageTypes::TransformMsg;
}

TransformPacket::TransformPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void TransformPacket::serialize(void* data)
{
	float* floatData = reinterpret_cast<float*>(data);

	//Vector3 pos;
	//Quaternion rot;

	//memcpy(&pos._x, floatData, sizeof(float) * 3);
	//memcpy(&rot._x, &floatData[3], sizeof(float) * 4);

	//TransformData transData = TransformData();
	//transData._pos = pos;
	//transData._rot = rot;

	size_t posSize = sizeof(float) * 3;
	size_t rotSize = sizeof(float) * 4;

	memcpy(&_data[DATA_START_POS], reinterpret_cast<char*>(floatData), posSize);
	memcpy(&_data[DATA_START_POS + posSize], reinterpret_cast<char*>(&floatData[3]), rotSize);

	floatData = nullptr;
}

void TransformPacket::deserialize(int8_t& objID, void* data)
{
	//int8_t networkID = _data[NET_ID_POS];
	//objID = _data[OBJ_ID_POS];


	size_t posSize = sizeof(float) * 3;
	size_t rotSize = sizeof(float) * 4;
	TransformData* transData = reinterpret_cast<TransformData*>(data);

	transData->_objID = _data[OBJ_ID_POS];
	memcpy(&transData->_pos._x, reinterpret_cast<float*>(&_data[DATA_START_POS]), posSize);
	memcpy(&transData->_rot._x, reinterpret_cast<float*>(&_data[DATA_START_POS + posSize]), rotSize);
}
