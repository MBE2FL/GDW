#include "Server.h"

uint8_t Server::_clientIDs = 0;


Server::Server()
{
	// Reserve space for clients.
	_clients.reserve(MAX_CLIENTS);
	_softConnectClients.reserve(MAX_CLIENTS);

	// Read in all saved scores.
	_scoreboard.Read();
}

Server::~Server()
{
	// Stop all threads and wait for them all stop.
	_stopThreads = true;
	_listenForConnections.join();
	_udpThread.join();
	_tcpSoftThread.join();
	_tcpThread.join();

	// Clean up all clients.
	for (Client* client : _softConnectClients)
	{
		delete client;
	}
	_softConnectClients.clear();

	for (Client* client : _clients)
	{
		delete client;
	}
	_clients.clear();


	// Clean up UDP connection info buffer.
	delete _udpListenInfoBuf;
	_udpListenInfoBuf = nullptr;

	// Clean up team name buffer.
	if (_teamNameBuf)
	{
		delete[] _teamNameBuf;
		_teamNameBuf = nullptr;
	}

	// Network cleanup
	closesocket(_serverTCP_socket);
	freeaddrinfo(_ptr);
	WSACleanup();
}

bool Server::initNetwork() {
	//Initialize winsock
	WSADATA wsa;
	int error;
	error = WSAStartup(MAKEWORD(2, 2), &wsa);

	if (error != 0) {
		printf("Failed to initialize %d\n", error);
		return 0;
	}

	//Create a server sockets
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


	printf("Server is ready!\n");

	return true;
}

bool Server::initUDP()
{
	//Create a server socket

	struct addrinfo hints;

	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_DGRAM;
	hints.ai_protocol = IPPROTO_UDP;
	hints.ai_flags = AI_PASSIVE;

	if (getaddrinfo(NULL, PORT, &hints, &_ptr) != 0) {
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		WSACleanup();
		return false;
	}


	_serverUDP_socket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);

	if (_serverUDP_socket == INVALID_SOCKET) {
		printf("Failed creating a socket %d\n", WSAGetLastError());
		WSACleanup();
		return false;
	}

	// Bind socket

	if (bind(_serverUDP_socket, _ptr->ai_addr, (int)_ptr->ai_addrlen) == SOCKET_ERROR) {
		printf("Bind failed: %d\n", WSAGetLastError());
		closesocket(_serverUDP_socket);
		freeaddrinfo(_ptr);
		WSACleanup();
		return false;
	}

	/// Change to non-blocking mode
	//u_long mode = 1;// 0 for blocking mode
	//ioctlsocket(_serverUDP_socket, FIONBIO, &mode);
	u_long mode = 0;// 0 for blocking mode
	ioctlsocket(_serverUDP_socket, FIONBIO, &mode);

	printf("UDP socket is ready!\n");

	return true;
}

bool Server::initTCP()
{
	//Create a server socket

	struct addrinfo hints;

	memset(&hints, 0, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;
	hints.ai_flags = AI_PASSIVE;

	if (getaddrinfo(NULL, PORT, &hints, &_ptr) != 0) {
		printf("Getaddrinfo failed!! %d\n", WSAGetLastError());
		WSACleanup();
		return false;
	}


	_serverTCP_socket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	if (_serverTCP_socket == INVALID_SOCKET) {
		printf("Failed creating a socket %d\n", WSAGetLastError());
		WSACleanup();
		return false;
	}

	// Bind socket

	if (bind(_serverTCP_socket, _ptr->ai_addr, (int)_ptr->ai_addrlen) == SOCKET_ERROR) {
		printf("Bind failed: %d\n", WSAGetLastError());
		closesocket(_serverTCP_socket);
		freeaddrinfo(_ptr);
		WSACleanup();
		return false;
	}

	printf("TCP socket is ready!\n");

	return true;
}

void Server::listenForConnections()
{
	// Only listen for connections while the server is not full.
	while ((_clients.size() != MAX_CLIENTS) && !_stopThreads)
	{
		cout << "Listening for clients..." << endl;

		// Listen on socket
		if (listen(_serverTCP_socket, SOMAXCONN) == SOCKET_ERROR)
		{
			printf("Listen failed: %d\n", WSAGetLastError());
			closesocket(_serverTCP_socket);
			freeaddrinfo(_ptr);
			WSACleanup();
			system("pause");
		}


		// Accept a connection (multiple clients --> threads)
		SOCKET clientSocket;
		sockaddr_in fromAddr;
		int fromLen = sizeof(fromAddr);

		clientSocket = accept(_serverTCP_socket, (sockaddr*)& fromAddr, &fromLen);

		if (clientSocket == INVALID_SOCKET)
		{
			printf("Accept() failed %d\n", WSAGetLastError());
			closesocket(clientSocket);
		}


		sockaddr_in fromUDPAddr;
		int fromUDPLen = sizeof(fromUDPAddr);

		// Wait for udp connection as well.
		clock_t startClock = clock();
		float totalTime = 0.0f;
		while (totalTime <= MAX_CONNECT_ATTEMPT_TIME)
		{
			totalTime = (clock() - startClock) / static_cast<float>(CLOCKS_PER_SEC);

			// Messaged was received on the UDP socket, but on a different thread.
			if (_udpListenInfoBuf)
			{
				fromUDPAddr = *_udpListenInfoBuf;
				break;
			}

		}

		char buf[BUF_LEN];
		memset(buf, 0, BUF_LEN);
		if (totalTime > MAX_CONNECT_ATTEMPT_TIME)
		{
			cout << "Client UDP connection could not be established." << endl;

			buf[PCK_TYPE_POS] = PacketTypes::ConnectionFailed;

			if (send(clientSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
			{
				cout << "Could not notify client their connect attempt failed! " << WSAGetLastError() << endl;
			}

			delete _udpListenInfoBuf;
			_udpListenInfoBuf = nullptr;

			closesocket(clientSocket);
			continue;
		}


		// Make sure only one thread writes to the vector at a time.
		mutex socketSaveMutex;
		lock_guard<mutex> guard(socketSaveMutex);
		//thread::id threadID = std::this_thread::get_id();


		// Send a connection accepted message back to the client.
		//char buf[BUF_LEN];
		memset(buf, 0, BUF_LEN);

		buf[PCK_TYPE_POS] = PacketTypes::ConnectionAccepted;
		buf[NET_ID_POS] = _clientIDs;

		if (send(clientSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			printf("Failed to send msg to client %d\n", WSAGetLastError());
			closesocket(clientSocket);
			freeaddrinfo(_ptr);
			WSACleanup();
			system("pause");
			return;
		}


		// Display client's ID, socket addresses and IP address.
		cout << "Client with ID, " << static_cast<int>(_clientIDs) << ", connected" << endl;

		char ipbuf[INET_ADDRSTRLEN];
		inet_ntop(AF_INET, &fromAddr, ipbuf, sizeof(ipbuf));
		cout << "TCP Socket Address: " << ipbuf << endl;

		memset(ipbuf, 0, INET_ADDRSTRLEN);
		inet_ntop(AF_INET, &fromUDPAddr, ipbuf, sizeof(ipbuf));
		cout << "UDP Socket Address: " << ipbuf << endl;

		char ip[INET_ADDRSTRLEN];
		inet_ntop(AF_INET, &fromUDPAddr.sin_addr, ip, sizeof(ip));
		cout << "IP: " << ip << endl;


		// Create an new client variable to store the newly connected client's information.
		Client* client = new Client();
		client->_ip = ipbuf;
		client->_connected = true;
		client->_id = _clientIDs;
		client->_udpSockAddr = fromUDPAddr;
		client->_udpSockAddrLen = fromUDPLen;
		client->_tcpSocket = clientSocket;

		_softConnectClients.insert(_softConnectClients.begin() + _clientIDs, client);

		++_clientIDs;

		// Server capacity has been recahed.
		//if (_clients.size() == MAX_CLIENTS)
		//{
		//	cout << "Both players have connected" << endl;

		//}

		delete _udpListenInfoBuf;
		_udpListenInfoBuf = nullptr;

		// Iff there is a buffered team name, then send it to the new client.
		if (_teamNameBuf)
		{
			ChatPacket packet = ChatPacket(PacketTypes::LobbyTeamName, SERVER_ID);
			ChatData chatData = ChatData();
			chatData._entityID = SERVER_ID;
			chatData._msg = _teamNameBuf;
			chatData._msgSize = strlen(_teamNameBuf);
			packet.serialize(&chatData);

			if (send(clientSocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
			{
				printf("Failed to send team name to client %d\n", WSAGetLastError());
				closesocket(clientSocket);
				freeaddrinfo(_ptr);
				WSACleanup();
				system("pause");
				return;
			}
		}

		// Iff there is a buffered character choice, then send it to the new client.
		CharacterChoices charChoice;
		Client* otherClient;
		for (int i = 0; i < _softConnectClients.size(); ++i)
		{
			otherClient = _softConnectClients[i];
			charChoice = otherClient->_charChoice;

			// Don't send back to the same client who sent the data, or if their choice was nothing.
			if ((client->_id == otherClient->_id) || (charChoice == CharacterChoices::NoChoice))
			{
				continue;
			}

			CharChoiceData charChoiceData = CharChoiceData();
			charChoiceData._charChoice = charChoice;
			CharChoicePacket packet = CharChoicePacket(otherClient->_id);
			packet.serialize(&charChoiceData);
			cout << "Sent buffered character choice to " << static_cast<int>(otherClient->_id) << ": " << static_cast<int>(charChoiceData._charChoice) << endl;

			if (send(clientSocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
			{
				printf("Failed to send character choice to client %d\n", WSAGetLastError());
				closesocket(clientSocket);
				freeaddrinfo(_ptr);
				WSACleanup();
				system("pause");
				return;
			}
		}

		// Send all other connected players to the client.
		for (int i = 0; i < _softConnectClients.size(); ++i)
		{
			otherClient = _softConnectClients[i];
			charChoice = otherClient->_charChoice;

			// Don't send new client to itself.
			if (client->_id == otherClient->_id)
			{
				continue;
			}

			cout << "Sent buffered client to " << static_cast<int>(otherClient->_id) << endl;

			memset(buf, 0, BUF_LEN);
			buf[PCK_TYPE_POS] = LobbyPlayer;
			buf[NET_ID_POS] = otherClient->_id;

			if (send(clientSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
			{
				printf("Failed to send lobby player to client %d\n", WSAGetLastError());
				closesocket(clientSocket);
				freeaddrinfo(_ptr);
				WSACleanup();
				system("pause");
				return;
			}
		}

		// Send new client to all other connected players.
		for (int i = 0; i < _softConnectClients.size(); ++i)
		{
			otherClient = _softConnectClients[i];
			charChoice = otherClient->_charChoice;

			// Don't send new client to itself.
			if (client->_id == otherClient->_id)
			{
				continue;
			}

			cout << "Sent new client to " << static_cast<int>(otherClient->_id) << endl;

			memset(buf, 0, BUF_LEN);
			buf[PCK_TYPE_POS] = LobbyPlayer;
			buf[NET_ID_POS] = client->_id;

			if (send(otherClient->_tcpSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
			{
				printf("Failed to send lobby player to client %d\n", WSAGetLastError());
				closesocket(otherClient->_tcpSocket);
				freeaddrinfo(_ptr);
				WSACleanup();
				system("pause");
				return;
			}
		}
	}
}

void Server::processClientEntityRequest(SOCKET* clientSocket)
{
	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	buf[PCK_TYPE_POS] = EntitiesQuery;


	if (!_gameStarted)
	{
		buf[DATA_START_POS] = PacketTypes::EntitiesStart;
		_gameStarted = true;
	}
	else
		buf[DATA_START_POS] = PacketTypes::EntitiesRequired;

	cout << "Server is requesting the " << static_cast<int>(buf[DATA_START_POS]) << " entity list from a client." << endl;

	if (send(*clientSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "Failed to reply to client entity request! " << WSAGetLastError() << endl;
		return;
	}
}

void Server::processStarterEntities(SOCKET* clientSocket, char buf[BUF_LEN])
{
	// Requested Entities received.
	EntityPacket packet = EntityPacket(buf);
	uint8_t numEntities = packet.getNumEntities();
	EntityData* entityData = new EntityData[numEntities];

	packet.deserialize(entityData);
	cout << "Num entities: " << int(numEntities) << endl;

	if (numEntities > 0)
	{
		uint8_t* entityIDs = new uint8_t[numEntities];
		uint8_t currEID = _entities.size();

		// Generate IDs for the number entities requested. 
		for (int i = 0; i < numEntities; ++i)
		{
			// Update the deserialized entity data.
			entityData[i]._entityID = currEID;

			cout << entityData[i]._position.toString();

			// Store each entity ID.
			entityIDs[i] = currEID;

			cout << "Added starter entity with ID, " << static_cast<int>(currEID) << endl;

			++currEID;
		}

		_entities.insert(_entities.end(), entityData, entityData + numEntities);
		//std::copy(entityData, entityData + int(numEntities), std::back_inserter(_entities));
		//_entities.resize(numEntities);
		//memcpy(_entities.data(), entityData, sizeof(EntityData*) * int(numEntities));
		
		delete[] entityData;
		entityData = nullptr;


		// Send generated IDs back to the client.
		memset(buf, 0, BUF_LEN);

		buf[PCK_TYPE_POS] = PacketTypes::EntityIDs;
		buf[DATA_START_POS] = numEntities;
		memcpy(&buf[DATA_START_POS + 1], entityIDs, numEntities);


		if (send(*clientSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			cout << "Failed to send generated entity IDs back to client." << endl;
		}
	}


	// Fully connect the client.
	int index = 0;
	Client* client;
	for (; index < _softConnectClients.size(); ++index)
	{
		client = _softConnectClients[index];

		if (*clientSocket == client->_tcpSocket)
			break;
	}

	_clients.emplace_back(client);

	_softConnectClients.erase(_softConnectClients.begin() + index);
}

void Server::processRequiredEntities(SOCKET* clientSocket, char buf[BUF_LEN])
{
	// Required Entities received.
	EntityPacket packet = EntityPacket(buf);
	uint8_t numEntities = packet.getNumEntities();
	EntityData* entityData = new EntityData[numEntities];

	packet.deserialize(entityData);
	cout << "Num entities " << int(numEntities) << endl;

	if (numEntities > 0)
	{
		uint8_t* entityIDs = new uint8_t[numEntities];
		uint8_t currEID = _entities.size();

		// Generate IDs for the number entities requested. 
		for (int i = 0; i < numEntities; ++i)
		{
			// Update the deserialized entity data.
			entityData[i]._entityID = currEID;

			// Store each entity ID.
			entityIDs[i] = currEID;

			cout << "Added required entity with ID, " << int(currEID) << endl;

			++currEID;
		}

		// Store new entities on the server.
		_entities.insert(_entities.end(), entityData, entityData + numEntities);

		// Serialize the updated entity data back into the packet.
		packet.serialize(entityData);

		delete[] entityData;
		entityData = nullptr;

		// Send new entities to all other connected clients.
		packet.setPacketType(PacketTypes::EntitiesUpdate);

		for (Client* client : _clients)
		{
			//if (socket == clientSocket)
			if (client->_tcpSocket == *clientSocket)
				continue;

			if (send(client->_tcpSocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
			{
				cout << "Failed to send generated required entity IDs to other clients." << endl;
			}
			else
				cout << "Sent generated required entity IDs to other client." << endl;
		}


		// Send newly generated and all pre-existing IDs to the client.
		memset(buf, 0, BUF_LEN);

		buf[PCK_TYPE_POS] = PacketTypes::EntityIDs;
		buf[DATA_START_POS] = numEntities;
		memcpy(&buf[DATA_START_POS + 1], entityIDs, numEntities);


		if (send(*clientSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			cout << "Failed to send generated entity IDs back to client." << endl;
		}
		else
			cout << "Sent generated required entity IDs back to client." << endl;
	}


	// Fully connect the client.
	int index = 0;
	Client* client;
	for (; index < _softConnectClients.size(); ++index)
	{
		client = _softConnectClients[index];

		if (*clientSocket == client->_tcpSocket)
			break;
	}

	_clients.emplace_back(client);

	_softConnectClients.erase(_softConnectClients.begin() + index);
}

void Server::processEntities(SOCKET* clientSocket, char buf[BUF_LEN])
{
	// Entities received.
	EntityPacket packet = EntityPacket(buf);
	uint8_t numEntities = packet.getNumEntities();
	EntityData* entityData = new EntityData[numEntities];

	packet.deserialize(entityData);
	cout << "Num entities " << int(numEntities) << endl;

	if (numEntities > 0)
	{
		uint8_t currEID = _entities.size();

		// Generate IDs for the number entities requested. 
		for (int i = 0; i < numEntities; ++i)
		{
			// Update the deserialized entity data.
			entityData[i]._entityID = currEID;

			cout << "Added required entity with ID, " << int(currEID) << endl;

			++currEID;
		}

		// Store new entities on the server.
		_entities.insert(_entities.end(), entityData, entityData + numEntities);

		// Serialize the updated entity data back into the packet.
		packet.serialize(entityData);

		delete[] entityData;
		entityData = nullptr;

		// Send new entities to all connected clients.
		packet.setPacketType(PacketTypes::EntitiesUpdate);
		packet.setNetworkID(SERVER_ID);

		for (Client* client : _softConnectClients)
		{
			if (send(client->_tcpSocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
			{
				cout << "Failed to send new entities to client, " << static_cast<int>(client->_id) << "." << endl;
			}
			else
				cout << "Sent generated new entities to client, " << static_cast<int>(client->_id) << "." << endl;
		}

		for (Client* client : _clients)
		{
			if (send(client->_tcpSocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
			{
				cout << "Failed to send new entities to client, " << static_cast<int>(client->_id) << "." << endl;
			}
			else
				cout << "Sent generated new entities to client, " << static_cast<int>(client->_id) << "." << endl;
		}
	}


	// Fully connect the client.
	int index = 0;
	Client* client;
	for (; index < _softConnectClients.size(); ++index)
	{
		client = _softConnectClients[index];

		if (*clientSocket == client->_tcpSocket)
			break;
	}

	_clients.emplace_back(client);

	_softConnectClients.erase(_softConnectClients.begin() + index);
}

void Server::sendEntitiesToClient(SOCKET* clientSocket)
{
	// Send all the current server entities to the newly connected client.
	EntityPacket packet = EntityPacket(PacketTypes::EntitiesUpdate, 0, _entities.size());
	packet.serialize(_entities.data());


	if (send(*clientSocket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
	{
		printf("Failed to request required entities from client. %d\n", WSAGetLastError());
		return;
	}
	else
	{
		cout << "Sent current server entities to the newly connected client." << endl;
	}
}

void Server::processTransform(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen)
{

	// Reveive transform updates from players.
	// Retrieve network id of incomming message.
	uint8_t networkID = buf[NET_ID_POS];

	cout << "Trans Packet: " << "ID: " << static_cast<int>(networkID) << endl;

	// Send data too all other clients.
	Client* client;
	vector<Client*>::const_iterator it;
	for (it = _clients.cbegin(); it != _clients.cend(); ++it)
	{
		client = *it;

		// Don't send back to the same client who sent the data.
		if (client->_id == networkID)
		{
			continue;
		}

		// Send data to other clients.
		if (sendto(_serverUDP_socket, buf, BUF_LEN, 0, (sockaddr*)& client->_udpSockAddr, client->_udpSockAddrLen) == SOCKET_ERROR)
		{
			printf("Failed to send transform data. %d\n", WSAGetLastError());
		}
	}
}

void Server::processAnim(char buf[BUF_LEN], SOCKET* socket)
{

	// Reveive anim updates from players.
	// Retrieve network id of incomming message.
	uint8_t networkID = buf[NET_ID_POS];


	// Extract data (FOR DEBUG ONLY).
	AnimData animData;
	AnimPacket packet = AnimPacket(buf);
	packet.deserialize(&animData);
	//cout << "Anim Packet: " << "ID: " << static_cast<int>(animData._entityID) << ", state: " << animData._state << endl;


	// Send data to all other clients.
	Client* client;
	vector<Client*>::const_iterator it;
	for (it = _clients.cbegin(); it != _clients.cend(); ++it)
	{
		client = *it;

		// Don't send back to the same client who sent the data.
		if (client->_tcpSocket == *socket)
		{
			continue;
		}

		// Send data to other clients.
		if (send(client->_tcpSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			printf("Failed to send anim data. %d\n", WSAGetLastError());
		}
	}
}

void Server::processScore(char buf[BUF_LEN])
{
	ScorePacket packet = ScorePacket(buf);
	uint8_t numScores = packet.getNumScores();
	ScoreData scoreData;

	packet.deserialize(&scoreData);

	_scoreboard.getTimes().emplace_back(scoreData._time);
	_scoreboard.Sort();
	_scoreboard.Write();
}

void Server::processClientScoresRequest(char buf[BUF_LEN], SOCKET* socket)
{
	vector<PlayerTime> times = _scoreboard.getTimes();
	uint8_t netID = buf[NET_ID_POS];
	ScorePacket packet = ScorePacket(netID, times.size());

	vector<ScoreData> dataBuf = vector<ScoreData>(times.size());
	for (PlayerTime time : times)
	{
		dataBuf.emplace_back(time);
	}

	packet.serialize(dataBuf.data());

	// Send data to client.
	if (send(*socket, packet._data, BUF_LEN, 0) == SOCKET_ERROR)
	{
		printf("Failed to send score data. %d\n", WSAGetLastError());
	}
}

void Server::processChat(char buf[BUF_LEN])
{
	// Retrieve network id of incomming message.
	uint8_t networkID = buf[NET_ID_POS];


	// Extract data (FOR DEBUG ONLY).
	//ChatData chatData;
	//ChatPacket packet = ChatPacket(buf);
	//packet.deserialize(&chatData);
	//cout << "Received chat msg from " << static_cast<int>(networkID) << ": " << chatData._msg << endl;
	//delete[] chatData._msg;


	// Send data to all other clients.
	Client* client;
	vector<Client*>::const_iterator it;
	for (it = _softConnectClients.cbegin(); it != _softConnectClients.cend(); ++it)
	{
		client = *it;

		// Don't send back to the same client who sent the data.
		if (client->_id == networkID)
		{
			continue;
		}

		// Send data to other clients.
		if (send(client->_tcpSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			printf("Failed to send chat data. %d\n", WSAGetLastError());
		}
	}
}

void Server::processTeamName(char buf[BUF_LEN])
{
	// Retrieve network id of incomming message.
	uint8_t networkID = buf[NET_ID_POS];


	// Extract data.
	ChatData chatData;
	ChatPacket packet = ChatPacket(buf);
	packet.deserialize(&chatData);
	cout << "Received team name msg from " << static_cast<int>(networkID) << ": " << chatData._msg << endl;

	// Save team name and send to new clients, upon connection.
	if (_teamNameBuf)
	{
		delete[] _teamNameBuf;
		_teamNameBuf = nullptr;
	}
	_teamNameBuf = chatData._msg;


	// Send data to all other clients.
	Client* client;
	vector<Client*>::const_iterator it;
	for (it = _softConnectClients.cbegin(); it != _softConnectClients.cend(); ++it)
	{
		client = *it;

		// Don't send back to the same client who sent the data.
		if (client->_id == networkID)
		{
			continue;
		}

		// Send data to other clients.
		if (send(client->_tcpSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			printf("Failed to send team name data. %d\n", WSAGetLastError());
		}
	}
}

void Server::processCharChoice(char buf[BUF_LEN])
{
	// Retrieve network id of incomming message.
	uint8_t networkID = buf[NET_ID_POS];


	// Extract data (FOR DEBUG ONLY).
	CharChoicePacket packet = CharChoicePacket(buf);
	CharChoiceData charChoiceData = CharChoiceData();
	packet.deserialize(&charChoiceData);
	cout << "Received character choice from " << static_cast<int>(networkID) << ": " << static_cast<int>(charChoiceData._charChoice) << endl;


	// Send data to all other clients.
	Client* client;
	vector<Client*>::const_iterator it;
	for (it = _softConnectClients.cbegin(); it != _softConnectClients.cend(); ++it)
	{
		client = *it;

		// Don't send back to the same client who sent the data.
		if (client->_id == networkID)
		{
			// Save the client's character choice.
			client->_charChoice = charChoiceData._charChoice;
			continue;
		}

		// Send data to other clients.
		if (send(client->_tcpSocket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			printf("Failed to send chat data. %d\n", WSAGetLastError());
		}
	}
}

void Server::update()
{
	////////////////////
	/*
	CODE TO RECEIVE UPDATES FROM CLIENT GOES HERE...
	*/
	//thread udpThread(&Server::udpUpdate, this);
	//thread tcpThread(&tcpUpdate);

	//udpThread.join();
	//tcpThread.join();

	//cout << "Update thread done" << endl;
}

void Server::initThreads()
{
	_listenForConnections = thread(&Server::listenForConnections, this);
	_udpThread = thread(&Server::udpUpdate, this);
	_tcpSoftThread = thread(&Server::tcpSoftUpdate, this);
	_tcpThread = thread(&Server::tcpUpdate, this);
}

void Server::udpUpdate()
{
	cout << "UDP Update Listening" << endl;

	while (!_stopThreads)
	{
		char buf[BUF_LEN];
		//char* buf = new char[BUF_LEN];
		struct sockaddr_in fromAddr;
		int fromLen;
		fromLen = sizeof(fromAddr);

		memset(buf, '\0', BUF_LEN);

		int bytesReceived = -1;
		int wsaError = -1;

		// Receive message from a client.
		bytesReceived = recvfrom(_serverUDP_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, &fromLen);

		wsaError = WSAGetLastError();

		// Packet recieved
		if (bytesReceived > 0)
		{
			PacketTypes msgType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);


			switch (msgType)
			{
			case TransformMsg:
				processTransform(buf, fromAddr, fromLen);
				break;
			case ConnectionAttempt:
			{
				_udpListenInfoBuf = new sockaddr_in(fromAddr);
			}
			break;
			default:
				cout << "Unexpected message type received!" << endl;
				break;
			}
		}
	}
}

void Server::tcpSoftUpdate()
{
	cout << "TCP Soft Update Listening" << endl;
	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	int bytesReceived = -1;
	int wsaError = -1;

	fd_set fds;
	SOCKET* clientSocket;

	// Specify time for a timeout to occur.
	timeval timeout;
	timeout.tv_sec = 1;
	timeout.tv_usec = 0;

	while ((_clients.size() != MAX_CLIENTS) && !_stopThreads)
	{
		// Skip handling new connections if no new clients are connecting.
		if (_softConnectClients.empty())
			continue;

		memset(buf, 0, BUF_LEN);

		bytesReceived = -1;
		wsaError = -1;

		// Receive message from a new client.
		FD_ZERO(&fds);
		for (int i = 0; i < _softConnectClients.size(); ++i)
		{
			FD_SET(_softConnectClients[i]->_tcpSocket, &fds);
		}

		if (fds.fd_count <= 0)
			continue;


		wsaError = select(NULL, &fds, NULL, NULL, &timeout);


		if (wsaError == SOCKET_ERROR)
		{
			cout << "TCP Soft Update: Error " << WSAGetLastError() << endl;
			continue;
		}
		// Timeout occured.
		else if (wsaError == 0)
		{
			continue;
		}


		clientSocket = nullptr;
		for (int i = 0; i < fds.fd_count; ++i)
		{
			clientSocket = &fds.fd_array[i];

			bytesReceived = recv(*clientSocket, buf, BUF_LEN, 0);


			if (bytesReceived > 0)
			{
				PacketTypes msgType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);

				switch (msgType)
				{
				case EntitiesQuery:
					processClientEntityRequest(clientSocket);
					break;
				case EntitiesStart:
					processStarterEntities(clientSocket, buf);
					break;
				case EntitiesNoStart:
					break;
				case EntitiesRequired:
					// Send all server entities to the client.
					sendEntitiesToClient(clientSocket);
					// Request any required entities the client has, and notify all other clients.
					processRequiredEntities(clientSocket, buf);
					break;
				case EntitiesUpdate:
					processEntities(clientSocket, buf);
					break;
				case EntityIDs:
					break;
				case ClientScoresRequest:
					processClientScoresRequest(buf, clientSocket);
					break;
				case LobbyChat:
					processChat(buf);
					break;
				case LobbyTeamName:
					processTeamName(buf);
					break;
				case LobbyCharChoice:
					processCharChoice(buf);
					break;
				default:
					break;
				}
			}
			else if (bytesReceived == 0)
			{
				cout << "TCP Soft Update: Empty message received." << endl;
			}
			else
			{
				cout << "TCP Soft Update: Error " << WSAGetLastError() << endl;
			}
		}
	}
}

void Server::tcpUpdate()
{
	cout << "TCP Update Listening" << endl;
	char buf[BUF_LEN];
	memset(buf, 0, BUF_LEN);

	int bytesReceived = -1;
	int wsaError = -1;

	fd_set fds;
	SOCKET* clientSocket;

	// Specify time for a timeout to occur.
	timeval timeout;
	timeout.tv_sec = 1;
	timeout.tv_usec = 0;

	while (!_stopThreads)
	{
		// Skip handling connected clients, if none are connected.
		if (_clients.empty())
			continue;

		memset(buf, 0, BUF_LEN);

		bytesReceived = -1;
		wsaError = -1;

		// Receive message from a client.
		FD_ZERO(&fds);
		for (int i = 0; i < _clients.size(); ++i)
		{
			FD_SET(_clients[i]->_tcpSocket, &fds);
		}
		//for (SOCKET* tcpSocket : _clientTCPSockets)
		//{
		//	FD_SET(*tcpSocket, &fds);
		//}

		if (fds.fd_count <= 0)
			continue;


		wsaError = select(NULL, &fds, NULL, NULL, NULL);


		if (wsaError == SOCKET_ERROR)
		{
			cout << "TCP Update: Error " << WSAGetLastError() << endl;
			continue;
		}
		// Timeout occured.
		else if (wsaError == 0)
		{
			continue;
		}


		clientSocket = nullptr;
		for (int i = 0; i < fds.fd_count; ++i)
		{
			clientSocket = &fds.fd_array[i];

			bytesReceived = recv(*clientSocket, buf, BUF_LEN, 0);

			
			if (bytesReceived > 0)
			{
				PacketTypes msgType = static_cast<PacketTypes>(buf[PCK_TYPE_POS]);

				switch (msgType)
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
					processAnim(buf, clientSocket);
					break;
				case EntitiesUpdate:
					break;
				case EmptyMsg:
					break;
				case Score:
					processScore(buf);
					break;
				default:
					break;
				}
			}
			else if (bytesReceived == 0)
			{
				cout << "TCP Update: Empty message received." << endl;
			}
			else
			{
				cout << "TCP Update: Error " << WSAGetLastError() << endl;
			}
		}
	}
}
