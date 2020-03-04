#include "EntityPacket.h"

EntityPacket::EntityPacket(int8_t networkID, int8_t objID)
	: Packet(networkID, objID)
{
	_data[MSG_TYPE_POS] = MessageTypes::EntitiesStart;
}

EntityPacket::EntityPacket(char data[BUF_LEN])
{
	memcpy(_data, data, BUF_LEN);
}

void EntityPacket::serialize(void* data)
{
	//int8_t* entityData = reinterpret_cast<int8_t*>(data);


	memcpy(_data, data, BUF_LEN);
}

void EntityPacket::deserialize(int8_t& objID, void* data)
{
	EntityData* entityData = reinterpret_cast<EntityData*>(data);

	entityData->_numEntities = _data[DATA_START_POS];
	entityData->_entityIDs = new int8_t[entityData->_numEntities];
	entityData->_entityPrefabTypes = new int8_t[entityData->_numEntities];

	memcpy(entityData->_entityIDs, reinterpret_cast<int8_t*>(&_data[DATA_START_POS + 1]), entityData->_numEntities);
	memcpy(entityData->_entityPrefabTypes, reinterpret_cast<int8_t*>(&_data[DATA_START_POS + 1 + entityData->_numEntities]), entityData->_numEntities);
}
