#include "Wrapper.h"
#include "ClientSide.h"

ClientSide cs;

PLUGIN_API

PLUGIN_API void send(const Vector3& position, const Quaternion& rotation)
{
	return PLUGIN_API send(position, rotation);
}

PLUGIN_API void receive(Vector3& position, Quaternion& rotation)
{
	return PLUGIN_API receive(position, rotation);
}
