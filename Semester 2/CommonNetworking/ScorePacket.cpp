#include "ScorePacket.h"

ScorePacket::ScorePacket(uint8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = PacketTypes::Score;
}

ScorePacket::ScorePacket(uint8_t networkID, uint8_t numScores)
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
	//memcpy(&_data[DATA_START_POS + 1], data, sizeof(ScoreData) * _data[DATA_START_POS]);

	ScoreData* scoreData = reinterpret_cast<ScoreData*>(data);
	int offset = 0;

	memcpy(&_data[DATA_START_POS], &scoreData->_entityID, 1);
	offset += 1;

	memcpy(&_data[DATA_START_POS + offset], &scoreData->_nameSize, 1);
	offset += 1;

	memcpy(&_data[DATA_START_POS + offset], scoreData->teamName, scoreData->_nameSize);
	offset += scoreData->_nameSize + 1;

	memcpy(&_data[DATA_START_POS + offset], &scoreData->minutes, sizeof(int));
	offset += sizeof(int);

	memcpy(&_data[DATA_START_POS + offset], &scoreData->seconds, sizeof(float));

	std::cout << "Score: EID: " << static_cast<int>(scoreData->_entityID) << 
		", name size: " << static_cast<int>(scoreData->_nameSize) <<
		", " << scoreData->teamName << 
		" time: " << scoreData->minutes << "min, " << scoreData->minutes << "sec" << std::endl;
}

void ScorePacket::deserialize(void* data)
{
	//memcpy(data, &_data[DATA_START_POS + 1], sizeof(ScoreData) * _data[DATA_START_POS]);

	ScoreData* scoreData = reinterpret_cast<ScoreData*>(data);
	int offset = 0;

	memcpy(&scoreData->_entityID, &_data[DATA_START_POS], 1);
	offset += 1;

	memcpy(&scoreData->_nameSize, &_data[DATA_START_POS + offset], 1);
	offset += 1;

	scoreData->teamName = new char[scoreData->_nameSize + 1];
	memcpy(scoreData->teamName, &_data[DATA_START_POS + offset], scoreData->_nameSize + 1);
	offset += scoreData->_nameSize + 1;

	memcpy(&scoreData->minutes, &_data[DATA_START_POS + offset], sizeof(int));
	offset += sizeof(int);

	memcpy(&scoreData->seconds, &_data[DATA_START_POS + offset], sizeof(float));
}

uint8_t ScorePacket::getNumScores() const
{
	return _data[DATA_START_POS];
}
