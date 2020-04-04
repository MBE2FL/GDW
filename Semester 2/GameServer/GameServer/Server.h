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
#include <vector>
#include <thread>
#include <mutex>

#include "Packet.h"
#include "TransformPacket.h"
#include "EntityPacket.h"
#include "AnimPacket.h"
#include "ScorePacket.h"
#include "ChatPacket.h"
#include "CharChoicePacket.h"
#include <ctime>

//#include <algorithm>
//#include <iterator>


using std::cout;
using std::endl;
using std::string;
using std::stof;
using std::vector;
using std::thread;
using std::mutex;
using std::lock_guard;
using std::clock;


#define PORT "5000"
#define BUF_LEN 512
#define MAX_CLIENTS 2
#define MAX_CONNECT_ATTEMPT_TIME 3.0f


struct Client
{
	string _ip = "";
	bool _connected = false;
	uint8_t _id = NULL;
	sockaddr_in _udpSockAddr;
	int _udpSockAddrLen = -1;
	SOCKET _tcpSocket = NULL;
	CharacterChoices _charChoice = NoChoice;
};



class Server
{
public:
	Server();
	~Server();

	bool initNetwork();
	bool initUDP();
	bool initTCP();

	void listenForConnections();

	void processClientEntityRequest(SOCKET* clientSocket);
	void processStarterEntities(SOCKET* clientSocket, char buf[BUF_LEN]);
	void processRequiredEntities(SOCKET* clientSocket, char buf[BUF_LEN]);
	void processEntities(SOCKET* clientSocket, char buf[BUF_LEN]);
	void sendEntitiesToClient(SOCKET* clientSocket);

	void processTransform(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen);
	void processAnim(char buf[BUF_LEN], SOCKET* socket);
	void processScore(char buf[BUF_LEN]);
	void processClientScoresRequest(char buf[BUF_LEN], SOCKET* socket);
	void processChat(char buf[BUF_LEN]);
	void processTeamName(char buf[BUF_LEN]);
	void processCharChoice(char buf[BUF_LEN]);

	void update();
	void initThreads();
	void udpUpdate();
	void tcpSoftUpdate();
	void tcpUpdate();


private:
	static uint8_t _clientIDs;
	vector<Client*> _clients;
	vector<Client*> _softConnectClients;
	thread _listenForConnections;
	thread _udpThread;
	thread _tcpSoftThread;
	thread _tcpThread;
	bool _stopThreads = false;
	vector<EntityData> _entities;

	bool _gameStarted = false;

	SOCKET _serverUDP_socket = NULL;
	SOCKET _serverTCP_socket = NULL;
	addrinfo* _ptr = nullptr;

	sockaddr_in* _udpListenInfoBuf;
	char* _teamNameBuf = nullptr;

	Scoreboard _scoreboard;
};