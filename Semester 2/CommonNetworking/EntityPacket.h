#pragma once
#include "Packet.h"
#include "Transform.h"


struct EntityData : PacketData
{
	uint8_t _entityPrefabType;
	uint8_t _ownership;
	Vector3 _position;
	Quaternion _rotation;
	uint8_t _parent;
};


class EntityPacket : public Packet
{
public:
	//EntityPacket(uint8_t networkID, uint8_t objID);
	EntityPacket(uint8_t networkID);
	EntityPacket(PacketTypes pckType, uint8_t networkID, uint8_t numEntities);
	EntityPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	//virtual void deserialize(uint8_t& numEntities, void* data) override;
	virtual void deserialize(void* data) override;
	uint8_t getNumEntities() const;

private:
	EntityPacket() {};
};