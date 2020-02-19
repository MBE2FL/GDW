#pragma once

//#include "PluginSettings.h"
#include "ClientSide.h"





#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

	// You need this to call C# functions from here
	CS_to_Plugin_Functions CS_Functions;

	PLUGIN_OUT void InitPlugin(CS_to_Plugin_Functions funcs);
	PLUGIN_OUT void InitConsole();
	PLUGIN_OUT void FreeTheConsole();
	PLUGIN_OUT const char* OutputMessageToConsole(const char* msg);

	PLUGIN_OUT bool initNetwork(const char* ip);
	PLUGIN_OUT bool connectToServer();
	PLUGIN_OUT bool queryConnectAttempt(int& id);
	PLUGIN_OUT void sendData(const Vector3& position, const Quaternion& rotation);//from unity to here
	PLUGIN_OUT void receiveData(Vector3& position, Quaternion& rotation);//from here to unity

#ifdef __cplusplus
}
#endif // __cplusplus
