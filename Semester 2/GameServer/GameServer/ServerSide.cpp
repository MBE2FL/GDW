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

//INPUT handling
struct  Player
{
	Transform _transform;
	size_t _ip;
	bool connected = false;
};




Player player1;
Player player2;
// Networking
SOCKET server_socket;
struct addrinfo* ptr = NULL;
#define PORT "5000"
#define BUF_LEN 512
#define UPDATE_INTERVAL 0.100 //seconds

bool connect()
{
	char buf[BUF_LEN];
	struct sockaddr_in fromAdder;
	int fromLen;
	fromLen = sizeof(fromAdder);

	memset(buf, 0, BUF_LEN);

	int bytes_received = -1;
	int sError = -1;

	bytes_received = recvfrom(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAdder, &fromLen);

	sError = WSAGetLastError();

	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
	{
		//std::cout << "Received: " << buf << std::endl;

		std::string temp = buf;
		if (temp == "connect")
		{
			if (!player1.connected)
			{
				player1._ip = fromAdder.sin_addr.S_un.S_addr;
				player1.connected = true;
			}
			else
			{
				player2._ip = fromAdder.sin_addr.S_un.S_addr;
				player2.connected = true;
			}
			memset(buf, 0, BUF_LEN);
			strcpy(buf,"connected");
			sendto(client_socket, buf, BUF_LEN, 0, ptr->ai_addr, ptr->ai_addrlen)
		}
	}
}

bool initNetwork() {
	//Initialize winsock
	WSADATA wsa;
	int error;
	error = WSAStartup(MAKEWORD(2, 2), &wsa);

	if (error != 0) {
		printf("Failed to initialize %d\n", error);
		return 0;
	}

	//Create a server socket

	struct addrinfo hints;

	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_DGRAM;
	hints.ai_protocol = IPPROTO_UDP;
	hints.ai_flags = AI_PASSIVE;

	if (getaddrinfo(NULL, PORT, &hints, &ptr) != 0) {
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		WSACleanup();
		return 0;
	}

	
	server_socket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

	if (server_socket == INVALID_SOCKET) {
		printf("Failed creating a socket %d\n", WSAGetLastError());
		WSACleanup();
		return 0;
	}

	// Bind socket

	if (bind(server_socket, ptr->ai_addr, (int)ptr->ai_addrlen) == SOCKET_ERROR) {
		printf("Bind failed: %d\n", WSAGetLastError());
		closesocket(server_socket);
		freeaddrinfo(ptr);
		WSACleanup();
		return 1;
	}

	/// Change to non-blocking mode
	u_long mode = 1;// 0 for blocking mode
	ioctlsocket(server_socket, FIONBIO, &mode);

	printf("Server is ready!\n");

	return 1;
}


int main() {
	//Initialize Network
	if (!initNetwork())
		return 1;
	
	///// Game loop /////
	while (true) {
		
		////////////////////
		/*
		CODE TO RECEIVE UPDATES FROM CLIENT GOES HERE...
		*/
		char buf[BUF_LEN];
		struct sockaddr_in fromAdder;
		int fromLen;
		fromLen = sizeof(fromAdder);

		memset(buf, 0, BUF_LEN);

		int bytes_received = -1;
		int sError = -1;

		bytes_received = recvfrom(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAdder, &fromLen);

		sError = WSAGetLastError();

		if (sError != WSAEWOULDBLOCK && bytes_received > 0)
		{
			//std::cout << "Received: " << buf << std::endl;

			std::string temp = buf;
			std::size_t pos = temp.find('@');
			temp = temp.substr(0, pos - 1);
			tx = std::stof(temp);
			temp = buf;
			temp = temp.substr(pos + 1);
			ty = std::stof(temp);

			std::cout << tx <<" " << ty << std::endl;
		}
		////////////////////
	}
	return 0;

}