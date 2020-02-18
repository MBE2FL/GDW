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


using std::cout;
using std::endl;
using std::string;
using std::stof;
using std::vector;

//INPUT handling
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


#define MAX_CLIENTS 2
Client _player1;
Client _player2;
vector<Client*> _clients;
// Networking
SOCKET server_socket;
struct addrinfo* ptr = NULL;
#define PORT "5000"
#define BUF_LEN 512
#define UPDATE_INTERVAL 0.100 //seconds


void parseData(const string& buf, Vector3& pos, Quaternion& rot);

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

void connectPlayer(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen)
{
	//char buf[BUF_LEN];


	// Check if incoming message is for connecting.
	//cout << "Received: " << buf << endl;
	char ipbuf[INET_ADDRSTRLEN];
	inet_ntop(AF_INET, &fromAddr, ipbuf, sizeof(ipbuf));
		


	//MessageTypes msgType = reinterpret_cast<MessageTypes&>(buf[0]);
	MessageTypes msgType;

	cout << "Incoming connection from " << ipbuf << endl;

	// Server is full.
	if (_clients.size() == MAX_CLIENTS)
	{
		cout << "Server is full!" << endl;


		msgType = MessageTypes::ConnectionFailed;
		memset(buf, 0, BUF_LEN);
		//buf[0] = reinterpret_cast<char&>(msgType);
		//buf[0] = static_cast<char>(msgType);
		buf[0] = msgType;
		msgType = MessageTypes::ServerFull;
		//buf[1] = reinterpret_cast<char&>(msgType);
		buf[1] = msgType;
	}
	// Allow connection to the server.
	else
	{
		// Find first available index.
		size_t index = 0;
		vector<Client*>::const_iterator it;
		for (it = _clients.cbegin(); it != _clients.cend(); ++it)
		{
			if (*it == nullptr)
			{
				break;
			}

			++index;
		}

		Client* client = new Client();
		client->_ip = ipbuf;
		client->connected = true;
		client->_id = index;
		client->_sockAddr = (sockaddr*)&fromAddr;
		client->_sockAddrLen = fromLen;
		client->fromAddr = fromAddr;
		_clients.push_back(client);


		if (_clients.size() == MAX_CLIENTS)
		{
			cout << "Both players have connected" << endl;
		}

		msgType = MessageTypes::ConnectionAccepted;
		memset(buf, 0, BUF_LEN);
		//buf[0] = reinterpret_cast<char&>(msgType);
		buf[0] = msgType;
		//buf[1] = reinterpret_cast<char&>(client->_id);
		buf[1] = client->_id;


		cout << "Player connected" << endl;
	}



	// Tell client they have connected to the server.
	if (sendto(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, fromLen) == SOCKET_ERROR)
	{
		printf("Failed to send connection packet. %d\n", WSAGetLastError());
	}
}

#pragma region OLD_TRANFORM
//void updateTransform()
//{
//	char buf[BUF_LEN];
//	struct sockaddr_in fromAddr;
//	int fromLen;
//	fromLen = sizeof(fromAddr);
//
//	memset(buf, 0, BUF_LEN);
//
//	int bytes_received = -1;
//	int sError = -1;
//
//
//	// Reveive transform updates from players.
//	bytes_received = recvfrom(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, &fromLen);
//
//	sError = WSAGetLastError();
//
//	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
//	{
//		//std::cout << "Received: " << buf << std::endl;
//
//		string temp = buf;
//		//std::size_t pos = temp.find('@');
//		//temp = temp.substr(0, pos - 1);
//		//tx = std::stof(temp);
//		//temp = buf;
//		//temp = temp.substr(pos + 1);
//		//ty = std::stof(temp);
//
//		//std::cout << tx << " " << ty << std::endl;
//
//		// Retrieve network id of incomming message.
//		temp = temp.substr(0, 1);
//
//		Vector3 position;
//		Quaternion rotation;
//
//
//		// Player one sent transform data.
//		if (temp == _player1._id)
//		{
//			cout << "Network ID: " << temp << endl;
//			temp = buf;
//
//			parseData(temp, position, rotation);
//
//			// Send data to other player.
//			if (sendto(server_socket, buf, BUF_LEN, 0, (sockaddr*)&_player2.fromAddr, fromLen) == SOCKET_ERROR)
//			{
//				printf("Failed to send transform data. %d\n", WSAGetLastError());
//				//char ipbuf[INET_ADDRSTRLEN];
//				//inet_ntop(AF_INET, &fromAddr, ipbuf, sizeof(ipbuf));
//				//cout << ipbuf << endl;
//			}
//
//			//if (sendto(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, fromLen) == SOCKET_ERROR)
//			//{
//			//	printf("Failed to send transform data. %d\n", WSAGetLastError());
//			//}
//		}
//		// Player two sent transform data.
//		else if (temp == _player2._id)
//		{
//			cout << "Network ID: " << temp << endl;
//			temp = buf;
//
//			parseData(temp, position, rotation);
//
//			//// Send data to other player.
//			if (sendto(server_socket, buf, BUF_LEN, 0, (sockaddr*)&_player1.fromAddr, fromLen) == SOCKET_ERROR)
//			{
//				printf("Failed to send transform data. %d\n", WSAGetLastError());
//				//char ipbuf[INET_ADDRSTRLEN];
//				//inet_ntop(AF_INET, &fromAddr, ipbuf, sizeof(ipbuf));
//				//cout << ipbuf << endl;
//			}
//		}
//		else
//		{
//			cout << "Unkown network id!" << endl;
//			return;
//		}
//	}
//}

void parseData(const string& buf, Vector3& pos, Quaternion& rot)
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

	pos._x = data[0];
	pos._y = data[1];
	pos._z = data[2];

	rot._x = data[3];
	rot._y = data[4];
	rot._z = data[5];
	rot._w = data[6];


	cout << pos._x << " " << pos._y << " " << pos._z << endl;
	cout << rot._x << " " << rot._y << " " << rot._z << " " << rot._w << endl;
}
#pragma endregion

void processTransform(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen)
{

	// Reveive transform updates from players.
		//std::cout << "Received: " << buf << std::endl;

	// Retrieve network id of incomming message.
	INT8 networkID = buf[1];


	// Extract data (FOR DEBUG ONLY).
	size_t posSize = sizeof(float) * 3;
	size_t rotSize = sizeof(float) * 4;
	Vector3 posDebug;
	Quaternion rotDebug;
	memcpy(&posDebug._x, reinterpret_cast<float*>(&buf[2]), posSize);
	memcpy(&rotDebug._x, reinterpret_cast<float*>(&buf[2 + posSize]), rotSize);
	cout << "Msg Type: " << int(buf[0]) << ", ID: " << int(buf[1]) << endl;
	cout << posDebug.toString() << rotDebug.toString();


	// Send data too all other clients.
	Client* client;
	vector<Client*>::const_iterator it;
	for (it = _clients.cbegin(); it != _clients.cend(); ++it)
	{
		client = *it;

		// Don't send back to the same client who sent the data.
		if (client->_id == networkID)
			continue;

		// Send data to other client.
		if (sendto(server_socket, buf, BUF_LEN, 0, (sockaddr*)&(client->fromAddr), fromLen) == SOCKET_ERROR)
		{
			printf("Failed to send transform data. %d\n", WSAGetLastError());
		}
	}
}

int main() 
{
	//Initialize Network
	if (!initNetwork())
		return 1;

	_clients.reserve(MAX_CLIENTS);
	
	///// Game loop /////
	while (true) 
	{
		
		////////////////////
		/*
		CODE TO RECEIVE UPDATES FROM CLIENT GOES HERE...
		*/
		char buf[BUF_LEN];
		//char* buf = new char[BUF_LEN];
		struct sockaddr_in fromAddr;
		int fromLen;
		fromLen = sizeof(fromAddr);

		memset(buf, '\0', BUF_LEN);

		int bytes_received = -1;
		int sError = -1;

		// Receive message from a client.
		bytes_received = recvfrom(server_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, &fromLen);

		sError = WSAGetLastError();


		// Packet recieved
		if (sError != WSAEWOULDBLOCK && bytes_received > 0)
		{
			MessageTypes msgType = static_cast<MessageTypes>(buf[0]);

			switch (msgType)
			{
			case ConnectionAttempt:
				// Only check for incoming connections while there is still room.
				if (_clients.size() != MAX_CLIENTS)
				{
					connectPlayer(buf, fromAddr, fromLen);
				}
				break;
			case TransformData:
				processTransform(buf, fromAddr, fromLen);
				break;
			default:
				cout << "Unexpected message type received!" << endl;
				break;
			}





			//updateTransform();
		}


		////////////////////
	}

	return 0;
}