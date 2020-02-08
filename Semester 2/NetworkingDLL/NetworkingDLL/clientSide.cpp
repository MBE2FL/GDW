#include "ClientSide.h"

ClientSide::ClientSide()
{
	_transform = Transform();

	//Initialize Network
	//initNetwork();

}

bool ClientSide::initNetwork(const string& ip) 
{
	//Initialize winsock
	WSADATA wsa;
	_serverIP = ip;

	int error;
	error = WSAStartup(MAKEWORD(2, 2), &wsa);

	if (error != 0) {
		printf("Failed to initialize %d\n", error);
		return 0;
	}

	//Create a client socket

	struct addrinfo hints;

	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_DGRAM;
	hints.ai_protocol = IPPROTO_UDP;

	if (getaddrinfo(_serverIP.c_str(), PORT, &hints, &ptr) != 0) 
	{
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		WSACleanup();
		return 0;
	}

	
	client_socket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

	if (client_socket == INVALID_SOCKET) 
	{
		printf("Failed creating a socket %d\n", WSAGetLastError());
		WSACleanup();
		return 0;
	}

	return 1;
}

bool ClientSide::connectToServer(const char* id)
{
	// Attempt to connect to the server.
	char message[BUF_LEN];

	string msg = "connect";
	msg += id;


	strcpy_s(message, (char*)msg.c_str());

	if (sendto(client_socket, message, BUF_LEN, 0, ptr->ai_addr, ptr->ai_addrlen) == SOCKET_ERROR)
	{
		cout << "Sendto() failed...\n" << endl;
		return false;
	}

	cout << "sent: " << message << endl;

	memset(message, 0, BUF_LEN);


	// Get potential response from server.
	struct sockaddr_in fromAdder;
	int fromLen;
	fromLen = sizeof(fromAdder);
	int bytes_received = -1;
	int sError = -1;


	// Reveive transform updates from players.
	bytes_received = recvfrom(client_socket, message, BUF_LEN, 0, (struct sockaddr*) & fromAdder, &fromLen);

	sError = WSAGetLastError();

	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
	{
		//std::cout << "Received: " << buf << std::endl;

		string temp = message;
		//std::size_t pos = temp.find('@');
		//temp = temp.substr(0, pos - 1);
		//tx = std::stof(temp);
		//temp = buf;
		//temp = temp.substr(pos + 1);
		//ty = std::stof(temp);

		cout << temp << endl;

		// Client connected.
		if (temp == "connected")
		{
			return true;
		}
	}

	// Client failed to connect.
	return false;
}

void ClientSide::sendData(const Vector3& position, const Quaternion& rotation)
{
	_transform.send(position, rotation);

	char message[BUF_LEN];

	std::string msg = std::to_string(position.x) + "$" + std::to_string(position.y) + "$" + std::to_string(position.z);
	msg += "@";
	msg += std::to_string(rotation.x) + "$" + std::to_string(rotation.y)
		+ "$" + std::to_string(rotation.z) + "$" + std::to_string(rotation.w);

	strcpy_s(message, (char*)msg.c_str());

	if (sendto(client_socket, message, BUF_LEN, 0, ptr->ai_addr, ptr->ai_addrlen) == SOCKET_ERROR)
	{
		std::cout << "Sendto() failed...\n" << std::endl;
	}
	std::cout << "sent: " << message << std::endl;
	memset(message, 0, BUF_LEN);
}

void ClientSide::receiveData(Vector3& position, Quaternion& rotation)
{
	position = _transform._position;
	rotation = _transform._rotation;
}


