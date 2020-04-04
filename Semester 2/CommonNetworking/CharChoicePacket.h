#pragma once
#include "Packet.h"

struct CharChoiceData : PacketData
{
	CharacterChoices _charChoice;
};

class CharChoicePacket : public Packet
{
public:
	CharChoicePacket(uint8_t networkID);
	CharChoicePacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	virtual void deserialize(void* data) override;

private:
	CharChoicePacket() {};
};