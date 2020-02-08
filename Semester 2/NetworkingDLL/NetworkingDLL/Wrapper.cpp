#include "Wrapper.h"

ClientSide cs;


void sendData(const Vector3& position, const Quaternion& rotation)
{
	return cs.sendData(position, rotation);
}

void receiveData(Vector3& position, Quaternion& rotation)
{
	return cs.receiveData(position, rotation);
}

bool connectToServer(const char* id)
{
	return cs.connectToServer(id);
}

bool initNetwork(const string& ip)
{
	return cs.initNetwork(ip);
}
