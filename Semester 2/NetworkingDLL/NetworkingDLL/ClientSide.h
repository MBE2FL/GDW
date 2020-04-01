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
#include "ScorePacket.h"
#include <vector>
#include <unordered_map>
#include <unordered_set>
#include <algorithm>
#include <iterator>


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
using std::copy;


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
	bool queryConnectAttempt(int& id);

	PacketTypes queryEntityRequest();
	bool sendStarterEntities(EntityData* entities, int numEntities);
	bool sendRequiredEntities(EntityData* entities, int& numEntities, int& numServerEntities);
	void getServerEntities(EntityData* serverEntities);


	void sendData(const PacketTypes pckType, void* data);

	void receiveUDPData();
	void receiveTCPData();
	void getPacketHandleSizes(int& transDataElements, int& animDataElements, int& entityDataElements);
	void getPacketHandles(void* dataHandle);

	
	void getScores(int& numScores);
	ScoreData* getScoresHandle();
	void cleanupScoresHandle();
	void sendScore(ScoreData scoreData);


	void setFuncs(const CS_to_Plugin_Functions& funcs);

private:
	CS_to_Plugin_Functions _funcs;


	bool _connected = false;
	SOCKET _clientUDPsocket;
	SOCKET _clientTCPsocket;
	addrinfo* _ptr = nullptr;
	string _serverIP = "";
	int8_t _networkID = NULL;


	unordered_map<PacketTypes, unordered_map<int8_t, Packet*>> _udpPacketBuf = unordered_map<PacketTypes, unordered_map<int8_t, Packet*>>();
	unordered_map<PacketTypes, unordered_map<int8_t, vector<PacketData>>> _tcpPacketBuf = unordered_map<PacketTypes, unordered_map<int8_t, vector<PacketData>>>();

	EntityData* _receivedEntitiesBuf = nullptr;
	int8_t _numEntitiesReceived = 0;


	vector<TransformData> _transDataBuf;
	vector<AnimData> _animDataBuf;
	vector<EntityData> _entityDataBuf;


	ScoreData* _scoresBuf;
};