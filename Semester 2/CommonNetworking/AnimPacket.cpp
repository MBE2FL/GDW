#include "AnimPacket.h"

//AnimPacket::AnimPacket(int8_t networkID, int8_t objID)
//	: Packet(networkID, objID)
//{
//	_data[PCK_TYPE_POS] = MessageTypes::Anim;
//}

AnimPacket::AnimPacket(int8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = PacketTypes::Anim;
}

AnimPacket::AnimPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void AnimPacket::serialize(void* data)
{
	memcpy(&_data[DATA_START_POS], data, sizeof(AnimData));
}

void AnimPacket::deserialize(void* data)
{
	memcpy(data, &_data[DATA_START_POS], sizeof(AnimData));
}

//void AnimPacket::deserialize(int8_t& objID, void* data)
//{
//	//objID = _data[OBJ_ID_POS];
//
//	size_t animStateSize = sizeof(int);
//	AnimData* animData = reinterpret_cast<AnimData*>(data);
//	animData->_entityID = objID;
//	memcpy(&animData->_state, reinterpret_cast<int*>(&_data[DATA_START_POS]), animStateSize);
//}
