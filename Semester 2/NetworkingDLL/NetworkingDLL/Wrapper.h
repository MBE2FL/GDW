#pragma once

//#include "PluginSettings.h"
#include "ClientSide.h"

#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

	PLUGIN_API void sendData(const Vector3& position, const Quaternion& rotation);//from unity to here
	PLUGIN_API void receiveData(Vector3& position, Quaternion& rotation);//from here to unity
	PLUGIN_API bool connectToServer(const char* id);
	PLUGIN_API bool initNetwork(const char* ip, const char* id);


#ifdef __cplusplus
}
#endif // __cplusplus
