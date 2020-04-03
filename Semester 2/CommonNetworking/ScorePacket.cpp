#include "ScorePacket.h"

ScorePacket::ScorePacket(int8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = PacketTypes::Score;
}

ScorePacket::ScorePacket(int8_t networkID, int8_t numScores)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = PacketTypes::Score;
	_data[DATA_START_POS] = numScores;
}

ScorePacket::ScorePacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void ScorePacket::serialize(void* data)
{
	memcpy(&_data[DATA_START_POS + 1], data, sizeof(ScoreData) * _data[DATA_START_POS]);
}

void ScorePacket::deserialize(void* data)
{
	memcpy(data, &_data[DATA_START_POS + 1], sizeof(ScoreData) * _data[DATA_START_POS]);
}

int8_t ScorePacket::getNumScores() const
{
	return _data[DATA_START_POS];
}
