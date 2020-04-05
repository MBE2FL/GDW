#pragma once
#include <iostream>
#include <cstdint>

#define BUF_LEN 512
#define PCK_TYPE_POS 0
#define NET_ID_POS 1
//#define EID_POS 2 //TO-DO Completely remove separate obj id
#define DATA_START_POS 2

#define SERVER_ID 255

enum PacketTypes : uint8_t
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
	ClientScoresRequest,
	LobbyChat,
	LobbyTeamName,
	LobbyCharChoice,
	LobbyPlayer,
	OwnershipChange
};

enum Ownership : uint8_t
{
	ClientOwned,
	ServerOwned,
	OtherClientOwned
};

enum CharacterChoices : uint8_t
{
	NoChoice,
	SisterChoice,
	BrotherChoice
};


struct PacketData
{
	uint8_t _entityID;
};


class TransformPacket;
class AnimPacket;


class Packet
{
public:
	//Packet(uint8_t networkID, uint8_t objID);
	Packet(uint8_t networkID);
	virtual ~Packet();
	virtual void serialize(void* data) = 0;
	//virtual void deserialize(uint8_t& objID, void* data) = 0;
	virtual void deserialize(void* data) = 0;
	uint8_t getEID() const;
	void setPacketType(const PacketTypes& packetType);
	void setNetworkID(const uint8_t networkID);

	static Packet* CreatePacket(char buf[BUF_LEN]);
	//static Packet* CreatePacket(MessageTypes pckType, uint8_t networkID, uint8_t objID);

	char _data[BUF_LEN];

protected:
	Packet() {};

private:
	Packet& operator=(const Packet& other) {};
};