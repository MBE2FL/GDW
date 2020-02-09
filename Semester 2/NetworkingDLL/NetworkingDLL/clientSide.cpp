#include "ClientSide.h"

ClientSide::ClientSide()
{
	_transform = Transform();
	_otherTransform = Transform();

	//Initialize Network
	//initNetwork();

}

bool ClientSide::initNetwork(const char* ip, const char* id)
{
	_networkID = id;

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

	string msg = _networkID + " X " + to_string(position.x)
							+ " Y " + to_string(position.y)
							+ " Z " + to_string(position.z);
	msg += " X " + to_string(rotation.x) 
			+ " Y " + to_string(rotation.y)
			+ " Z " + to_string(rotation.z)
			+ " W " + to_string(rotation.w);

	//Vector3 pos = position;
	//Quaternion rot = rotation;
	//string msg = reinterpret_cast<char*>(&_networkID);
	//msg += reinterpret_cast<char*>(&pos.x);
	//msg += reinterpret_cast<char*>(&pos.y);
	//msg += reinterpret_cast<char*>(&pos.z);
	//msg += reinterpret_cast<char*>(&rot.w);
	//msg += reinterpret_cast<char*>(&rot.x);
	//msg += reinterpret_cast<char*>(&rot.y);
	//msg += reinterpret_cast<char*>(&rot.z);

	strcpy_s(message, (char*)msg.c_str());

	// Failed to send message.
	if (sendto(client_socket, message, BUF_LEN, 0, ptr->ai_addr, ptr->ai_addrlen) == SOCKET_ERROR)
	{
		cout << "Sendto() failed...\n" << endl;
	}
	// Successfully sent message.
	else
	{
		cout << "sent: " << message << endl;
	}
	

	memset(message, 0, BUF_LEN);
}

void ClientSide::receiveData(Vector3& position, Quaternion& rotation)
{
	char buf[BUF_LEN];
	sockaddr_in fromAddr;
	int fromLen;
	fromLen = sizeof(fromAddr);

	memset(buf, 0, BUF_LEN);

	int bytes_received = -1;
	int sError = -1;


	// Reveive transform updates from the server.
	// Turn off blocking.
	u_long iMode = 1;
	int iResult = ioctlsocket(client_socket, FIONBIO, &iMode);
	if (iResult != NO_ERROR)
	{
		//printf("ioctlsocket failed with error: %ld\n", iResult);
		Vector3 tempPos;
		tempPos.x = -6.0f;
		tempPos.y = 0.0f;
		tempPos.z = 6.0f;
		Quaternion tempRot;
		tempRot.x = 0.0f;
		tempRot.y = 0.0f;
		tempRot.z = 0.0f;
		tempRot.w = 1.0f;

		position = tempPos;
		rotation = tempRot;

		return;
	}

	bytes_received = recvfrom(client_socket, buf, BUF_LEN, 0, (sockaddr*)&fromAddr, &fromLen);

	sError = WSAGetLastError();

	// Received transform data from server.
	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
	{
		//std::cout << "Received: " << buf << std::endl;

		string temp = buf;


		// Retrieve network id of incomming message.
		temp = temp.substr(0, 1);

		Vector3 pos;
		Quaternion rot;


		// Server sent other client's transform data.
		if (temp != _networkID)
		{
			cout << "Network ID: " << temp << endl;
			temp = buf;

			parseData(temp, pos, rot);
		}
		// Server sent client their own transform data.
		else
		{
			cout << "Own network ID!" << endl;
			return;
		}


		position = pos;
		rotation = rot;
		//Vector3 tempPos;
		//tempPos.x = 3.0f;
		//tempPos.y = 3.0f;
		//tempPos.z = 3.0f;
		//Quaternion tempRot;
		//tempRot.x = 0.0f;
		//tempRot.y = 0.0f;
		//tempRot.z = 0.0f;
		//tempRot.w = 1.0f;

		//position = tempPos;
		//rotation = tempRot;
		_otherTransform._position = pos;
		_otherTransform._rotation = rot;

		return;
	}
	// Use previous transform data.
	else
	{
		position = _otherTransform._position;
		rotation = _otherTransform._rotation;
	}
}

void ClientSide::parseData(const string& buf, Vector3& pos, Quaternion rot)
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
