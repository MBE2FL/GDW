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

	//Create a client sockets.
	if (!initUDP(ip))
	{
		printf("UDP socket failed to initialize! %d\n", WSAGetLastError());
		return false;
	}

	if (!initTCP(ip))
	{
		printf("TCP socket failed to initialize! %d\n", WSAGetLastError());
		return false;
	}


	printf("Client is initialized!\n");


	return 1;
}

bool ClientSide::initUDP(const char* ip)
{
	//Create a client socket

	struct addrinfo hints;

	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_DGRAM;
	hints.ai_protocol = IPPROTO_UDP;

	if (getaddrinfo(_serverIP.c_str(), PORT, &hints, &_ptr) != 0)
	{
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		WSACleanup();
		return false;
	}


	_clientUDPsocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

	if (_clientUDPsocket == INVALID_SOCKET)
	{
		printf("Failed creating a socket %d\n", WSAGetLastError());
		WSACleanup();
		return false;
	}

	// Turn off blocking.
	u_long iMode = 1;
	int iResult = ioctlsocket(_clientUDPsocket, FIONBIO, &iMode);
	if (iResult != NO_ERROR)
	{
		printf("ioctlsocket failed with error: %ld\n", iResult);
	}

	printf("UDP socket is ready!\n");

	return true;
}

bool ClientSide::initTCP(const char* ip)
{
	//Create a client socket

	struct addrinfo hints;

	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	if (getaddrinfo(_serverIP.c_str(), PORT, &hints, &_ptr) != 0)
	{
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		WSACleanup();
		return false;
	}


	_clientTCPsocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	if (_clientTCPsocket == INVALID_SOCKET)
	{
		printf("Failed creating a socket %d\n", WSAGetLastError());
		WSACleanup();
		return false;
	}

	printf("TCP socket is ready!\n");

	return true;
}

void ClientSide::connectToServerTCP()
{
	//Connect to the server
	if (connect(_clientTCPsocket, _ptr->ai_addr, (int)_ptr->ai_addrlen) == SOCKET_ERROR) {
		printf("Unable to connect TCP to server: %d\n", WSAGetLastError());
		closesocket(_clientTCPsocket);
		freeaddrinfo(_ptr);
		WSACleanup();
		return;
	}

	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	MessageTypes msgType;

	buf[0] = MessageTypes::ConnectionAttempt;

	unsigned int timeouts = 0;
	while (timeouts < MAX_TIMEOUTS)
	{
		// Send udp socket info over to server.
		if (sendto(_clientUDPsocket, buf, BUF_LEN, 0, _ptr->ai_addr, _ptr->ai_addrlen) == SOCKET_ERROR)
		{
			printf("Unable to connect UDP to server: %d\n", WSAGetLastError());
			closesocket(_clientUDPsocket);
			freeaddrinfo(_ptr);
			WSACleanup();
			system("pause");
			return;
		}

		// Wait for reply from server of UDP acception.
		timeval timeout;
		timeout.tv_sec = 2;
		timeout.tv_usec = 500000;

		fd_set fds;
		FD_ZERO(&fds);
		FD_SET(_clientTCPsocket, &fds);

		int wsaError = -1;

		wsaError = select(NULL, &fds, NULL, NULL, &timeout);

		

		// Socker error
		if (wsaError == SOCKET_ERROR)
		{
			printf("Select() failed %d\n", WSAGetLastError());
			closesocket(_clientTCPsocket);
			freeaddrinfo(_ptr);
			WSACleanup();
			return;
		}
		// Timeout
		else if (wsaError == 0)
		{
			cout << "UDP connect attempt timeout!" << endl;
			++timeouts;
			continue;
		}
		// Socket ready for reading.
		else
		{
			cout << "UDP Connected: " << wsaError << endl;
			cout << WSAGetLastError() << endl;
			break;
		}
	}


	// Client UDP connection could not be established.
	if (timeouts >= MAX_TIMEOUTS)
	{
		cout << "Server UDP connection could not be established." << endl;
		return;
	}


	memset(buf, 0, BUF_LEN);

	if (recv(_clientTCPsocket, buf, BUF_LEN, 0) > 0)
	{
		msgType = static_cast<MessageTypes>(buf[0]);
		switch (msgType)
		{
		case MessageTypes::ConnectionAccepted:
		{
			_networkID = buf[1];
			//id = _networkID;
			//cout << "ID: " << id << endl;
			cout << "ID: " << int(_networkID) << endl;
			_connected = true;
		}
		break;
		default:
			cout << "Unexpected message type receieved for connection attempt!" << endl;
			//return false;
			return;
			break;
		}
	}
	else 
		printf("recv() error: %d\n", WSAGetLastError());

	// Call c# function.
	//_funcs.connectedToServer();

	//return true;
	return;



	//bool result = false;

	//thread([&id, &result, this]
	//	{
	//		//Connect to the server
	//		if (connect(_clientTCPsocket, _ptr->ai_addr, (int)_ptr->ai_addrlen) == SOCKET_ERROR) {
	//			printf("Unable to connect to server: %d\n", WSAGetLastError());
	//			closesocket(_clientTCPsocket);
	//			freeaddrinfo(_ptr);
	//			WSACleanup();
	//			system("pause");
	//			return;
	//		}


	//		char buf[BUF_LEN];
	//		memset(buf, 0, BUF_LEN);

	//		MessageTypes msgType;


	//		if (recv(_clientTCPsocket, buf, BUF_LEN, 0) > 0)
	//		{
	//			msgType = static_cast<MessageTypes>(buf[0]);
	//			switch (msgType)
	//			{
	//			case MessageTypes::ConnectionAccepted:
	//			{
	//				_networkID = buf[1];
	//				id = _networkID;
	//				cout << "ID: " << id << endl;
	//			}
	//				break;
	//			default:
	//				cout << "Unexpected message type receieved for connection attempt!" << endl;
	//				return;
	//				break;
	//			}
	//		}
	//		else printf("recv() error: %d\n", WSAGetLastError());

	//		result = true;

	//		// Call c# function.
	//		_funcs.connectedToServer();

	//		return;
	//	}
	//);
}

bool ClientSide::connectToServer()
{
	thread tcpConnect(&ClientSide::connectToServerTCP, this);
	//connectToServerTCP();
	tcpConnect.detach();

	return false;


#pragma region OLD_UDP_CONNECT
	INT8 id = 0;
	// Attempt to connect to the server.
	char message[BUF_LEN];
	//char* message = new char[BUF_LEN];
	memset(message, 0, BUF_LEN);

	//string msg = "connect";

	MessageTypes msgType = MessageTypes::ConnectionAttempt;
	//message[0] = reinterpret_cast<char&>(msgType);
	message[0] = msgType;


	//strcpy_s(message, (char*)msg.c_str());

	if (sendto(_clientUDPsocket, message, BUF_LEN, 0, _ptr->ai_addr, _ptr->ai_addrlen) == SOCKET_ERROR)
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


	bytes_received = recvfrom(_clientUDPsocket, message, BUF_LEN, 0, (struct sockaddr*) & fromAdder, &fromLen);

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
#pragma endregion
}

bool ClientSide::queryConnectAttempt(int& id)
{
	if (_connected)
		id = _networkID;

	return _connected;
}

void ClientSide::sendData(const Vector3& position, const Quaternion& rotation)
{
	_transform.send(position, rotation);

	Vector3 pos = position;
	Quaternion rot = rotation;

	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	buf[0] = MessageTypes::TransformMsg;
	buf[1] = _networkID;

	size_t posSize = sizeof(float) * 3;
	size_t rotSize = sizeof(float) * 4;
	//buf[1] = reinterpret_cast<char*>(&pos._x);
	memcpy(&buf[2], reinterpret_cast<char*>(&pos._x), posSize);
	memcpy(&buf[2 + posSize], reinterpret_cast<char*>(&rot._x), rotSize);

	// Failed to send message.
	if (sendto(_clientUDPsocket, buf, BUF_LEN, 0, _ptr->ai_addr, _ptr->ai_addrlen) == SOCKET_ERROR)
	{
		cout << "Sendto() failed...\n" << endl;
	}
	// Successfully sent message.
	else
	{
		Vector3 posDebug;
		Quaternion rotDebug;
		memcpy(&posDebug._x, reinterpret_cast<float*>(&buf[2]), posSize);
		memcpy(&rotDebug._x, reinterpret_cast<float*>(&buf[2 + posSize]), rotSize);
		//cout << "Msg Type: " << int(buf[0]) << ", ID: " << int(buf[1]) << endl;
		//cout << posDebug.toString() << rotDebug.toString();
	}

	char test[BUF_LEN];
	memset(test, 0, BUF_LEN);
	test[0] = MessageTypes::Anim;
	test[1] = _networkID;
	test[2] = int8_t(0);
	int state = 1;
	memcpy(&test[3], &state, sizeof(int));
	if (send(_clientTCPsocket, test, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "TCP Test Error!" << endl;
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

void ClientSide::sendData(const int msgType, const int objID, void* data)
{
	Packet* packet = nullptr;

	MessageTypes _msgType = static_cast<MessageTypes>(msgType);

	switch (_msgType)
	{
	case TransformMsg:
		packet = new TransformPacket(_networkID, objID);
		break;
	case Anim:
		packet = new AnimPacket(_networkID, objID);
		break;
	default:
		break;
	}

	// Failed to create a packet.
	if (!packet)
	{
		cout << "Failed to create and send a packet!" << endl;
		return;
	}


	// Serialize data to be sent.
	packet->serialize(data);


	// Failed to send message.
	if (sendto(_clientUDPsocket, packet->_data, BUF_LEN, 0, _ptr->ai_addr, _ptr->ai_addrlen) == SOCKET_ERROR)
	{
		cout << "Failed to send packet on UDP socket!\n" << endl;
	}
	// Successfully sent message.
	else
	{
		//Vector3 posDebug;
		//Quaternion rotDebug;
		//memcpy(&posDebug._x, reinterpret_cast<float*>(&buf[2]), posSize);
		//memcpy(&rotDebug._x, reinterpret_cast<float*>(&buf[2 + posSize]), rotSize);
		//cout << "Msg Type: " << int(buf[0]) << ", ID: " << int(buf[1]) << endl;
		//cout << posDebug.toString() << rotDebug.toString();
	}

	delete packet;
	packet = nullptr;
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
	bytes_received = recvfrom(_clientUDPsocket, buf, BUF_LEN, 0, (sockaddr*)&fromAddr, &fromLen);

	sError = WSAGetLastError();


	// Received transform data from server.
	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
	{
		std::cout << "Received: " << bytes_received << std::endl;

		MessageTypes msgType = static_cast<MessageTypes>(buf[0]);

		switch (msgType)
		{
		case TransformMsg:
		{
			Vector3 pos;
			Quaternion rot;

			// Retrieve network ID of incomming message.
			INT8 networkID = buf[1];

			if (networkID != _networkID)
			{

				size_t posSize = sizeof(float) * 3;
				size_t rotSize = sizeof(float) * 4;

				memcpy(&pos._x, reinterpret_cast<float*>(&buf[2]), posSize);
				memcpy(&rot._x, reinterpret_cast<float*>(&buf[2 + posSize]), rotSize);

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
		cout << sError << "Bytes Recieved: " << bytes_received <<  endl;
		position = _otherTransform._position;
		rotation = _otherTransform._rotation;
	}
}

#pragma region VOID_RECEIVE
//void ClientSide::receiveData(MessageTypes& msgType, int& objID, void* data)
//{
//	char buf[BUF_LEN];
//	sockaddr_in fromAddr;
//	int fromLen;
//	fromLen = sizeof(fromAddr);
//
//	memset(buf, 0, BUF_LEN);
//
//	int bytes_received = -1;
//	int sError = -1;
//
//
//	// Reveive updates from the server.
//	bytes_received = recvfrom(_clientUDPsocket, buf, BUF_LEN, 0, (sockaddr*)&fromAddr, &fromLen);
//
//	sError = WSAGetLastError();
//
//
//	// Received data from server.
//	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
//	{
//		//MessageTypes msgType = static_cast<MessageTypes>(buf[0]);
//		msgType = static_cast<MessageTypes>(buf[0]);
//
//		switch (msgType)
//		{
//		case TransformMsg:
//		{
//			// Retrieve network ID of incomming message.
//			INT8 networkID = buf[1];
//
//			if (networkID != _networkID)
//			{
//				TransformData transData = TransformData();
//				int8_t _objID = -1;
//
//				Packet* packet = new TransformPacket(buf);
//				packet->deserialize(_objID, &transData);
//
//
//				objID = _objID;
//				data = &transData;
//				memcpy(data, &transData, sizeof(TransformData));
//
//
//				_otherTransform._position = transData._pos;
//				_otherTransform._rotation = transData._rot;
//
//				delete packet;
//				packet = nullptr;
//
//				return;
//			}
//			else
//			{
//				cout << "Own network ID!" << endl;
//				TransformData transData = TransformData();
//				int8_t _objID = -1;
//
//				Packet* packet = new TransformPacket(buf);
//				packet->deserialize(_objID, &transData);
//
//
//
//				objID = _objID;
//				//data = &transData;
//				//memcpy(data, reinterpret_cast<float*>(&transData), sizeof(TransformData));
//				memcpy(data, &transData._pos._x, sizeof(float) * 3);
//				memcpy(&data[3], reinterpret_cast<char*>(&floatData[3]), rotSize);
//
//
//				_otherTransform._position = transData._pos;
//				_otherTransform._rotation = transData._rot;
//
//				cout << transData._pos.toString();
//				cout << transData._rot.toString();
//
//				delete packet;
//				packet = nullptr;
//				return;
//			}
//		}
//		break;
//		// Use previous transform data.
//		default:
//			//position = _otherTransform._position;
//			//rotation = _otherTransform._rotation;
//			break;
//		}
//	}
//	// Use previous transform data.
//	else
//	{
//		//cout << sError << "Bytes Recieved: " << bytes_received << endl;
//		//position = _otherTransform._position;
//		//rotation = _otherTransform._rotation;
//	}
//}
#pragma endregion


void ClientSide::receiveData(MessageTypes& msgType, int& objID, void* data)
{
	char buf[BUF_LEN];
	sockaddr_in fromAddr;
	int fromLen;
	fromLen = sizeof(fromAddr);

	memset(buf, 0, BUF_LEN);

	int bytes_received = -1;
	int sError = -1;


	// Reveive updates from the server.
	bytes_received = recvfrom(_clientUDPsocket, buf, BUF_LEN, 0, (sockaddr*)&fromAddr, &fromLen);

	sError = WSAGetLastError();


	// Received data from server.
	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
	{
		_receiveBuf.insert(_receiveBuf.end(), std::begin(buf), std::end(buf));
		++_receiveBufElements;
	}
	else
	{

	}
}

char* ClientSide::getReceiveData(int& numElements)
{
	// Copy data to c#.
	numElements = _receiveBufElements;
	_receiveBufHandle = new char[_receiveBuf.size()]; // NEED TO CLEANUP
	memcpy(_receiveBufHandle, _receiveBuf.data(), _receiveBuf.size());

	// Clear data on c++.
	_receiveBuf.clear();
	_receiveBufElements = 0;

	return _receiveBufHandle;
}

void ClientSide::receiveUDPData()
{
	char buf[BUF_LEN];
	sockaddr_in fromAddr;
	int fromLen;
	fromLen = sizeof(fromAddr);

	memset(buf, 0, BUF_LEN);

	int bytes_received = -1;
	int sError = -1;


	// Reveive updates from the server.
	bytes_received = recvfrom(_clientUDPsocket, buf, BUF_LEN, 0, (sockaddr*)&fromAddr, &fromLen);

	sError = WSAGetLastError();


	// Received data from server.
	if (sError != WSAEWOULDBLOCK && bytes_received > 0)
	{
		// Retrieve network ID of incomming message.
		int8_t networkID = buf[NET_ID_POS];

		if (networkID == _networkID)
		{
			cout << "Same Network ID" << endl;
			return;
		}


		MessageTypes msgType = static_cast<MessageTypes>(buf[MSG_TYPE_POS]);

		switch (msgType)
		{
		case TransformMsg:
		{
			// Deserialize and store transform data.
			TransformData transData = TransformData();
			int8_t _objID = -1;

			Packet* packet = new TransformPacket(buf);
			//packet->deserialize(_objID, &transData);

			//_transDataBuf.push_back(transData);



			_udpPacketBuf[msgType][_objID] = packet;
			

			// Cleanup.
			//delete packet;
			//packet = nullptr;

			return;
		}
			break;
		case Anim:
			break;
		default:
			break;
		}
	}
	else
	{

	}
}

TransformData* ClientSide::getTransfromPackets(int& transDataElements)
{
	//// Copy data to c#.
	//transDataElements = _transDataBuf.size();
	//_transDataHandle = new TransformData[_transDataBuf.size()]; // NEED TO CLEANUP
	//memcpy(_transDataHandle, _transDataBuf.data(), _transDataBuf.size());

	//// Clear data on c++.
	//_transDataBuf.clear();

	//return _transDataHandle;




#pragma region MapWay
	_transDataHandle = new TransformData[_transDataBuf.size()]; // NEED TO CLEANUP
	unordered_map<int8_t, Packet*> currObjMap;
	Packet* packet = nullptr;
	TransformData transData;
	int8_t objID = -1;

	
	// Search through each type of packet.
	for (auto msgType : _udpPacketBuf)
	{
		currObjMap = msgType.second;

		for(auto obj : currObjMap)
		{
			// Deserialze transform packet.
			transData = TransformData();
			packet = obj.second;
			packet->deserialize(objID, &transData);

			// Store deserialized data.
			_transDataBuf.push_back(transData);

			// CLean up.
			delete packet;
			packet = nullptr;
		}
	}


	// Copy data to c# handle.
	memcpy(_transDataHandle, _transDataBuf.data(), _transDataBuf.size());


	// Clean up.
	_transDataBuf.clear();


	return _transDataHandle;
#pragma endregion
}

void ClientSide::transformPacketCleanUp()
{
	delete[] _transDataHandle;
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

void ClientSide::setFuncs(const CS_to_Plugin_Functions& funcs)
{
	_funcs = funcs;
}
