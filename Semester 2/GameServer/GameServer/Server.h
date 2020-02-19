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


struct Client
{
	Transform _transform;
	string _ip = "";
	bool connected = false;
	INT8 _id = NULL;
	sockaddr* _sockAddr;
	int _sockAddrLen;
	sockaddr_in fromAddr;
};


enum MessageTypes : INT8
{
	ConnectionAttempt,
	ConnectionAccepted,
	ConnectionFailed,
	ServerFull,
	TransformData
};



class Server
{
public:
	Server();

	bool initNetwork();
	bool initUDP();
	bool initTCP();

	void connectPlayer(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen);
	void listenForConnections();
	void processTransform(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen);

	void update();
	void initUpdateThreads();
	void udpUpdate();
	void tcpUpdate();

	//void parseData(const string& buf, Vector3& pos, Quaternion& rot);

private:
	Client _player1;
	Client _player2;
	vector<Client*> _clients;
	thread _udpThread;
	thread _tcpThread;

	// Networking
	SOCKET _serverUDP_socket = NULL;
	SOCKET _serverTCP_socket = NULL;
	struct addrinfo* _ptr = NULL;
	vector<SOCKET*> _clientTCPSockets;
};