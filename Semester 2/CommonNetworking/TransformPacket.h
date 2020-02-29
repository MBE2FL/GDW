#pragma once
#include "Packet.h"
#include "Transform.h"

struct TransformData
{
	Vector3 _pos;
	Quaternion _rot;
};

class TransformPacket : public Packet
{
public:
	TransformPacket(int8_t networkID, int8_t objID);
	TransformPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	virtual void deserialize(void* data) override;

private:
	TransformPacket() {};
};