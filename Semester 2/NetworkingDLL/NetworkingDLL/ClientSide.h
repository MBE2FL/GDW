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
#define SERVER "127.0.0.1"
#define PORT "5000"
#define BUF_LEN 512
#define UPDATE_INTERVAL 0.100 //seconds

class PLUGIN_API ClientSide
{
	ClientSide();
	bool initNetwork();
	void send(const Vector3& position, const Quaternion& rotation);//from unity to here
	void receive(Vector3& position, Quaternion& rotation);//from here to unity

private:
	Transform _transform;

	SOCKET client_socket;
	struct addrinfo* ptr = NULL;
};