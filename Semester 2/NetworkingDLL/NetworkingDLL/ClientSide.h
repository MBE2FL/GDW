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

// Networking
#define LOCAL_HOST "127.0.0.1"
#define PORT "5000"
#define BUF_LEN 512
#define UPDATE_INTERVAL 0.100 //seconds

using std::cout;
using std::endl;
using std::string;
using std::to_string;

enum MessageTypes : INT8
{
	ConnectionAttempt,
	ConnectionAccepted,
	ConnectionFailed,
	ServerFull,
	TransformData
};


class PLUGIN_OUT ClientSide
{
public:
	ClientSide();
	bool initNetwork(const char* ip);
	bool connectToServer(int& id);
	void sendData(const Vector3& position, const Quaternion& rotation);//from unity to here
	void receiveData(Vector3& position, Quaternion& rotation);//from here to unity
	void parseData(const string& buf, Vector3& pos, Quaternion& rot);

private:
	Transform _transform;
	Transform _otherTransform;

	SOCKET client_socket;
	struct addrinfo* ptr = NULL;
	string _serverIP = "";
	INT8 _networkID = NULL;
};