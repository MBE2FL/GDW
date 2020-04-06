#include "ClientSide.h"

ClientSide::ClientSide()
{
	memset(_entityIDsBuf, 0, BUF_LEN);
	_entityIDsBuf[PCK_TYPE_POS] = EmptyMsg;
}

bool ClientSide::initNetwork()
{
	//Initialize winsock
	WSADATA wsa;

	int error;
	error = WSAStartup(MAKEWORD(2, 2), &wsa);

	if (error != 0) {
		printf("Failed to initialize %d\n", error);
		return 0;
	}

	//Create a client sockets.
	if (!initUDP())
	{
		printf("UDP socket failed to initialize! %d\n", WSAGetLastError());
		return false;
	}

	if (!initTCP())
	{
		printf("TCP socket failed to initialize! %d\n", WSAGetLastError());
		return false;
	}


	printf("Client is initialized!\n");


	return 1;
}

bool ClientSide::initUDP()
{
	// Create a client UDP socket.
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

bool ClientSide::initTCP()
{
	// Create a client TCP socket.
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
	_status = ConnectionStatus::Connecting;

	_serverIP = ip;

	addrinfo hints;
	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	if (getaddrinfo(_serverIP.c_str(), PORT, &hints, &_ptr) != 0)
	{
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		//WSACleanup();
		_status = ConnectionStatus::ConnectionFailedStatus;
		return;
	}


	//Connect to the server
	if (connect(_clientTCPsocket, _ptr->ai_addr, (int)_ptr->ai_addrlen) == SOCKET_ERROR) {
		printf("Unable to connect TCP to server: %d\n", WSAGetLastError());
		_status = ConnectionStatus::ConnectionFailedStatus;
		//closesocket(_clientTCPsocket);
		//freeaddrinfo(_ptr);
		//WSACleanup();
		return;
	}

	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	buf[PCK_TYPE_POS] = PacketTypes::ConnectionAttempt;
		
	// Send udp socket info over to server.
	if (sendto(_clientUDPsocket, buf, BUF_LEN, 0, _ptr->ai_addr, _ptr->ai_addrlen) == SOCKET_ERROR)
	{
		printf("Unable to connect UDP to server: %d\n", WSAGetLastError());
		_status = ConnectionStatus::ConnectionFailedStatus;
		//closesocket(_clientUDPsocket);
		//freeaddrinfo(_ptr);
		//WSACleanup();
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

	if (!_connected && totalConnectAttemptTime > MAX_CONNECT_ATTEMPT_TIME)
		_status = ConnectionStatus::ConnectionFailedStatus;
	else if (_connected)
	{
		_status = ConnectionStatus::Connected;
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

void ClientSide::queryConnectAttempt(int& id, ConnectionStatus& status)
{
	if (_connected)
	{
		id = _networkID;
	}
	cout << "Status: " << static_cast<int>(_status) << endl;
	status = _status;

	if ((_status == ConnectionFailedStatus) || (_status == ServerFullStatus))
		_status = Disconected;
}

void ClientSide::queryEntityRequest(PacketTypes& query)
{
	if (query == PacketTypes::EmptyMsg)
	{
		// Ask server if it wants the starting entity list or just the client's required list.
		char buf[BUF_LEN];
		memset(buf, 0, BUF_LEN);

		buf[PCK_TYPE_POS] = PacketTypes::EntitiesQuery;
		buf[NET_ID_POS] = _networkID;

		if (send(_clientTCPsocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			cout << "Unable to query server for which entity list to provide!" << endl;
			query = ErrorMsg;
		}

		query = EntitiesQuery;
	}
	else
	{
		// Recieve server's reply.
		if (_entityQueryBuf != EmptyMsg)
			query = _entityQueryBuf;
		else
			query = EntitiesQuery;

		_entityQueryBuf = EmptyMsg;
	}
}

PacketTypes ClientSide::sendEntities(EntityData* entities, int& numEntities)
{
	// Send required entities to the server.
	EntityPacket packet = EntityPacket(PacketTypes::EntitiesUpdate, _networkID, numEntities);
	packet.serialize(entities);

	for (int i = 0; i < numEntities; ++i)
	{
		EntityData data = entities[i];
		cout << "Ent prefab: " << static_cast<int>(data._entityPrefabType) << endl;
		cout << "Ent Pos: " << data._position.toString();
		cout << "Ent Rot: " << data._rotation.toString();
	}

	if (send(_clientTCPsocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "Failed to send entities to the server!" << endl;
		return ErrorMsg;
	}
	else
	{
		cout << "Sent entities to the server." << endl;
		return EntitiesUpdate;
	}
}

void ClientSide::getServerEntities(EntityData* serverEntities, int& numServerEntities)
{
	//// Receive any pre-existing server entities.
	//EntityPacket serverEntityPacket = EntityPacket(_entityUpdatesBuf);
	//uint8_t _numEntitiesReceived = serverEntityPacket.getNumEntities();

	//if (numServerEntities <= 0)
	//{
	//	numServerEntities = _numEntitiesReceived;
	//	cout << "Received " << static_cast<int>(_numEntitiesReceived) << " server entities." << endl;
	//}
	//else
	//{
	//	EntityData* serverEntityData = new EntityData[numServerEntities];

	//	serverEntityPacket.deserialize(serverEntityData);

	//	EntityData* data;
	//	for (int i = 0; i < _numEntitiesReceived; ++i)
	//	{
	//		data = &serverEntityData[i];
	//		cout << "Server Entity: " << static_cast<int>(data->_entityID) << endl;
	//		cout << "Server Ent prefab: " << static_cast<int>(data->_entityPrefabType) << endl;
	//		cout << "Server Ent Pos: " << data->_position.toString();
	//		cout << "Server Ent Rot: " << data->_rotation.toString();
	//	}

	//	memcpy(serverEntities, &serverEntityPacket._data[DATA_START_POS + 1], sizeof(EntityData) * _numEntitiesReceived);


	//	// Reset the entity update buffer.
	//	memset(_entityUpdatesBuf, 0, BUF_LEN);
	//	_entityUpdatesBuf[PCK_TYPE_POS] = EmptyMsg;
	//}

	// Receive any pre-existing server entities.
	clock_t startClock = clock();
	int totalTime = 0;

	while ((numServerEntities <= 0) && (totalTime <= 6.0f))
	{
		if (numServerEntities <= 0)
		{
			numServerEntities = _entityUpdatesBuf.size();
			//cout << "Received " << numServerEntities << " server entities." << endl;

			if (numServerEntities > 0)
			{
				cout << "Received " << numServerEntities << " server entities." << endl;
				return;
			}
		}

		totalTime = (startClock - clock()) / CLOCKS_PER_SEC;
	}


	if (numServerEntities > 0)
	{
		memcpy(serverEntities, _entityUpdatesBuf.data(), sizeof(EntityData) * numServerEntities);


		// Reset the entity update buffer.
		//_entityUpdatesBuf.erase(_entityUpdatesBuf.begin() + numServerEntities);
	}
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
	case LobbyCharChoice:
		packet = new CharChoicePacket(_networkID);
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

	int bytesReceived = -1;
	int wsaError = -1;


	// Reveive updates from the server.
	bytesReceived = recvfrom(_clientUDPsocket, buf, BUF_LEN, 0, (sockaddr*)&fromAddr, &fromLen);

	//wsaError = WSAGetLastError();


	// Received data from server.
	if (bytesReceived > 0)
	{
		// Retrieve network ID of incomming message.
		uint8_t networkID = buf[NET_ID_POS];

		if (networkID == _networkID)
		{
			cout << "Same Network ID, UDP, Msg Type: " << int(uint8_t(buf[PCK_TYPE_POS])) << endl;
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
	else if (bytesReceived < 0)
	{
		wsaError = WSAGetLastError();
		//cout << "UDP Receive: Error " << wsaError << endl;

		if (wsaError == WSAECONNRESET)
		{
			cout << "Disconnected From Server." << endl;
			closesocket(_clientTCPsocket);
			closesocket(_clientUDPsocket);
			WSACleanup();
		}
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

	//wsaError = WSAGetLastError();


	// Received data from server.
	if (bytesReceived > 0)
	{
		// Retrieve network ID of incomming message.
		uint8_t networkID = buf[NET_ID_POS];

		if (networkID == _networkID)
		{
			cout << "Same Network ID, TCP, Msg Type: " << int(uint8_t(buf[PCK_TYPE_POS])) << endl;
			return;
		}


		PacketTypes pckType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);

		switch (pckType)
		{
		case Anim:
		{
			// Deserialize and store anim data.
			AnimPacket packet = AnimPacket(buf);
			AnimData animData;

			packet.deserialize(&animData);

			//cout << "Received entity " << static_cast<int>(animData._entityID) << " anim packet. EID: " << static_cast<int>(animData._entityID) << ", state: " << animData._state << endl;

			_animDataBuf.emplace_back(animData);
			return;
		}
		case EntitiesUpdate:
		{
			// Deserialize and store entity data.
			EntityPacket packet = EntityPacket(buf);
			uint8_t numEntities = packet.getNumEntities();
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
		case OwnershipChange:
		{
			OwnershipData ownershipData = OwnershipData();
			ownershipData._entityID = buf[DATA_START_POS];
			ownershipData._ownership = static_cast<Ownership>(buf[DATA_START_POS + 1]);

			cout << "Received ownership change: EID: " << static_cast<int>(ownershipData._entityID) << " , Ownership: " << static_cast<int>(ownershipData._ownership) << endl;

			_ownershipDataBuf.emplace_back(ownershipData);
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
	else if (bytesReceived < 0)
	{
		wsaError = WSAGetLastError();
		//cout << "TCP Receive: Error " << wsaError << endl;

		if (wsaError == WSAECONNRESET)
		{
			cout << "Disconnected From Server." << endl;
			closesocket(_clientTCPsocket);
			closesocket(_clientUDPsocket);
			WSACleanup();
		}
	}
}

void ClientSide::getPacketHandleSizes(int& transDataElements, int& animDataElements, int& entityDataElements, int& ownershipDataElements)
{



#pragma region OldMap
	//unordered_map<uint8_t, Packet*> currObjMap;


	//Packet* packet = nullptr;
	////PacketData* packetData;
	//TransformData transData;
	//AnimData animData;


	//uint8_t objID = -1;

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
	ownershipDataElements = _ownershipDataBuf.size();
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
	offset += sizeof(EntityData) * _entityDataBuf.size();

	memcpy(&byteDatahandle[offset], _ownershipDataBuf.data(), sizeof(OwnershipData) * _ownershipDataBuf.size());


	// Clean up.
	_transDataBuf.clear();
	_animDataBuf.clear();
	_entityDataBuf.clear();
	_ownershipDataBuf.clear();
}

void ClientSide::requestScores()
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
}

void ClientSide::getNumScores(int& numScores)
{
	numScores = _numScores;
}

ScoreData* ClientSide::getScoresHandle()
{
	return _scoresBuf;
}

void ClientSide::cleanupScoresHandle()
{
	ScoreData scoreData;
	for (int i = 0; i < _numScores; ++i)
	{
		scoreData = _scoresBuf[i];

		if (scoreData.teamName)
		{
			delete[] scoreData.teamName;
			scoreData.teamName = nullptr;
		}
	}

	delete _scoresBuf;
	_scoresBuf = nullptr;
}

void ClientSide::receiveLobbyData()
{
	// Reveive updates from the server.
	if (_inLobby)
	{
		char buf[BUF_LEN];

		memset(buf, 0, BUF_LEN);

		int bytesReceived = -1;
		int wsaError = -1;

		timeval timeout;
		timeout.tv_sec = 1;
		timeout.tv_usec = 0;
		fd_set fds;
		FD_ZERO(&fds);
		FD_SET(_clientTCPsocket, &fds);
		wsaError = select(NULL, &fds, NULL, NULL, &timeout);

		if (wsaError == SOCKET_ERROR)
		{
			//cout << "TCP Receive Lobby Data Update: Error " << WSAGetLastError() << endl;

			if (wsaError == WSAECONNRESET)
			{
				cout << "Disconnected From Server." << endl;
				closesocket(_clientTCPsocket);
				closesocket(_clientUDPsocket);
				WSACleanup();
			}
			return;
		}
		// Timeout occured.
		else if (wsaError == 0)
		{
			return;
		}


		bytesReceived = recv(_clientTCPsocket, buf, BUF_LEN, 0);
		// wsaError = WSAGetLastError();


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
				cout << "Server is requesting the " << static_cast<int>(buf[DATA_START_POS]) << " entity list." << endl;
				_entityQueryBuf = static_cast<PacketTypes>(buf[DATA_START_POS]);
				break;
			case EntitiesStart:
				break;
			case EntitiesNoStart:
				break;
			case EntitiesRequired:
				break;
			case EntityIDs:
				memset(_entityIDsBuf, 0, BUF_LEN);
				_entityIDsBuf[PCK_TYPE_POS] = EmptyMsg;

				memcpy(_entityIDsBuf, buf, BUF_LEN);
				break;
			case EntitiesUpdate:
			{
				// Retrieve network ID of incomming message.
				uint8_t networkID = buf[NET_ID_POS];

				if (networkID == _networkID)
				{
					cout << "Same Network ID, Lobby TCP, Msg Type: " << int(uint8_t(buf[PCK_TYPE_POS])) << endl;
					return;
				}

				//memset(_entityUpdatesBuf, 0, BUF_LEN);
				//_entityUpdatesBuf[PCK_TYPE_POS] = EmptyMsg;

				//memcpy(_entityUpdatesBuf, buf, BUF_LEN);


				EntityPacket serverEntityPacket = EntityPacket(buf);
				uint8_t _numEntitiesReceived = serverEntityPacket.getNumEntities();

				if (_numEntitiesReceived > 0)
				{
					EntityData* serverEntityData = new EntityData[_numEntitiesReceived];
					serverEntityPacket.deserialize(serverEntityData);

					EntityData* data;
					for (int i = 0; i < _numEntitiesReceived; ++i)
					{
						data = &serverEntityData[i];
						cout << "Server Entity: " << static_cast<int>(data->_entityID) << endl;
						cout << "Server Ent prefab: " << static_cast<int>(data->_entityPrefabType) << endl;
						cout << "Server Ent Pos: " << data->_position.toString();
						cout << "Server Ent Rot: " << data->_rotation.toString();
					}

					//memcpy(serverEntities, &serverEntityPacket._data[DATA_START_POS + 1], sizeof(EntityData) * _numEntitiesReceived);
					_entityUpdatesBuf.insert(_entityUpdatesBuf.end(), serverEntityData, serverEntityData + _numEntitiesReceived);

					delete[] serverEntityData;
					serverEntityData = nullptr;
				}
				
				break;
			}
			case Score:
			{
				ScorePacket packet = ScorePacket(buf);
				_numScores = packet.getNumScores();
				_scoresBuf = new ScoreData[_numScores];
				packet.deserialize(_scoresBuf);

				break;
			}
			case LobbyChat:
			{
				// Retrieve network ID of incomming message.
				uint8_t networkID = buf[NET_ID_POS];

				if (networkID == _networkID)
				{
					cout << "Same Network ID, Lobby TCP, Msg Type: " << int(uint8_t(buf[PCK_TYPE_POS])) << endl;
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
				uint8_t networkID = buf[NET_ID_POS];

				if (networkID == _networkID)
				{
					cout << "Same Network ID, Lobby TCP, Msg Type: " << int(uint8_t(buf[PCK_TYPE_POS])) << endl;
					return;
				}

				ChatPacket packet = ChatPacket(buf);
				_teamNameBuf = new ChatData();	// CLEAN UP

				packet.deserialize(_teamNameBuf);	// CLEAN UP

				cout << "Received team name msg from " << static_cast<int>(networkID) << ": " << _teamNameBuf->_msg << endl;
				break;
			}
			case LobbyCharChoice:
			{
				// Retrieve network ID of incomming message.
				uint8_t networkID = buf[NET_ID_POS];

				if (networkID == _networkID)
				{
					cout << "Same Network ID, Lobby TCP, Msg Type: " << int(uint8_t(buf[PCK_TYPE_POS])) << endl;
					return;
				}

				CharChoicePacket packet = CharChoicePacket(buf);
				_charChoiceBuf = new CharChoiceData();	// CLEAN UP

				packet.deserialize(_charChoiceBuf);

				cout << "Received character choice from " << static_cast<int>(networkID) << ": " << static_cast<int>(_charChoiceBuf->_charChoice) << endl;
				break;
			}
			case LobbyPlayer:
			{
				// Retrieve network ID of incomming message.
				uint8_t networkID = buf[NET_ID_POS];

				if (networkID == _networkID)
				{
					cout << "Same Network ID, Lobby TCP, Msg Type: " << int(uint8_t(buf[PCK_TYPE_POS])) << endl;
					return;
				}

				_lobbyPlayersBuf.emplace_back(networkID);

				cout << "Received lobby player from " << static_cast<int>(networkID) << endl;
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
		else if (bytesReceived < 0)
		{
			wsaError = WSAGetLastError();
			//cout << "TCP Lobby Receive: Error " << wsaError << endl;

			if (wsaError == WSAECONNRESET)
			{
				cout << "Disconnected From Server." << endl;
				closesocket(_clientTCPsocket);
				closesocket(_clientUDPsocket);
				WSACleanup();
			}
		}
	}
}

void ClientSide::stopLobbyReceive()
{
	_inLobby = false;
}

void ClientSide::getNumLobbyPackets(int& numMsgs, int& newTeamNameMsg, int& newCharChoice, int& numNewPlayers)
{
	numMsgs = _chatDataBuf.size();

	if (_teamNameBuf)
		newTeamNameMsg = 1;

	if (_charChoiceBuf)
		newCharChoice = 1;

	numNewPlayers = _lobbyPlayersBuf.size();
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

	if (_charChoiceBuf)
	{
		memcpy(&byteDatahandle[offset], _charChoiceBuf, sizeof(CharChoiceData));
		offset += sizeof(CharChoiceData);

		// Clean up
		delete _charChoiceBuf;
		_charChoiceBuf = nullptr;
	}

	memcpy(byteDatahandle, _lobbyPlayersBuf.data(), sizeof(uint8_t) * _lobbyPlayersBuf.size());
	offset += sizeof(uint8_t) * _lobbyPlayersBuf.size();


	// Clean up.
	_chatDataBuf.clear();
	_lobbyPlayersBuf.clear();
}

void ClientSide::clearLobbyBuffers()
{
	_entityUpdatesBuf.clear();
}

void ClientSide::setOwnership(uint8_t EID, Ownership ownership)
{
	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	buf[PCK_TYPE_POS] = OwnershipChange;
	buf[NET_ID_POS] = _networkID;
	buf[DATA_START_POS] = EID;
	buf[DATA_START_POS + 1] = ownership;

	if (send(_clientTCPsocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "Failed to send ownership change to the server!" << endl;
		return;
	}
	else
	{
		cout << "Sent ownership change to the server." << endl;
	}
}

void ClientSide::setFuncs(const CS_to_Plugin_Functions& funcs)
{
	_funcs = funcs;
}
