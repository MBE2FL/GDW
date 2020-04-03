#pragma once
#include "Packet.h"

struct ChatData : PacketData
{
	int8_t _msgSize;
	//const char* _msg;
	char* _msg;
	//char _msg[256];
};

class ChatPacket : public Packet
{
public:
	ChatPacket(int8_t networkID);
	ChatPacket(PacketTypes pckType, int8_t networkID);
	ChatPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	virtual void deserialize(void* data) override;

private:
	ChatPacket() {};
};