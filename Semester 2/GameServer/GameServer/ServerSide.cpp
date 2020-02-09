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

using std::cout;
using std::endl;
using std::string;
using std::stof;

//INPUT handling
struct Player
{
	Transform _transform;
	string _ip = "";
	bool connected = false;
	string _id = "";
	sockaddr* _sockAddr;
	int _sockAddrLen;
	sockaddr_in fromAddr;
};




Player _player1;
Player _player2;
// Networking
SOCKET server_socket;
struct addrinfo* ptr = NULL;
#define PORT "5000"
#define BUF_LEN 512
#define UPDATE_INTERVAL 0.100 //seconds

void parseData(const string& buf, Vector3& pos, Quaternion rot);

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

bool connectPlayer()
{
	char buf[BUF_LEN];
	struct sockaddr_in fromAddr;
	int fromLen;
	fromLen = sizeof(fromAddr);

	memset(buf, 0, BUF_LEN);

	int bytes_received = -1;
	int sError = -1;

	// Receive message from a client.
	bytes_received = recvfrom(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, &fromLen);

	sError = WSAGetLastError();


	// Check if incoming message is for connecting.
	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
	{
		//std::cout << "Received: " << buf << std::endl;
		char ipbuf[INET_ADDRSTRLEN];
		inet_ntop(AF_INET, &fromAddr, ipbuf, sizeof(ipbuf));
		


		string temp = buf;
		temp = temp.substr(0, 7);
		if (temp == "connect")
		{
			temp = buf;
			temp = temp.substr(7);

			// First player is connecting
			if (!_player1.connected)
			{
				_player1._ip = ipbuf;
				_player1.connected = true;
				_player1._id = temp;
				_player1._sockAddr = (sockaddr*) &fromAddr;
				_player1._sockAddrLen = fromLen;
				_player1.fromAddr = fromAddr;
			}
			// Second player is connecting
			else
			{
				// Make sure not the same player connecting.
				//if (_player1._ip == ipbuf)
				//	return false;
				if (_player1._id == temp)
					return false;



				// New player connecting.
				_player2._ip = ipbuf;
				_player2.connected = true;
				_player2._id = temp;
				_player2._sockAddr = (sockaddr*) &fromAddr;
				_player2._sockAddrLen = fromLen;
				_player2.fromAddr = fromAddr;

				cout << "Both players have connected" << endl;
			}

			// Tell client they have connected to the server.
			memset(buf, 0, BUF_LEN);
			strcpy_s(buf, "connected");
			sendto(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, fromLen);

			cout << "Player connected" << endl;
			printf("Source IP address: %s\n", ipbuf);
			return true;
		}
	}

	return false;
}

void updateTransform()
{
	char buf[BUF_LEN];
	struct sockaddr_in fromAddr;
	int fromLen;
	fromLen = sizeof(fromAddr);

	memset(buf, 0, BUF_LEN);

	int bytes_received = -1;
	int sError = -1;


	// Reveive transform updates from players.
	bytes_received = recvfrom(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, &fromLen);

	sError = WSAGetLastError();

	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
	{
		//std::cout << "Received: " << buf << std::endl;

		string temp = buf;
		//std::size_t pos = temp.find('@');
		//temp = temp.substr(0, pos - 1);
		//tx = std::stof(temp);
		//temp = buf;
		//temp = temp.substr(pos + 1);
		//ty = std::stof(temp);

		//std::cout << tx << " " << ty << std::endl;

		// Retrieve network id of incomming message.
		temp = temp.substr(0, 1);

		Vector3 position;
		Quaternion rotation;


		// Player one sent transform data.
		if (temp == _player1._id)
		{
			cout << "Network ID: " << temp << endl;
			temp = buf;

			parseData(temp, position, rotation);

			// Send data to other player.
			if (sendto(server_socket, buf, BUF_LEN, 0, (sockaddr*)&_player2.fromAddr, fromLen) == SOCKET_ERROR)
			{
				printf("Failed to send transform data. %d\n", WSAGetLastError());
				//char ipbuf[INET_ADDRSTRLEN];
				//inet_ntop(AF_INET, &fromAddr, ipbuf, sizeof(ipbuf));
				//cout << ipbuf << endl;
			}

			//if (sendto(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, fromLen) == SOCKET_ERROR)
			//{
			//	printf("Failed to send transform data. %d\n", WSAGetLastError());
			//}
		}
		// Player two sent transform data.
		else if (temp == _player2._id)
		{
			cout << "Network ID: " << temp << endl;
			temp = buf;

			parseData(temp, position, rotation);

			//// Send data to other player.
			if (sendto(server_socket, buf, BUF_LEN, 0, (sockaddr*)&_player1.fromAddr, fromLen) == SOCKET_ERROR)
			{
				printf("Failed to send transform data. %d\n", WSAGetLastError());
				//char ipbuf[INET_ADDRSTRLEN];
				//inet_ntop(AF_INET, &fromAddr, ipbuf, sizeof(ipbuf));
				//cout << ipbuf << endl;
			}
		}
		else
		{
			cout << "Unkown network id!" << endl;
			return;
		}
	}
}

void parseData(const string& buf, Vector3& pos, Quaternion rot)
{
	size_t currPos = 0;
	size_t endPos = 0;
	float data[7];
	unsigned int index = 0;

	string temp = buf;

	do
	{
		currPos = buf.find_first_of("WXYZ", currPos);
		endPos = buf.find_first_of("WXYZ", currPos + 1);

		currPos = currPos + 2; // Skip space

		if (endPos != string::npos)
			temp = buf.substr(currPos, (endPos - currPos) - 1);
		else
			temp = buf.substr(currPos);


		data[index] = stof(temp);

		++index;

	} while (endPos != string::npos);

	pos.x = data[0];
	pos.y = data[1];
	pos.z = data[2];

	rot.x = data[3];
	rot.y = data[4];
	rot.z = data[5];
	rot.w = data[6];


	cout << pos.x << " " << pos.y << " " << pos.z << endl;
	cout << rot.x << " " << rot.y << " " << rot.z << " " << rot.w << endl;
}


int main() 
{
	//Initialize Network
	if (!initNetwork())
		return 1;
	
	///// Game loop /////
	while (true) 
	{
		
		////////////////////
		/*
		CODE TO RECEIVE UPDATES FROM CLIENT GOES HERE...
		*/


		// Don't send updates until both players have connected.
		while (!_player1.connected || !_player2.connected)
		{
			connectPlayer();
		}

		

		updateTransform();
		////////////////////
	}

	return 0;
}