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
#include "ChatPacket.h"
#include "CharChoicePacket.h"
#include <vector>
#include <unordered_map>
#include <unordered_set>
#include <algorithm>
#include <iterator>
#include <ctime>


// Networking
#define LOCAL_HOST "127.0.0.1"
#define PORT "5000"
#define BUF_LEN 512
#define UPDATE_INTERVAL 0.100 //seconds
#define MAX_TIMEOUTS 8
#define MAX_CONNECT_ATTEMPT_TIME 8.0f

using std::cout;
using std::endl;
using std::string;
using std::to_string;
using std::thread;
using std::vector;
using std::unordered_map;
using std::unordered_set;
using std::copy;
using std::clock;


enum ConnectionStatus : uint8_t
{
	Connected,
	Connecting,
	Disconected,
	ConnectionFailedStatus,
	ServerFullStatus,
};


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

	void connectToServer(const char* ip);
	void processConnectAttempt(PacketTypes pckType, char buf[BUF_LEN]);
	void queryConnectAttempt(int& id, ConnectionStatus& status);

	PacketTypes queryEntityRequest();
	bool sendStarterEntities(EntityData* entities, int numEntities);
	bool sendRequiredEntities(EntityData* entities, int& numEntities, int& numServerEntities);
	void getServerEntities(EntityData* serverEntities);


	void sendData(const PacketTypes pckType, void* data);

	void receiveUDPData();
	void receiveTCPData();
	void getPacketHandleSizes(int& transDataElements, int& animDataElements, int& entityDataElements);
	void getPacketHandles(void* dataHandle);

	
	void requestScores();
	void getNumScores(int& numScores);
	ScoreData* getScoresHandle();
	void cleanupScoresHandle();


	void receiveLobbyData();
	void getNumLobbyPackets(int& numMsgs, int& newTeamNameMsg, int& newCharChoice);
	void getLobbyPacketHandles(void* dataHandle);


	void setFuncs(const CS_to_Plugin_Functions& funcs);

private:
	CS_to_Plugin_Functions _funcs;


	bool _connected = false;
	SOCKET _clientUDPsocket;
	SOCKET _clientTCPsocket;
	addrinfo* _ptr = nullptr;
	string _serverIP = "";
	uint8_t _networkID = NULL;
	int totalConnectAttemptTime;
	ConnectionStatus _status = Disconected;


	unordered_map<PacketTypes, unordered_map<uint8_t, Packet*>> _udpPacketBuf = unordered_map<PacketTypes, unordered_map<uint8_t, Packet*>>();
	unordered_map<PacketTypes, unordered_map<uint8_t, vector<PacketData>>> _tcpPacketBuf = unordered_map<PacketTypes, unordered_map<uint8_t, vector<PacketData>>>();

	EntityData* _receivedEntitiesBuf = nullptr;
	uint8_t _numEntitiesReceived = 0;


	vector<TransformData> _transDataBuf;
	vector<AnimData> _animDataBuf;
	vector<EntityData> _entityDataBuf;


	int _numScores = 0;
	ScoreData* _scoresBuf = nullptr;

	vector<ChatData> _chatDataBuf;
	ChatData* _teamNameBuf = nullptr;
	CharChoiceData* _charChoiceBuf = nullptr;
};