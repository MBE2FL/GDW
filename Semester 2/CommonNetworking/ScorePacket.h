#pragma once
#include "Packet.h"
#include "Scoreboard.h"


struct ScoreData : PacketData
{
	uint8_t _nameSize;
	char* teamName;
	int minutes;
	float seconds;
};

class ScorePacket : public Packet
{
public:
	ScorePacket(uint8_t networkID);
	ScorePacket(uint8_t networkID, uint8_t numScores);
	ScorePacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	virtual void deserialize(void* data) override;
	uint8_t getNumScores() const;

private:
	ScorePacket() {};
};
