#include "AnimPacket.h"

AnimPacket::AnimPacket(int8_t networkID, int8_t objID)
	: Packet(networkID, objID)
{
	_data[MSG_TYPE_POS] = MessageTypes::Anim;
}

AnimPacket::AnimPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void AnimPacket::serialize(void* data)
{
	size_t animStateSize = sizeof(int);
	AnimData animData = reinterpret_cast<AnimData&>(data);
	memcpy(&_data, reinterpret_cast<char*>(&animData._state), animStateSize);
}

void AnimPacket::deserialize(int8_t& objID, void* data)
{
	//objID = _data[OBJ_ID_POS];

	size_t animStateSize = sizeof(int);
	AnimData* animData = reinterpret_cast<AnimData*>(data);
	animData->_objID = objID;
	memcpy(&animData->_state, reinterpret_cast<int*>(&_data[DATA_START_POS]), animStateSize);
}
