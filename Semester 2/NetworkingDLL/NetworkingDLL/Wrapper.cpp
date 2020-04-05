// MAKE SURE pch.h is included in ALL .cpp files
#include "pch.h"
#include "Wrapper.h"

ClientSide cs;


// Implement functions
PLUGIN_OUT void InitPlugin(CS_to_Plugin_Functions funcs)
{
	// We just copy the struct over
	CS_Functions = funcs;
}

// Init the console. You can probably but this in the initplugin function
// I suggest adding a bool to the function though to make the console optional
// ( if(bool) InitConsole )
PLUGIN_OUT void InitConsole()
{
	FILE* pConsole;
	AllocConsole();
	freopen_s(&pConsole, "CONOUT$", "wb", stdout);

	std::cout << "Client plugin initialized.\n";
	std::cout << "==============================================================================\n";

	//std::cout << CS_Functions.multiplyVectors(Vector3(1, 2, 4), Vector3(2, 1, 2)).toString() << std::endl;
	//std::cout << CS_Functions.multiplyInts(1, 2) << std::endl;
	//std::cout << CS_Functions.GetFloat() << std::endl;

	cs.setFuncs(CS_Functions);
}

// This may or may not work, it's not tested yet
PLUGIN_OUT void FreeTheConsole()
{
	FreeConsole();
}

// C++ always takes in C# strings as const char* and sends them back as const char*
PLUGIN_OUT const char* OutputMessageToConsole(const char* msg)
{
	std::cout << msg << std::endl;
	return msg;
}


PLUGIN_OUT bool initNetwork()
{
	return cs.initNetwork();
}

PLUGIN_OUT void networkCleanup()
{
	return cs.networkCleanup();
}

PLUGIN_OUT void connectToServer(const char* ip)
{
	return cs.connectToServer(ip);
}

PLUGIN_OUT void queryConnectAttempt(int& id, ConnectionStatus& status)
{
	return cs.queryConnectAttempt(id, status);
}

PLUGIN_OUT void queryEntityRequest(PacketTypes& query)
{
	return cs.queryEntityRequest(query);
}

//PLUGIN_OUT PacketTypes sendStarterEntities(EntityData* entities, int numEntities)
//{
//	return cs.sendStarterEntities(entities, numEntities);
//}
//
//PLUGIN_OUT PacketTypes sendRequiredEntities(EntityData* entities, int& numEntities, int& numServerEntities)
//{
//	return cs.sendRequiredEntities(entities, numEntities, numServerEntities);
//}

PLUGIN_OUT PacketTypes sendEntities(EntityData* entities, int& numEntities)
{
	return cs.sendEntities(entities, numEntities);
}

PLUGIN_OUT void getServerEntities(EntityData* serverEntities, int& numServerEntities)
{
	return cs.getServerEntities(serverEntities, numServerEntities);
}

PLUGIN_OUT void sendData(const PacketTypes pckType, void* data)
{
	return cs.sendData(pckType, data);
}

PLUGIN_OUT void receiveUDPData()
{
	return cs.receiveUDPData();
}

PLUGIN_OUT void receiveTCPData()
{
	return cs.receiveTCPData();
}

PLUGIN_OUT void getPacketHandleSizes(int& transDataElements, int& animDataElements, int& entityDataElements, int& ownershipDataElements)
{
	return cs.getPacketHandleSizes(transDataElements, animDataElements, entityDataElements, ownershipDataElements);
}

PLUGIN_OUT void getPacketHandles(void* dataHandle)
{
	return cs.getPacketHandles(dataHandle);
}

PLUGIN_OUT void requestScores()
{
	return cs.requestScores();
}

PLUGIN_OUT void getNumScores(int& numScores)
{
	return cs.getNumScores(numScores);
}

PLUGIN_OUT ScoreData* getScoresHandle()
{
	return cs.getScoresHandle();
}

PLUGIN_OUT void cleanupScoresHandle()
{
	return cs.cleanupScoresHandle();
}

PLUGIN_OUT void receiveLobbyData()
{
	return cs.receiveLobbyData();
}

PLUGIN_OUT void stopLobbyReceive()
{
	return cs.stopLobbyReceive();
}

PLUGIN_OUT void getNumLobbyPackets(int& numMsgs, int& newTeamNameMsg, int& newCharChoice, int& numNewPlayers)
{
	return cs.getNumLobbyPackets(numMsgs, newTeamNameMsg, newCharChoice, numNewPlayers);
}

PLUGIN_OUT void getLobbyPacketHandles(void* dataHandle)
{
	return cs.getLobbyPacketHandles(dataHandle);
}

PLUGIN_OUT void clearLobbyBuffers()
{
	return cs.clearLobbyBuffers();
}

PLUGIN_OUT void setOwnership(uint8_t EID, Ownership ownership)
{
	return cs.setOwnership(EID, ownership);
}
