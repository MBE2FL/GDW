#include "ChatPacket.h"

ChatPacket::ChatPacket(uint8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = PacketTypes::LobbyChat;
}

ChatPacket::ChatPacket(PacketTypes pckType, uint8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = pckType;
}

ChatPacket::ChatPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void ChatPacket::serialize(void* data)
{
	ChatData* chatData = reinterpret_cast<ChatData*>(data);

	memcpy(&_data[DATA_START_POS], &chatData->_entityID, 1);
	memcpy(&_data[DATA_START_POS + 1], &chatData->_msgSize, 1);
	memcpy(&_data[DATA_START_POS + 2], chatData->_msg, chatData->_msgSize);
}

void ChatPacket::deserialize(void* data)
{
	//memcpy(data, &_data[DATA_START_POS], sizeof(ChatData));

	ChatData* chatData = reinterpret_cast<ChatData*>(data);

	memcpy(&chatData->_entityID, &_data[DATA_START_POS], 1);
	memcpy(&chatData->_msgSize, &_data[DATA_START_POS + 1], 1);

	chatData->_msg = new char[chatData->_msgSize + 1];
	memcpy(chatData->_msg, &_data[DATA_START_POS + 2], chatData->_msgSize + 1);
}
