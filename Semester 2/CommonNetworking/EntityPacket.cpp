#include "EntityPacket.h"

//EntityPacket::EntityPacket(int8_t networkID, int8_t objID)
//	: Packet(networkID, objID)
//{
//	_data[PCK_TYPE_POS] = MessageTypes::EntitiesStart;
//}

EntityPacket::EntityPacket(int8_t networkID)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = PacketTypes::EntitiesStart;
}

EntityPacket::EntityPacket(PacketTypes pckType, int8_t networkID, int8_t numEntities)
	: Packet(networkID)
{
	_data[PCK_TYPE_POS] = pckType;
	_data[DATA_START_POS] = numEntities;
}

EntityPacket::EntityPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void EntityPacket::serialize(void* data)
{
	memcpy(&_data[DATA_START_POS + 1], data, sizeof(EntityData) * _data[DATA_START_POS]);
}

//void EntityPacket::deserialize(int8_t& numEntities, void* data)
//{
//	memcpy(data, &_data[DATA_START_POS + 1], sizeof(EntityData) * numEntities);
//}

void EntityPacket::deserialize(void* data)
{
	memcpy(data, &_data[DATA_START_POS + 1], sizeof(EntityData) * _data[DATA_START_POS]);
}

int8_t EntityPacket::getNumEntities() const
{
	return _data[DATA_START_POS];
}
