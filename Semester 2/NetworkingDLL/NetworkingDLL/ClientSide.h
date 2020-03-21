#pragma once
#include <iostream>
#include <fstream>
#include <string>

///// Networking //////
#include <WinSock2.h>
#include <ws2tcpip.h>
#include <stdio.h>
#pragma comment(lib, "Ws2_32.lib")
///////////////////////

#include "Transform.h"
#include "PluginSettings.h"
#include <thread>
#include "TransformPacket.h"
#include "AnimPacket.h"
#include "EntityPacket.h"
#include <vector>
#include <unordered_map>
#include <unordered_set>


// Networking
#define LOCAL_HOST "127.0.0.1"
#define PORT "5000"
#define BUF_LEN 512
#define UPDATE_INTERVAL 0.100 //seconds
#define MAX_TIMEOUTS 8

using std::cout;
using std::endl;
using std::string;
using std::to_string;
using std::thread;
using std::vector;
using std::unordered_map;
using std::unordered_set;


// This struct also needs to be the same as in C#, if you want more functions just add it here and there.
// Syntax is return_type(*function_name)(parameters)
// To call just call it regularly [function_name(parameters)]
struct CS_to_Plugin_Functions
{
	Vector3(*multiplyVectors)(Vector3 v1, Vector3 v2);
	int(*multiplyInts)(int i1, int i2);
	float(*GetFloat)();

	bool(*connectedToServer)();
};



class PLUGIN_OUT ClientSide
{
public:
	ClientSide();

	bool initNetwork(const char* ip);
	bool initUDP(const char* ip);
	bool initTCP(const char* ip);
	void networkCleanup();

	bool connectToServer(const char* ip);

	MessageTypes queryEntityRequest();
	bool sendStarterEntities(EntityData* entities, int numEntities);
	bool sendRequiredEntities(EntityData* entities, int& numEntities);

	//bool connectToServer();
	bool queryConnectAttempt(int& id);


	void sendData(const Vector3& position, const Quaternion& rotation);//from unity to here
	void sendData(const int msgType, const int objID, void* data);

	void receiveData(Vector3& position, Quaternion& rotation);//from here to unity
	void receiveData(MessageTypes& msgType, int& objID, void* data);
	char* getReceiveData(int& numElements);


	void receiveUDPData();
	void receiveTCPData();
	void getPacketHandleSizes(int& transDataElements, int& animDataElements);
	void getPacketHandles(void* dataHandle);
	TransformData* getTransformHandle();
	void packetHandlesCleanUp();



	void parseData(const string& buf, Vector3& pos, Quaternion& rot);

	void setFuncs(const CS_to_Plugin_Functions& funcs);

private:
	CS_to_Plugin_Functions _funcs;

	Transform _transform;
	Transform _otherTransform;
	bool _connected = false;

	SOCKET _clientUDPsocket;
	SOCKET _clientTCPsocket;
	struct addrinfo* _ptr = NULL;
	string _serverIP = "";
	int8_t _networkID = NULL;


	vector<char> _receiveBuf = vector<char>();
	int _receiveBufElements = 0;
	char* _receiveBufHandle = nullptr;



	unordered_map<MessageTypes, unordered_map<int8_t, Packet*>> _udpPacketBuf = unordered_map<MessageTypes, unordered_map<int8_t, Packet*>>();
	unordered_map<MessageTypes, unordered_map<int8_t, vector<Packet*>>> _tcpPacketBuf = unordered_map<MessageTypes, unordered_map<int8_t, vector<Packet*>>>();




	vector<TransformData> _transDataBuf;
	TransformData* _transDataHandle = nullptr;

	vector<AnimData> _animDataBuf;
	AnimData* _animDataHandle = nullptr;
};