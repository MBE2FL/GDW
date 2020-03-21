#include "EntityPacket.h"

EntityPacket::EntityPacket(int8_t networkID, int8_t objID)
	: Packet(networkID, objID)
{
	_data[MSG_TYPE_POS] = MessageTypes::EntitiesStart;
}

EntityPacket::EntityPacket(MessageTypes msgType, int8_t networkID, int8_t numEntities)
	: Packet(networkID, 0)
{
	_data[MSG_TYPE_POS] = msgType;
	_data[DATA_START_POS] = numEntities;
}

EntityPacket::EntityPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void EntityPacket::serialize(void* data)
{
	memcpy(&_data[DATA_START_POS + 1], data, sizeof(EntityData) *_data[DATA_START_POS]);
}

void EntityPacket::deserialize(int8_t& numEntities, void* data)
{
	//EntityData* entityData = reinterpret_cast<EntityData*>(data);

	//EntityData currEntity;

	//numEntities = _data[DATA_START_POS];


	//memcpy(entityData->_entityIDs, reinterpret_cast<int8_t*>(&_data[DATA_START_POS + 1]), entityData->_numEntities);
	//memcpy(entityData->_entityPrefabTypes, reinterpret_cast<int8_t*>(&_data[DATA_START_POS + 1 + entityData->_numEntities]), entityData->_numEntities);

	memcpy(data, &_data[DATA_START_POS + 1], sizeof(EntityData) * numEntities);

	//for (int i = 0; i < numEntities; ++i)
	//{
	//	memcpy(&currEntity._entityID, reinterpret_cast<int8_t*>(&_data[DATA_START_POS + 1]), 1);
	//	memcpy(&currEntity._entityPrefabType, reinterpret_cast<int8_t*>(&_data[DATA_START_POS + 1 + numEntities]), 1);
	//	memcpy(&currEntity._ownership, reinterpret_cast<int8_t*>(&_data[DATA_START_POS + 2 + numEntities]), 1);

	//	entityData[i] = currEntity;
	//}
}

int8_t EntityPacket::getNumEntities() const
{
	return _data[DATA_START_POS];
}
