#pragma once
#include "Packet.h"
#include "Transform.h"


struct EntityData : PacketData
{
	int8_t _entityPrefabType;
	int8_t _ownership;
	Vector3 _position;
	Quaternion _rotation;
	int8_t _parent;
};


class EntityPacket : public Packet
{
public:
	//EntityPacket(int8_t networkID, int8_t objID);
	EntityPacket(int8_t networkID);
	EntityPacket(PacketTypes pckType, int8_t networkID, int8_t numEntities);
	EntityPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	//virtual void deserialize(int8_t& numEntities, void* data) override;
	virtual void deserialize(void* data) override;
	int8_t getNumEntities() const;

private:
	EntityPacket() {};
};