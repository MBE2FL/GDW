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

class PLUGIN_API ClientSide
{
public:
	ClientSide();
	bool initNetwork(const string& ip);
	bool connectToServer(const char* id);
	void sendData(const Vector3& position, const Quaternion& rotation);//from unity to here
	void receiveData(Vector3& position, Quaternion& rotation);//from here to unity

private:
	Transform _transform;

	SOCKET client_socket;
	struct addrinfo* ptr = NULL;
	string _serverIP = "";
};