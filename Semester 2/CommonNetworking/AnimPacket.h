#include "Packet.h"
#include <iostream>


struct AnimData
{
	int _state;
};


class AnimPacket : public Packet
{
public:
	AnimPacket(int8_t networkID, int8_t objID);
	AnimPacket(char data[BUF_LEN]);
	virtual void serialize(void* data) override;
	virtual void deserialize(void* data) override;

private:
	AnimPacket() {};
};