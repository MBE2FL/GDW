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


using std::cout;
using std::endl;
using std::string;
using std::stof;
using std::vector;
using std::thread;
using std::mutex;
using std::lock_guard;


#define PORT "5000"
#define BUF_LEN 512
#define UPDATE_INTERVAL 0.100 //seconds
#define MAX_CLIENTS 2
#define MAX_TIMEOUTS 4


struct Client
{
	string _ip = "";
	bool _connected = false;
	int8_t _id = NULL;
	sockaddr_in _udpSockAddr;
	int _udpSockAddrLen = -1;
};

struct Entity
{
	int8_t _objID = 0;
	int8_t _prefabType = 0;
};


//enum MessageTypes : INT8
//{
//	ConnectionAttempt,
//	ConnectionAccepted,
//	ConnectionFailed,
//	ServerFull,
//	TransformData
//};



class Server
{
public:
	Server();

	bool initNetwork();
	bool initUDP();
	bool initTCP();

	void listenForConnections();

	void processClientEntityRequest(SOCKET* clientSocket);
	void processStarterEntities(SOCKET* clientSocket, char buf[BUF_LEN]);
	void processRequiredEntities(SOCKET* clientSocket, char buf[BUF_LEN]);
	void sendEntitiesToClient(SOCKET* clientSocket);

	void processTransform(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen);
	void processAnim(char buf[BUF_LEN], SOCKET* socket);

	void update();
	void initUpdateThreads();
	void udpUpdate();
	void tcpUpdate();


private:
	Client _player1;
	Client _player2;

	vector<Client*> _clients;
	thread _udpThread;
	thread _tcpThread;
	vector<Entity*> _entities;

	// Networking
	SOCKET _serverUDP_socket = NULL;
	SOCKET _serverTCP_socket = NULL;
	struct addrinfo* _ptr = NULL;
	vector<SOCKET*> _clientTCPSockets;

	sockaddr_in* _udpListenInfo;
};