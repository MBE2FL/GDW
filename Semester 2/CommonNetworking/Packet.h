#pragma once
#include <iostream>
#include <cstdint>

#define BUF_LEN 512
#define PCK_TYPE_POS 0
#define NET_ID_POS 1
//#define EID_POS 2 //TO-DO Completely remove separate obj id
#define DATA_START_POS 3

enum PacketTypes : int8_t
{
	ConnectionAttempt,
	ConnectionAccepted,
	ConnectionFailed,
	ServerFull,
	TransformMsg,
	Anim,
	EntitiesQuery,
	EntitiesStart,
	EntitiesNoStart,
	EntitiesRequired,
	EntitiesUpdate,
	EntityIDs,
	EmptyMsg,
	ErrorMsg,
	Score,
	ClientScoresRequest
};

struct PacketData
{
	int8_t _entityID;
};


class TransformPacket;
class AnimPacket;


class Packet
{
public:
	//Packet(int8_t networkID, int8_t objID);
	Packet(int8_t networkID);
	virtual ~Packet();
	virtual void serialize(void* data) = 0;
	//virtual void deserialize(int8_t& objID, void* data) = 0;
	virtual void deserialize(void* data) = 0;
	int8_t getEID() const;
	void setPacketType(const PacketTypes& packetType);

	static Packet* CreatePacket(char buf[BUF_LEN]);
	//static Packet* CreatePacket(MessageTypes pckType, int8_t networkID, int8_t objID);

	char _data[BUF_LEN];

protected:
	Packet() {};

private:
	Packet& operator=(const Packet& other) {};
};