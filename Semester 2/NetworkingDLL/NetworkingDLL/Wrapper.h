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
	PLUGIN_OUT void networkCleanup();
	PLUGIN_OUT bool connectToServer(const char* ip);
	PLUGIN_OUT bool queryConnectAttempt(int& id);
	PLUGIN_OUT PacketTypes queryEntityRequest();
	PLUGIN_OUT bool sendStarterEntities(EntityData* entities, int numEntities);
	PLUGIN_OUT bool sendRequiredEntities(EntityData* entities, int& numEntities, int& numServerEntities);
	PLUGIN_OUT void getServerEntities(EntityData* serverEntities);


	PLUGIN_OUT void sendData(const PacketTypes pckType, void* data);

	PLUGIN_OUT void receiveUDPData();
	PLUGIN_OUT void receiveTCPData();
	PLUGIN_OUT void getPacketHandleSizes(int& transDataElements, int& animDataElements, int& entityDataElements);
	PLUGIN_OUT void getPacketHandles(void* dataHandle);

#ifdef __cplusplus
}
#endif // __cplusplus
