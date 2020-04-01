#include "ChatPacket.h"

ChatPacket::ChatPacket(int8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = PacketTypes::LobbyChat;
}

ChatPacket::ChatPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void ChatPacket::serialize(void* data)
{
	memcpy(&_data[DATA_START_POS], data, sizeof(ChatData));
}

void ChatPacket::deserialize(void* data)
{
	memcpy(data, &_data[DATA_START_POS], sizeof(ChatData));
}
