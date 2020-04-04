#include "TransformPacket.h"

//TransformPacket::TransformPacket(uint8_t networkID, uint8_t objID)
//	: Packet(networkID, objID)
//{
//	_data[PCK_TYPE_POS] = MessageTypes::TransformMsg;
//}

TransformPacket::TransformPacket(uint8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = PacketTypes::TransformMsg;
}

TransformPacket::TransformPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void TransformPacket::serialize(void* data)
{
	memcpy(&_data[DATA_START_POS], data, sizeof(TransformData));
}

void TransformPacket::deserialize(void* data)
{
	memcpy(data, &_data[DATA_START_POS], sizeof(TransformData));
}

//void TransformPacket::deserialize(uint8_t& objID, void* data)
//{
//	//uint8_t networkID = _data[NET_ID_POS];
//	//objID = _data[OBJ_ID_POS];
//
//
//	size_t posSize = sizeof(float) * 3;
//	size_t rotSize = sizeof(float) * 4;
//	TransformData* transData = reinterpret_cast<TransformData*>(data);
//
//	transData->_entityID = _data[EID_POS];
//	memcpy(&transData->_pos._x, reinterpret_cast<float*>(&_data[DATA_START_POS]), posSize);
//	memcpy(&transData->_rot._x, reinterpret_cast<float*>(&_data[DATA_START_POS + posSize]), rotSize);
//}
