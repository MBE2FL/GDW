#pragma once
#include "Packet.h"
#include "Scoreboard.h"


struct ScoreData : PacketData
{
	PlayerTime _time;

	ScoreData() {}

	ScoreData(PlayerTime time)
	{
		_time = time;
	}
};

class ScorePacket : public Packet
{
public:
	ScorePacket(int8_t networkID);
	ScorePacket(int8_t networkID, int8_t numScores);
	ScorePacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	virtual void deserialize(void* data) override;
	int8_t getNumScores() const;

private:
	ScorePacket() {};
};
