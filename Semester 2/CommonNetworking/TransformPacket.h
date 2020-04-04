#pragma once
#include "Packet.h"
#include "Transform.h"

struct TransformData : PacketData
{
	Vector3 _pos;
	Quaternion _rot;
	Vector3 _vel;
};

class TransformPacket : public Packet
{
public:
	//TransformPacket(uint8_t networkID, uint8_t objID);
	TransformPacket(uint8_t networkID);
	TransformPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	//virtual void deserialize(uint8_t& objID, void* data) override;
	virtual void deserialize(void* data) override;

private:
	TransformPacket() {};
};