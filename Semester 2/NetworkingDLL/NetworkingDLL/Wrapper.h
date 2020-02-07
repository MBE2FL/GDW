#pragma once

#include "PluginSettings.h"
#include "ClientSide.h"

#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

	PLUGIN_API 	void send(const Vector3& position, const Quaternion& rotation);//from unity to here
	PLUGIN_API	void receive(Vector3& position, Quaternion& rotation);//from here to unity


#ifdef __cplusplus
}
#endif // __cplusplus
