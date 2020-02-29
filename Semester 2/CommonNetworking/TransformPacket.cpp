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
	TransformData transData = reinterpret_cast<TransformData&>(data);

	//memset(_data, 0, BUF_LEN);

	size_t posSize = sizeof(float) * 3;
	size_t rotSize = sizeof(float) * 4;

	memcpy(&_data[DATA_START_POS], reinterpret_cast<char*>(&transData._pos._x), posSize);
	memcpy(&_data[DATA_START_POS + posSize], reinterpret_cast<char*>(&transData._rot._x), rotSize);
}

void TransformPacket::deserialize(void* data)
{
	size_t posSize = sizeof(float) * 3;
	size_t rotSize = sizeof(float) * 4;
	TransformData* transData = reinterpret_cast<TransformData*>(data);

	memcpy(&transData->_pos._x, reinterpret_cast<float*>(&_data[DATA_START_POS]), posSize);
	memcpy(&transData->_rot._x, reinterpret_cast<float*>(&_data[DATA_START_POS + posSize]), rotSize);
}
