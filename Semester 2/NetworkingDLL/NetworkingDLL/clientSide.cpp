#include "ClientSide.h"

ClientSide::ClientSide()
{
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
	//u_long iMode = 1;
	//int iResult = ioctlsocket(_clientUDPsocket, FIONBIO, &iMode);
	//if (iResult != NO_ERROR)
	//{
	//	printf("ioctlsocket failed with error: %ld\n", iResult);
	//}

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

void ClientSide::networkCleanup()
{
	freeaddrinfo(_ptr);
	closesocket(_clientUDPsocket);
	closesocket(_clientTCPsocket);
	WSACleanup();
	cout << "Network Cleanup" << endl;
}

void ClientSide::connectToServer(const char* ip)
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

	buf[PCK_TYPE_POS] = PacketTypes::ConnectionAttempt;
		
	// Send udp socket info over to server.
	if (sendto(_clientUDPsocket, buf, BUF_LEN, 0, _ptr->ai_addr, _ptr->ai_addrlen) == SOCKET_ERROR)
	{
		printf("Unable to connect UDP to server: %d\n", WSAGetLastError());
		closesocket(_clientUDPsocket);
		freeaddrinfo(_ptr);
		WSACleanup();
		return;
	}


	clock_t startClock = clock();
	clock_t startTimerClock = clock();
	totalConnectAttemptTime = 0;
	int sendTimer = 0;
	while (!_connected && (totalConnectAttemptTime <= MAX_CONNECT_ATTEMPT_TIME))
	{
		_status = ConnectionStatus::Connecting;
		totalConnectAttemptTime = (clock() - startClock) / CLOCKS_PER_SEC;
		sendTimer = (clock() - startTimerClock) / CLOCKS_PER_SEC;

		if (sendTimer >= 4)
		{
			cout << "UDP Connect Attempt Timeout!" << endl;

			// Send udp socket info over to server.
			if (sendto(_clientUDPsocket, buf, BUF_LEN, 0, _ptr->ai_addr, _ptr->ai_addrlen) == SOCKET_ERROR)
			{
				printf("Unable to connect UDP to server: %d\n", WSAGetLastError());
				closesocket(_clientUDPsocket);
				freeaddrinfo(_ptr);
				WSACleanup();
				return;
			}

			startTimerClock = clock();
		}
	}
}

void ClientSide::processConnectAttempt(PacketTypes pckType, char buf[BUF_LEN])
{
	if (pckType == PacketTypes::ConnectionAccepted)
	{
		_networkID = buf[NET_ID_POS];
		_connected = true;
		_status = ConnectionStatus::Connected;
		cout << "Connected to the server!" << endl;
	}
	else if (pckType == PacketTypes::ConnectionFailed)
	{
		totalConnectAttemptTime = MAX_CONNECT_ATTEMPT_TIME + 1;
		_status = ConnectionStatus::ConnectionFailedStatus;
		cout << "Failed to connect to the server!" << endl;
	}
	else
	{
		totalConnectAttemptTime = MAX_CONNECT_ATTEMPT_TIME + 1;
		_status = ConnectionStatus::ServerFullStatus;
		cout << "The Server is full!" << endl;
	}
}

ConnectionStatus ClientSide::queryConnectAttempt(int& id)
{
	if (_connected)
	{
		id = _networkID;
	}

	return _status;
}

PacketTypes ClientSide::queryEntityRequest()
{
	// Ask server if it wants the starting entity list or just the client's required list.
	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	buf[PCK_TYPE_POS] = PacketTypes::EntitiesQuery;
	buf[NET_ID_POS] = _networkID;

	if (send(_clientTCPsocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "Unable to query server for which entity list to provide!" << endl;
		return PacketTypes::ErrorMsg;
	}


	// Recieve server's reply.
	int bytesReceived = -1;
	int wsaError = -1;
	memset(buf, 0, BUF_LEN);

	bytesReceived = recv(_clientTCPsocket, buf, BUF_LEN, 0);

	wsaError = WSAGetLastError();

	if (bytesReceived > 0)
	{
		cout << "Server is requesting the " << static_cast<int>(buf[PCK_TYPE_POS]) << " entity list." << endl;
		return static_cast<PacketTypes>(buf[PCK_TYPE_POS]);
	}
	else
	{
		return PacketTypes::EmptyMsg;
	}
}

bool ClientSide::sendStarterEntities(EntityData* entities, int numEntities)
{
	//buf[PCK_TYPE_POS] = MessageTypes::EntitiesStart;
	//buf[NET_ID_POS] = _networkID;
	//buf[DATA_START_POS] = numEntities;
	//memcpy(&buf[DATA_START_POS + 1], entities, sizeof(EntityData) * numEntities);

	EntityPacket packet = EntityPacket(PacketTypes::EntitiesStart, _networkID, numEntities);
	packet.serialize(entities);

	if (send(_clientTCPsocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "Failed to send starter entities to the server!" << endl;
		return false;
	}
	else
	{
		//cout << "Number of entities: " << numEntities << endl;
		//cout << "Entity[0]: " << int(entities[0]._entityPrefabType) << endl;
		cout << "Sent starter entities to the server." << endl;
	}


	// Receive server assigned entity ids.
	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);
	int bytesReceived = -1;

	bytesReceived = recv(_clientTCPsocket, buf, BUF_LEN, 0);


	if (bytesReceived > 0)
	{
		PacketTypes pckType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);

		if (pckType != PacketTypes::EntityIDs)
		{
			cout << "Was expecting to receive server assigned entity ids!" << endl;
			return false;
		}

		int8_t numEntitesReturned = buf[DATA_START_POS];

		if (numEntitesReturned != numEntities)
		{
			cout << "Was expecting to receive, " << int(numEntities) << " server assigned entity ids, but received " << int(numEntitesReturned)<<  " instead!" << endl;
			return false;
		}

		cout << "Received " << int(numEntitesReturned) << " server assigned entity ids." << endl;

		if (numEntities > 0)
		{
			int8_t* entityIDs = new int8_t[numEntities];
			memcpy(entityIDs, &buf[DATA_START_POS + 1], numEntities);
			for (int i = 0; i < numEntities; ++i)
			{
				entities[i]._entityID = entityIDs[i];
			}

			delete[] entityIDs;
			entityIDs = nullptr;
		}
	}
	else
	{
		cout << "Failed to receive server assigned entity ids! " << WSAGetLastError() << endl;
		return false;
	}


	return true;
}

bool ClientSide::sendRequiredEntities(EntityData* entities, int& numEntities, int& numServerEntities)
{
	EntityPacket packet = EntityPacket(PacketTypes::EntitiesRequired, _networkID, numEntities);
	packet.serialize(entities);

	if (send(_clientTCPsocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "Failed to send required entities to the server!" << endl;
		return false;
	}
	else
	{
		//cout << "Number of entities: " << numEntities << endl;
		//cout << "Entity[0]: " << int(entities[0]._entityPrefabType) << endl;
		cout << "Sent required entities to the server." << endl;
	}


	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	// Receive any pre-existing server entities.
	while (true)
	{
		if (recv(_clientTCPsocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			printf("Failed to receive any pre-existing entities from the server. %d\n", WSAGetLastError());
		}
		else
		{
			if (buf[PCK_TYPE_POS] != PacketTypes::EntitiesUpdate)
				continue;

			cout << "Received pre-existing entities from the server." << endl;

			EntityPacket serverEntityPacket = EntityPacket(buf);
			_numEntitiesReceived = serverEntityPacket.getNumEntities();
			numServerEntities = _numEntitiesReceived;
			EntityData* serverEntityData = new EntityData[_numEntitiesReceived];

			serverEntityPacket.deserialize(serverEntityData);

			//memcpy(&entities[numEntities], &serverEntityPacket._data[DATA_START_POS + 1], numberServerEntities * sizeof(EntityData));

			for (int i = 0; i < _numEntitiesReceived; ++i)
			{
				cout << "Server Entity: " << static_cast<int>(serverEntityData[i]._entityID) << endl;
			}

			_receivedEntitiesBuf = new EntityData[_numEntitiesReceived];
			memcpy(_receivedEntitiesBuf, &serverEntityPacket._data[DATA_START_POS + 1], sizeof(EntityData) * _numEntitiesReceived);

			break;
		}
	}


	// Receive server assigned entity ids.
	//char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);
	int bytesReceived = -1;

	while (true)
	{
		bytesReceived = recv(_clientTCPsocket, buf, BUF_LEN, 0);

		if (bytesReceived > 0)
		{
			PacketTypes pckType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);
			//cout << "Ent ID Msg Type: " << int(buf[PCK_TYPE_POS]) << endl;

			if (pckType != PacketTypes::EntityIDs)
			{
				cout << "Was expecting to receive server assigned entity ids!" << endl;

				continue;
			}

			//numEntities = buf[DATA_START_POS] - '0';

			int8_t numEntitiesReturned = buf[DATA_START_POS];
			numEntities = numEntitiesReturned;
			cout << "Received " << static_cast<int>(numEntitiesReturned) << " server assigned entity ids." << endl;

			if (numEntities > 0)
			{
				int8_t* entityIDs = new int8_t[numEntities];
				memcpy(entityIDs, &buf[DATA_START_POS + 1], numEntities);
				for (int i = 0; i < numEntities; ++i)
				{
					entities[i]._entityID = entityIDs[i];
				}

				delete[] entityIDs;
				entityIDs = nullptr;
			}

			break;
		}
		else
		{
			cout << "Failed to receive server assigned entity ids! " << WSAGetLastError() << endl;
			return false;
		}
	}


	return true;
}

void ClientSide::getServerEntities(EntityData* serverEntities)
{
	memcpy(serverEntities, _receivedEntitiesBuf, sizeof(EntityData) * _numEntitiesReceived);

	delete[] _receivedEntitiesBuf;
	_receivedEntitiesBuf = nullptr;
}

void ClientSide::sendData(const PacketTypes pckType, void* data)
{
	Packet* packet = nullptr;
	bool udpPacket = true;

	switch (pckType)
	{
	case TransformMsg:
		packet = new TransformPacket(_networkID);
		break;
	case Anim:
		packet = new AnimPacket(_networkID);
		udpPacket = false;
		break;
	case Score:
		packet = new ScorePacket(_networkID, 1);
		udpPacket = false;
		break;
	case LobbyChat:
		packet = new ChatPacket(_networkID);
		udpPacket = false;
		break;
	case LobbyTeamName:
		packet = new ChatPacket(pckType, _networkID);
		udpPacket = false;
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


	// Send packet.
	if (udpPacket)
	{
		if (sendto(_clientUDPsocket, packet->_data, BUF_LEN, 0, _ptr->ai_addr, _ptr->ai_addrlen) == SOCKET_ERROR)
		{
			cout << "Failed to send packet on UDP socket!" << endl;
		}
	}
	else
	{
		if (send(_clientTCPsocket, packet->_data, BUF_LEN, 0) == SOCKET_ERROR)
		{
			cout << "Failed to send packet on TCP socket!" << endl;
		}
	}


	delete packet;
	packet = nullptr;
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
	if (sError != SOCKET_ERROR && bytes_received > 0)
	{
		// Retrieve network ID of incomming message.
		int8_t networkID = buf[NET_ID_POS];

		if (networkID == _networkID)
		{
			cout << "Same Network ID, UDP, Msg Type: " << int(int8_t(buf[PCK_TYPE_POS])) << endl;
			return;
		}


		PacketTypes pckType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);

		switch (pckType)
		{
		case TransformMsg:
		{
			// Deserialize and store transform data.
			TransformPacket packet = TransformPacket(buf);
			TransformData transData = TransformData();

			packet.deserialize(&transData);

			//_transDataBuf.push_back(transData);
			_transDataBuf.emplace_back(transData);

			//_udpPacketBuf[pckType][_objID] = packet;
			
			//cout << "Transform received for object: " << static_cast<int>(transData._entityID) << endl;
			return;
		}
			break;
		default:
			break;
		}
	}
	else
	{
		//cout << "UDP Receive Error, " << WSAGetLastError() << endl;
	}
}

void ClientSide::receiveTCPData()
{
	char buf[BUF_LEN];

	memset(buf, 0, BUF_LEN);

	int bytesReceived = -1;
	int wsaError = -1;


	// Reveive updates from the server.
	bytesReceived = recv(_clientTCPsocket, buf, BUF_LEN, 0);

	wsaError = WSAGetLastError();


	// Received data from server.
	if (bytesReceived > 0)
	{
		// Retrieve network ID of incomming message.
		int8_t networkID = buf[NET_ID_POS];

		if (networkID == _networkID)
		{
			cout << "Same Network ID, TCP, Msg Type: " << int(int8_t(buf[PCK_TYPE_POS])) << endl;
			return;
		}


		PacketTypes pckType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);

		switch (pckType)
		{
		case ConnectionAttempt:
			break;
		case ConnectionAccepted:
			break;
		case ConnectionFailed:
			break;
		case ServerFull:
			break;
		case Anim:
		{
			// Deserialize and store anim data.
			AnimPacket packet = AnimPacket(buf);
			AnimData animData;

			packet.deserialize(&animData);

			cout << "Received entity " << static_cast<int>(animData._entityID) << " anim packet. EID: " << static_cast<int>(animData._entityID) << ", state: " << animData._state << endl;

			_animDataBuf.emplace_back(animData);
			return;
		}
		case EntitiesQuery:
			break;
		case EntitiesStart:
			break;
		case EntitiesNoStart:
			break;
		case EntitiesRequired:
			break;
		case EntitiesUpdate:
		{
			// Deserialize and store entity data.
			EntityPacket packet = EntityPacket(buf);
			int8_t numEntities = packet.getNumEntities();
			EntityData* entityData = new EntityData[numEntities];

			packet.deserialize(entityData);

			cout << "Received " << static_cast<int>(numEntities) << " server entities, from another client." << endl;

			if (numEntities > 0)
			{
				_entityDataBuf.insert(_entityDataBuf.end(), entityData, entityData + numEntities);

				delete[] entityData;
			}
			return;
		}
		case EntityIDs:
			break;
		case EmptyMsg:
			break;
		case ErrorMsg:
			break;
		default:
			break;
		}
	}
	else if (wsaError == SOCKET_ERROR)
	{
		//cout << "TCP Receive Error, " << WSAGetLastError() << endl;
	}
}

void ClientSide::getPacketHandleSizes(int& transDataElements, int& animDataElements, int& entityDataElements)
{



#pragma region OldMap
	//unordered_map<int8_t, Packet*> currObjMap;


	//Packet* packet = nullptr;
	////PacketData* packetData;
	//TransformData transData;
	//AnimData animData;


	//int8_t objID = -1;

	//// Search through each type of packet.
	//for (auto pckType : _udpPacketBuf)
	//{
	//	currObjMap = pckType.second;

	//	for (auto obj : currObjMap)
	//	{
	//		// Deserialze transform packet.
	//		switch (pckType.first)
	//		{
	//		case TransformMsg:
	//		{
	//			transData = TransformData();
	//			packet = obj.second;
	//			packet->deserialize(objID, &transData);

	//			// Store deserialized data.
	//			_transDataBuf.push_back(transData);
	//		}
	//		break;
	//		case Anim:
	//		{
	//			animData = AnimData();
	//			packet = obj.second;
	//			packet->deserialize(objID, &animData);

	//			// Store deserialized data.
	//			_animDataBuf.push_back(animData);
	//		}
	//		break;
	//		default:
	//			continue;
	//			//break;
	//		}


	//		// CLean up.
	//		delete packet;
	//		packet = nullptr;
	//	}
	//}
#pragma endregion



	transDataElements = _transDataBuf.size();
	animDataElements = _animDataBuf.size();
	entityDataElements = _entityDataBuf.size();
}

void ClientSide::getPacketHandles(void* dataHandle)
{
	// Copy data to c# handle.
	char* byteDatahandle = reinterpret_cast<char*>(dataHandle);
	size_t offset = 0;

	memcpy(byteDatahandle, _transDataBuf.data(), sizeof(TransformData) * _transDataBuf.size());
	offset += sizeof(TransformData) * _transDataBuf.size();

	memcpy(&byteDatahandle[offset], _animDataBuf.data(), sizeof(AnimData) * _animDataBuf.size());
	offset += sizeof(AnimData) * _animDataBuf.size();

	memcpy(&byteDatahandle[offset], _entityDataBuf.data(), sizeof(EntityData) * _entityDataBuf.size());


	//for (const auto& pckKV : _tcpPacketBuf)
	//{
	//	switch (pckKV.first)
	//	{
	//	case Anim:
	//		for (const auto& objKV : pckKV.second)
	//		{
	//			objIDsBuf.emplace_back(objKV.first);


	//			vector<Packet*> animPackets = objKV.second;
	//			AnimData animData;
	//			int8_t objID;

	//			for (Packet* animPck : animPackets)
	//			{
	//				animPck->deserialize(objID, &animData);

	//			}
	//		}
	//		break;
	//	case EntitiesUpdate:
	//		break;
	//	case EntityIDs:
	//		break;
	//	default:
	//		break;
	//	}
	//}


	// Clean up.
	_transDataBuf.clear();
	_animDataBuf.clear();
	//_udpPacketBuf.clear();
	_entityDataBuf.clear();
}

void ClientSide::getScores(int& numScores)
{
	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	buf[PCK_TYPE_POS] = PacketTypes::ClientScoresRequest;
	buf[NET_ID_POS] = _networkID;

	// Send packet.
	if (send(_clientTCPsocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "Failed to send ClientScoresRequest packet on TCP socket!" << endl;
	}

	memset(buf, 0, BUF_LEN);
	int bytesRecieved = -1;
	int wsaError = -1;

	bytesRecieved = recv(_clientTCPsocket, buf, BUF_LEN, 0);

	wsaError = WSAGetLastError();

	if (bytesRecieved > 0)
	{
		ScorePacket packet = ScorePacket(buf);
		numScores = packet.getNumScores();
		_scoresBuf = new ScoreData[numScores];
		packet.deserialize(_scoresBuf);
	}
	else
	{
		cout << "Failed to receive scores packet on TCP socket! " << wsaError << endl;
	}
}

ScoreData* ClientSide::getScoresHandle()
{
	return _scoresBuf;
}

void ClientSide::cleanupScoresHandle()
{
	delete _scoresBuf;
	_scoresBuf = nullptr;
}

void ClientSide::receiveLobbyData()
{
	char buf[BUF_LEN];

	memset(buf, 0, BUF_LEN);

	int bytesReceived = -1;
	int wsaError = -1;


	// Reveive updates from the server.
	bytesReceived = recv(_clientTCPsocket, buf, BUF_LEN, 0);

	wsaError = WSAGetLastError();


	// Received data from server.
	if (bytesReceived > 0)
	{
		PacketTypes pckType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);

		switch (pckType)
		{
		case ConnectionAccepted:
			processConnectAttempt(pckType, buf);
			break;
		case ConnectionFailed:
			processConnectAttempt(pckType, buf);
			break;
		case ServerFull:
			processConnectAttempt(pckType, buf);
			break;
		case EntitiesQuery:
			break;
		case EntitiesStart:
			break;
		case EntitiesNoStart:
			break;
		case EntitiesRequired:
			break;
		case EntitiesUpdate:
		{
			// Retrieve network ID of incomming message.
			int8_t networkID = buf[NET_ID_POS];

			if (networkID == _networkID)
			{
				cout << "Same Network ID, Lobby TCP, Msg Type: " << int(int8_t(buf[PCK_TYPE_POS])) << endl;
				return;
			}

			// Deserialize and store entity data.
			EntityPacket packet = EntityPacket(buf);
			int8_t numEntities = packet.getNumEntities();
			EntityData* entityData = new EntityData[numEntities];

			packet.deserialize(entityData);

			cout << "Received " << static_cast<int>(numEntities) << " server entities, from another client." << endl;

			if (numEntities > 0)
			{
				_entityDataBuf.insert(_entityDataBuf.end(), entityData, entityData + numEntities);

				delete[] entityData;
			}
			return;
		}
		case LobbyChat:
		{
			// Retrieve network ID of incomming message.
			int8_t networkID = buf[NET_ID_POS];

			if (networkID == _networkID)
			{
				cout << "Same Network ID, Lobby TCP, Msg Type: " << int(int8_t(buf[PCK_TYPE_POS])) << endl;
				return;
			}

			ChatPacket packet = ChatPacket(buf);
			ChatData chatData = ChatData();

			packet.deserialize(&chatData);

			cout << "Received chat msg from " << static_cast<int>(networkID) << ": " << chatData._msg << endl;
			delete[] chatData._msg;

			_chatDataBuf.emplace_back(chatData);
			break;
		}
		case LobbyTeamName:
		{
			// Retrieve network ID of incomming message.
			int8_t networkID = buf[NET_ID_POS];

			if (networkID == _networkID)
			{
				cout << "Same Network ID, Lobby TCP, Msg Type: " << int(int8_t(buf[PCK_TYPE_POS])) << endl;
				return;
			}

			ChatPacket packet = ChatPacket(buf);
			_teamNameBuf = new ChatData();	// CLEAN UP

			packet.deserialize(_teamNameBuf);	// CLEAN UP

			cout << "Received team name msg from " << static_cast<int>(networkID) << ": " << _teamNameBuf->_msg << endl;
			break;
			break;
		}
		case EmptyMsg:
			cout << "Empty message received." << endl;
			break;
		case ErrorMsg:
			cout << "Error message received: " << wsaError << endl;
			break;
		default:
			break;
		}
	}
	else if (wsaError == SOCKET_ERROR)
	{
		//cout << "TCP Receive Error, " << WSAGetLastError() << endl;
	}
}

void ClientSide::getNumLobbyPackets(int& numMsgs, bool& newTeamNameMsg)
{
	numMsgs = _chatDataBuf.size();
	newTeamNameMsg = _teamNameBuf;
}

void ClientSide::getLobbyPacketHandles(void* dataHandle)
{
	// Copy data to c# handle.
	char* byteDatahandle = reinterpret_cast<char*>(dataHandle);
	size_t offset = 0;

	memcpy(byteDatahandle, _chatDataBuf.data(), sizeof(ChatData) * _chatDataBuf.size());
	offset += sizeof(ChatData) * _chatDataBuf.size();

	if (_teamNameBuf)
	{
		memcpy(&byteDatahandle[offset], _teamNameBuf, sizeof(ChatData));
		offset += sizeof(ChatData);

		// Clean up
		delete[] _teamNameBuf->_msg;
		delete _teamNameBuf;
		_teamNameBuf = nullptr;
	}


	// Clean up.
	_chatDataBuf.clear();
}

void ClientSide::setFuncs(const CS_to_Plugin_Functions& funcs)
{
	_funcs = funcs;
}
