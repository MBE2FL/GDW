#include "Packet.h"
#include <iostream>


struct AnimData : PacketData
{
	float _state;
};


class AnimPacket : public Packet
{
public:
	//AnimPacket(uint8_t networkID, uint8_t objID);
	AnimPacket(uint8_t networkID);
	AnimPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	//virtual void deserialize(uint8_t& objID, void* data) override;
	virtual void deserialize(void* data) override;

private:
	AnimPacket() {};
};