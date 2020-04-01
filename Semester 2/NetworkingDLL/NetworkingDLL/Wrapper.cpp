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


PLUGIN_OUT bool initNetwork(const char* ip)
{
	return cs.initNetwork(ip);
}

PLUGIN_OUT void networkCleanup()
{
	return cs.networkCleanup();
}

PLUGIN_OUT bool connectToServer(const char* ip)
{
	return cs.connectToServer(ip);
}

PLUGIN_OUT bool queryConnectAttempt(int& id)
{
	return cs.queryConnectAttempt(id);
}

PLUGIN_OUT PacketTypes queryEntityRequest()
{
	return cs.queryEntityRequest();
}

PLUGIN_OUT bool sendStarterEntities(EntityData* entities, int numEntities)
{
	return cs.sendStarterEntities(entities, numEntities);
}

PLUGIN_OUT bool sendRequiredEntities(EntityData* entities, int& numEntities, int& numServerEntities)
{
	return cs.sendRequiredEntities(entities, numEntities, numServerEntities);
}

PLUGIN_OUT void getServerEntities(EntityData* serverEntities)
{
	return cs.getServerEntities(serverEntities);
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

PLUGIN_OUT void getPacketHandleSizes(int& transDataElements, int& animDataElements, int& entityDataElements)
{
	return cs.getPacketHandleSizes(transDataElements, animDataElements, entityDataElements);
}

PLUGIN_OUT void getPacketHandles(void* dataHandle)
{
	return cs.getPacketHandles(dataHandle);
}

PLUGIN_OUT void getScores(int& numScores)
{
	return cs.getScores(numScores);
}

PLUGIN_OUT ScoreData* getScoresHandle()
{
	return cs.getScoresHandle();
}

PLUGIN_OUT void cleanupScoresHandle()
{
	return cs.cleanupScoresHandle();
}

PLUGIN_OUT void sendScore(ScoreData scoreData)
{
	return cs.sendScore(scoreData);
}

PLUGIN_OUT void receiveLobbyData()
{
	return cs.receiveLobbyData();
}

PLUGIN_OUT void getNumLobbyPackets(int& numMsgs, int& numChars)
{
	return cs.getNumLobbyPackets(numMsgs, numChars);
}

PLUGIN_OUT void getLobbyPacketHandles(void* dataHandle)
{
	return cs.getLobbyPacketHandles(dataHandle);
}
