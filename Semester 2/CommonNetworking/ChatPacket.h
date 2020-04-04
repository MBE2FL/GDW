#pragma once
#include "Packet.h"

struct ChatData : PacketData
{
	uint8_t _msgSize;
	//const char* _msg;
	char* _msg;
	//char _msg[256];
};

class ChatPacket : public Packet
{
public:
	ChatPacket(uint8_t networkID);
	ChatPacket(PacketTypes pckType, uint8_t networkID);
	ChatPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	virtual void deserialize(void* data) override;

private:
	ChatPacket() {};
};