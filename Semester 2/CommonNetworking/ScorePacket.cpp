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

	memcpy(&_data[DATA_START_POS], &scoreData->_entityID, 1);
	memcpy(&_data[DATA_START_POS + 1], &scoreData->_nameSize, 1);
	memcpy(&_data[DATA_START_POS + 2], &scoreData->_time.teamName, scoreData->_nameSize);
	memcpy(&_data[DATA_START_POS + 2 + scoreData->_nameSize + 1], &scoreData->_time.totalTime, sizeof(Time));

	std::cout << "Score: EID: " << static_cast<int>(scoreData->_entityID) << 
		", name size: " << static_cast<int>(scoreData->_nameSize) <<
		", " << scoreData->_time.teamName << 
		" time: " << scoreData->_time.totalTime.minutes << "min, " << scoreData->_time.totalTime.minutes << "sec" << std::endl;
}

void ScorePacket::deserialize(void* data)
{
	//memcpy(data, &_data[DATA_START_POS + 1], sizeof(ScoreData) * _data[DATA_START_POS]);

	ScoreData* scoreData = reinterpret_cast<ScoreData*>(data);
	scoreData->_time = PlayerTime();
	scoreData->_time.totalTime = Time();

	memcpy(&scoreData->_entityID, &_data[DATA_START_POS], 1);
	memcpy(&scoreData->_nameSize, &_data[DATA_START_POS + 1], 1);

	scoreData->_time.teamName = new char[scoreData->_nameSize + 1];
	memcpy(scoreData->_time.teamName, &_data[DATA_START_POS + 2], scoreData->_nameSize + 1);

	memcpy(&scoreData->_time.totalTime, &_data[DATA_START_POS + 2 + scoreData->_nameSize + 1], sizeof(Time));
}

uint8_t ScorePacket::getNumScores() const
{
	return _data[DATA_START_POS];
}
