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

	std::cout << "Welcome to our plugin.\n";
	std::cout << "==============================================================================\n";

	std::cout << CS_Functions.multiplyVectors(Vector3(1, 2, 4), Vector3(2, 1, 2)).toString() << std::endl;
	std::cout << CS_Functions.multiplyInts(1, 2) << std::endl;
	std::cout << CS_Functions.GetFloat() << std::endl;

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

PLUGIN_OUT bool connectToServer()
{
	return cs.connectToServer();
}

PLUGIN_OUT bool queryConnectAttempt(int& id)
{
	return cs.queryConnectAttempt(id);
}

//PLUGIN_OUT void sendData(const Vector3& position, const Quaternion& rotation)
//{
//	return cs.sendData(position, rotation);
//}

PLUGIN_OUT void sendData(const int msgType, const int objID, void* data)
{
	return cs.sendData(msgType, objID, data);
}

PLUGIN_OUT void receiveData(MessageTypes& msgType, int& objID, void* data)
{
	return cs.receiveData(msgType, objID, data);
}

PLUGIN_OUT char* getReceiveData(int& numElements)
{
	return cs.getReceiveData(numElements);
}

PLUGIN_OUT void receiveUDPData()
{
	return cs.receiveUDPData();
}

PLUGIN_OUT void getPacketHandles(int& transDataElements, TransformData* transDataHandle, int& animDataElements, AnimData* animDataHandle)
{
	return cs.getPacketHandles(transDataElements, transDataHandle, animDataElements, animDataHandle);
}

PLUGIN_OUT TransformData* getTransformHandle()
{
	return cs.getTransformHandle();
}

PLUGIN_OUT void packetHandlesCleanUp()
{
	return cs.packetHandlesCleanUp();
}

PLUGIN_OUT void receiveData(Vector3& position, Quaternion& rotation)
{
	return cs.receiveData(position, rotation);
}
