#pragma once
#include <iostream>
#include <cstdint>

#define BUF_LEN 512
#define MSG_TYPE_POS 0
#define NET_ID_POS 1
#define OBJ_ID_POS 2
#define DATA_START_POS 3

enum MessageTypes : int8_t
{
	ConnectionAttempt,
	ConnectionAccepted,
	ConnectionFailed,
	ServerFull,
	TransformMsg,
	Anim
};


class TransformPacket;
class AnimPacket;


class Packet
{
public:
	Packet(int8_t networkID, int8_t objID);
	virtual ~Packet();
	virtual void serialize(void* data) = 0;
	virtual void deserialize(int8_t& objID, void* data) = 0;

	static Packet* CreatePacket(char buf[BUF_LEN]);
	//static Packet* CreatePacket(MessageTypes msgType, int8_t networkID, int8_t objID);

	char _data[BUF_LEN];

protected:
	Packet() {};

private:
	Packet& operator=(const Packet& other) {};
};