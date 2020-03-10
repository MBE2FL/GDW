#pragma once
#include "Packet.h"


struct EntityData
{
	int8_t _entityID;
	int8_t _entityPrefabType;
	int8_t _ownership;
};


class EntityPacket : public Packet
{
public:
	EntityPacket(int8_t networkID, int8_t objID);
	EntityPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	virtual void deserialize(int8_t& numEntities, void* data) override;
	int8_t getNumEntities() const;

private:
	EntityPacket() {};
};