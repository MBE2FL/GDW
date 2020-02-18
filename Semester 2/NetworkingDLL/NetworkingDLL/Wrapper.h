#pragma once

//#include "PluginSettings.h"
#include "ClientSide.h"


// This struct also needs to be the same as in C#, if you want more functions just add it here and there.
// Syntax is return_type(*function_name)(parameters)
// To call just call it regularly [function_name(parameters)]
struct CS_to_Plugin_Functions
{
	Vector3(*multiplyVectors)(Vector3 v1, Vector3 v2);
	int(*multiplyInts)(int i1, int i2);
	float(*GetFloat)();
};


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

	PLUGIN_OUT void sendData(const Vector3& position, const Quaternion& rotation);//from unity to here
	PLUGIN_OUT void receiveData(Vector3& position, Quaternion& rotation);//from here to unity
	PLUGIN_OUT bool connectToServer(int& id);
	PLUGIN_OUT bool initNetwork(const char* ip);


#ifdef __cplusplus
}
#endif // __cplusplus
