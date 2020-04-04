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

	PLUGIN_OUT bool initNetwork();
	PLUGIN_OUT void networkCleanup();
	PLUGIN_OUT void connectToServer(const char* ip);
	PLUGIN_OUT void queryConnectAttempt(int& id, ConnectionStatus& status);
	PLUGIN_OUT void queryEntityRequest(PacketTypes& query);
	PLUGIN_OUT PacketTypes sendStarterEntities(EntityData* entities, int numEntities);
	PLUGIN_OUT PacketTypes sendRequiredEntities(EntityData* entities, int& numEntities, int& numServerEntities);
	PLUGIN_OUT PacketTypes sendEntities(EntityData* entities, int& numEntities);
	PLUGIN_OUT void getServerEntities(EntityData* serverEntities, int& numServerEntities);


	PLUGIN_OUT void sendData(const PacketTypes pckType, void* data);

	PLUGIN_OUT void receiveUDPData();
	PLUGIN_OUT void receiveTCPData();
	PLUGIN_OUT void getPacketHandleSizes(int& transDataElements, int& animDataElements, int& entityDataElements);
	PLUGIN_OUT void getPacketHandles(void* dataHandle);


	PLUGIN_OUT void requestScores();
	PLUGIN_OUT void getNumScores(int& numScores);
	PLUGIN_OUT ScoreData* getScoresHandle();
	PLUGIN_OUT void cleanupScoresHandle();


	PLUGIN_OUT void receiveLobbyData();
	PLUGIN_OUT void getNumLobbyPackets(int& numMsgs, int& newTeamNameMsg, int& newCharChoice, int& numNewPlayers);
	PLUGIN_OUT void getLobbyPacketHandles(void* dataHandle);

#ifdef __cplusplus
}
#endif // __cplusplus
