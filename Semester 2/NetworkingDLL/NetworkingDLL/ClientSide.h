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
#include "Packet.h"


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


//enum MessageTypes : INT8
//{
//	ConnectionAttempt,
//	ConnectionAccepted,
//	ConnectionFailed,
//	ServerFull,
//	TransformData
//};


class PLUGIN_OUT ClientSide
{
public:
	ClientSide();
	bool initNetwork(const char* ip);
	bool initUDP(const char* ip);
	bool initTCP(const char* ip);
	void connectToServerTCP();
	bool connectToServer();
	bool queryConnectAttempt(int& id);
	void sendData(const Vector3& position, const Quaternion& rotation);//from unity to here
	void receiveData(Vector3& position, Quaternion& rotation);//from here to unity
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
};