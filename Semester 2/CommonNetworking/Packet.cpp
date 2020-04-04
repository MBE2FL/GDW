#include "Packet.h"
#include "TransformPacket.h"
#include "AnimPacket.h"


//Packet::Packet(uint8_t networkID, uint8_t objID)
//{
//	memset(_data, 0, BUF_LEN);
//
//	_data[NET_ID_POS] = networkID;
//	_data[EID_POS] = objID;
//}

Packet::Packet(uint8_t networkID)
{
	memset(_data, 0, BUF_LEN);

	_data[NET_ID_POS] = networkID;
}

Packet::~Packet()
{
}

uint8_t Packet::getEID() const
{
	return _data[DATA_START_POS];
}

void Packet::setPacketType(const PacketTypes& packetType)
{
	_data[PCK_TYPE_POS] = packetType;
}

Packet* Packet::CreatePacket(char data[BUF_LEN])
{
	PacketTypes pckType = static_cast<PacketTypes>(data[0]);

	Packet* packet = nullptr;

	switch (pckType)
	{
	case ConnectionAttempt:
		break;
	case ConnectionAccepted:
		break;
	case ConnectionFailed:
		break;
	case ServerFull:
		break;
	case TransformMsg:
	{
		packet = new TransformPacket(data);
	}
		break;
	case Anim:
	{
		packet = new AnimPacket(data);
	}
		break;
	default:
		break;
	}

	return packet;
}
