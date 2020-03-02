#include "Packet.h"
#include "TransformPacket.h"
#include "AnimPacket.h"


Packet::Packet(int8_t networkID, int8_t objID)
{
	memset(_data, 0, BUF_LEN);

	_data[NET_ID_POS] = networkID;
	_data[OBJ_ID_POS] = objID;
}

Packet::~Packet()
{
}

Packet* Packet::CreatePacket(char data[BUF_LEN])
{
	MessageTypes msgType = static_cast<MessageTypes>(data[0]);

	Packet* packet = nullptr;

	switch (msgType)
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
