#include "ClientSide.h"

ClientSide::ClientSide()
{
	_transform = Transform();

	//Initialize Network
	initNetwork();

}

bool ClientSide::initNetwork() {
	//Initialize winsock
	WSADATA wsa;

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

	if (getaddrinfo(SERVER, PORT, &hints, &ptr) != 0) {
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		WSACleanup();
		return 0;
	}

	
	client_socket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

	if (client_socket == INVALID_SOCKET) {
		printf("Failed creating a socket %d\n", WSAGetLastError());
		WSACleanup();
		return 0;
	}

	return 1;
}

void ClientSide::send(const Vector3& position, const Quaternion& rotation)
{
	_transform.send(position, rotation);

	char message[BUF_LEN];

	std::string msg = std::to_string(position.x) + "$" + std::to_string(position.y) + "$" + std::to_string(position.z);
	msg += "@";
	msg += std::to_string(rotation.x) + "$" + std::to_string(rotation.y)
		+ "$" + std::to_string(rotation.z) + "$" + std::to_string(rotation.w);

	strcpy(message, (char*)msg.c_str());

	if (sendto(client_socket, message, BUF_LEN, 0, ptr->ai_addr, ptr->ai_addrlen) == SOCKET_ERROR)
	{
		std::cout << "Sendto() failed...\n" << std::endl;
	}
	std::cout << "sent: " << message << std::endl;
	memset(message, '/0', BUF_LEN);
}

void ClientSide::receive(Vector3& position, Quaternion& rotation)
{
	position = _transform._position;
	rotation = _transform._rotation;
}


