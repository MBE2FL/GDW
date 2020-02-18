#include "ClientSide.h"

ClientSide::ClientSide()
{
	_transform = Transform();
	_otherTransform = Transform();

	//Initialize Network
	//initNetwork();

}

bool ClientSide::initNetwork(const char* ip)
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

bool ClientSide::connectToServer(int& id)
{
	// Attempt to connect to the server.
	char message[BUF_LEN];
	//char* message = new char[BUF_LEN];
	memset(message, 0, BUF_LEN);

	//string msg = "connect";

	MessageTypes msgType = MessageTypes::ConnectionAttempt;
	//message[0] = reinterpret_cast<char&>(msgType);
	message[0] = msgType;


	//strcpy_s(message, (char*)msg.c_str());

	if (sendto(client_socket, message, BUF_LEN, 0, ptr->ai_addr, ptr->ai_addrlen) == SOCKET_ERROR)
	{
		cout << "Connection attempt failed to send!" << endl;
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

		//string temp = message;
		//std::size_t pos = temp.find('@');
		//temp = temp.substr(0, pos - 1);
		//tx = std::stof(temp);
		//temp = buf;
		//temp = temp.substr(pos + 1);
		//ty = std::stof(temp);

		//msgType = reinterpret_cast<MessageTypes&>(message[0]);
		msgType = static_cast<MessageTypes>(message[0]);

		switch (msgType)
		{
		case MessageTypes::ConnectionAccepted:
		{
			cout << "Connection successful" << endl;

			//_networkID = reinterpret_cast<INT8&>(message[1]);
			_networkID = message[1];
			cout << "ID: " << _networkID << endl;
			char msg = _networkID;
			cout << "ID: " << msg << endl;
			cout << "ID: " << message[1] << endl;
			id = _networkID;
			cout << "ID: " << id << endl;

			return true;
		}
			break;
		default:
			cout << "Incorrect message type receieved! " << msgType << endl;
			break;
		}
	}

	// Client failed to connect.
	cout << "Failed to connect" << endl;
	return false;
}

void ClientSide::sendData(const Vector3& position, const Quaternion& rotation)
{
	_transform.send(position, rotation);

	Vector3 pos = position;
	Quaternion rot = rotation;

	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	buf[0] = MessageTypes::TransformData;
	buf[1] = _networkID;

	size_t posSize = sizeof(float) * 3;
	size_t rotSize = sizeof(float) * 4;
	//buf[1] = reinterpret_cast<char*>(&pos._x);
	memcpy(&buf[2], reinterpret_cast<char*>(&pos._x), posSize);
	memcpy(&buf[2 + posSize], reinterpret_cast<char*>(&rot._x), rotSize);

	// Failed to send message.
	if (sendto(client_socket, buf, BUF_LEN, 0, ptr->ai_addr, ptr->ai_addrlen) == SOCKET_ERROR)
	{
		cout << "Sendto() failed...\n" << endl;
	}
	// Successfully sent message.
	else
	{
		cout << "sent: " << buf << endl;
		string bufStr = buf;
		cout << "sent:" << bufStr << endl;

		Vector3 posDebug;
		Quaternion rotDebug;
		memcpy(&posDebug._x, reinterpret_cast<float*>(&buf[2]), posSize);
		memcpy(&rotDebug._x, reinterpret_cast<float*>(&buf[2 + posSize]), rotSize);
		cout << "Msg Type: " << int(buf[0]) << ", ID: " << int(buf[1]) << endl;
		cout << posDebug.toString() << rotDebug.toString();
	}



	//string msg = _networkID + " X " + to_string(position._x)
	//						+ " Y " + to_string(position._y)
	//						+ " Z " + to_string(position._z);
	//msg += " X " + to_string(rotation._x) 
	//		+ " Y " + to_string(rotation._y)
	//		+ " Z " + to_string(rotation._z)
	//		+ " W " + to_string(rotation._w);

	//Vector3 pos = position;
	//Quaternion rot = rotation;
	//string msg = reinterpret_cast<char*>(&_networkID);
	//msg += reinterpret_cast<char*>(&pos._x);
	//msg += reinterpret_cast<char*>(&pos._y);
	//msg += reinterpret_cast<char*>(&pos._z);
	//msg += reinterpret_cast<char*>(&rot._w);
	//msg += reinterpret_cast<char*>(&rot._x);
	//msg += reinterpret_cast<char*>(&rot._y);
	//msg += reinterpret_cast<char*>(&rot._z);

	//strcpy_s(buf, (char*)msg.c_str());

	//// Failed to send message.
	//if (sendto(client_socket, buf, BUF_LEN, 0, ptr->ai_addr, ptr->ai_addrlen) == SOCKET_ERROR)
	//{
	//	cout << "Sendto() failed...\n" << endl;
	//}
	//// Successfully sent message.
	//else
	//{
	//	cout << "sent: " << buf << endl;
	//}
	//

	//memset(buf, 0, BUF_LEN);
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
		tempPos._x = -6.0f;
		tempPos._y = 0.0f;
		tempPos._z = 6.0f;
		Quaternion tempRot;
		tempRot._x = 0.0f;
		tempRot._y = 0.0f;
		tempRot._z = 0.0f;
		tempRot._w = 1.0f;

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

		MessageTypes msgType = static_cast<MessageTypes>(buf[0]);

		switch (msgType)
		{
		case TransformData:
		{
			Vector3 pos;
			Quaternion rot;

			// Retrieve network ID of incomming message.
			INT8 networkID = buf[1];

			if (networkID != _networkID)
			{

				size_t posSize = sizeof(float) * 3;
				size_t rotSize = sizeof(float) * 4;

				memcpy(&position._x, reinterpret_cast<float*>(&buf[2]), posSize);
				memcpy(&rotation._x, reinterpret_cast<float*>(&buf[2 + posSize]), rotSize);

				cout << "Msg Type: " << int(buf[0]) << ", ID: " << int(buf[1]) << endl;
				cout << pos.toString() << rot.toString();

				position = pos;
				rotation = rot;

				_otherTransform._position = pos;
				_otherTransform._rotation = rot;

				return;
			}
			else
			{
				cout << "Own network ID!" << endl;
				return;
			}
		}
			break;
			// Use previous transform data.
		default:
			position = _otherTransform._position;
			rotation = _otherTransform._rotation;
			break;
		}
	}
	// Use previous transform data.
	else
	{
		position = _otherTransform._position;
		rotation = _otherTransform._rotation;
	}
}

void ClientSide::parseData(const string& buf, Vector3& pos, Quaternion& rot)
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
