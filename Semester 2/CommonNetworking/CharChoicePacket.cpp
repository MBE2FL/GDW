#include "CharChoicePacket.h"

CharChoicePacket::CharChoicePacket(uint8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = LobbyCharChoice;
}

CharChoicePacket::CharChoicePacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void CharChoicePacket::serialize(void* data)
{
	memcpy(&_data[DATA_START_POS], data, sizeof(CharChoiceData));
}

void CharChoicePacket::deserialize(void* data)
{
	memcpy(data, &_data[DATA_START_POS], sizeof(CharChoiceData));
}
