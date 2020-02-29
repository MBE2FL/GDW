#include "Server.h"


Server::Server()
{
	_clients.reserve(MAX_CLIENTS);
	_clientTCPSockets.reserve(MAX_CLIENTS);
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

void Server::connectPlayer(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen)
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
	if (sendto(_serverUDP_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, fromLen) == SOCKET_ERROR)
	{
		printf("Failed to send connection packet. %d\n", WSAGetLastError());
	}
}

void Server::listenForConnections()
{
	// Only listen for connections while the server is not full.
	while (_clients.size() != MAX_CLIENTS)
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
		SOCKET client_socket;
		sockaddr_in fromAddr;
		int fromLen = sizeof(fromAddr);

		client_socket = accept(_serverTCP_socket, (sockaddr*)&fromAddr, &fromLen);

		if (client_socket == INVALID_SOCKET)
		{
			printf("Accept() failed %d\n", WSAGetLastError());
			closesocket(client_socket);
			//freeaddrinfo(_ptr);
			//WSACleanup();
			system("pause");
		}

		//u_long mode = 1;// 0 for blocking mode
		//ioctlsocket(client_socket, FIONBIO, &mode);


		sockaddr_in fromUDPAddr;
		int fromUDPLen = sizeof(fromUDPAddr);


		// Wait for udp connection as well.
		unsigned int timeouts = 0;
		while (timeouts < MAX_TIMEOUTS)
		{
			timeval timeout;
			timeout.tv_sec = 5;
			timeout.tv_usec = 0;

			fd_set fds;
			FD_ZERO(&fds);
			FD_SET(_serverUDP_socket, &fds);

			int wsaError = -1;

			wsaError = select(NULL, &fds, NULL, NULL, &timeout);

			if (_udpListenInfo)
			{
				fromUDPAddr = *_udpListenInfo;

				char ipbuf[INET_ADDRSTRLEN];
				inet_ntop(AF_INET, _udpListenInfo, ipbuf, sizeof(ipbuf));
				cout << "UDP Socket Address: " << ipbuf << endl;

				char ip[BUF_LEN];
				inet_ntop(AF_INET, &_udpListenInfo->sin_addr, ip, sizeof(ip));
				cout << "IP: " << ip << endl;
				break;
			}

			switch (wsaError)
			{
			case SOCKET_ERROR:
				printf("Select() failed %d\n", WSAGetLastError());
				closesocket(client_socket);
				//freeaddrinfo(_ptr);
				//WSACleanup();
				break;
			case 0:
				cout << "UDP connect attempt timeout!" << endl;
				++timeouts;
				continue;
			default:
			{
				// UDP connection received. Store sockaddr info, and notify client on TCP socket.
				//sockaddr_in fromUDPAddr;
				//int fromUDPLen = sizeof(fromUDPAddr);
				char buf[BUF_LEN];
				memset(buf, 0, BUF_LEN);
				int bytesReceived = -1;

				bytesReceived = recvfrom(_serverUDP_socket, buf, BUF_LEN, 0, (sockaddr*)&fromUDPAddr, &fromUDPLen);

				if (bytesReceived >= 0)
				{
					char ipbuf[INET_ADDRSTRLEN];
					inet_ntop(AF_INET, &fromUDPAddr, ipbuf, sizeof(ipbuf));
					cout << "UDP Socket Address: " << ipbuf << endl;

					char ip[BUF_LEN];
					inet_ntop(AF_INET, &fromUDPAddr.sin_addr, ip, sizeof(ip));
					cout << "IP: " << ip << endl;
				}
			}
				break;
			}
			

			break;
		}


		// Client UDP connection could not be established.
		if (timeouts >= MAX_TIMEOUTS)
		{
			closesocket(client_socket);
			cout << "Client UDP connection could not be established." << endl;
			continue;
		}


		// Make sure only one thread writes to the vector at a time.
		mutex socketSaveMutex;
		lock_guard<mutex> guard(socketSaveMutex);
		//thread::id threadID = std::this_thread::get_id();

		// Find first available index.
		INT8 index = 0;
		vector<SOCKET*>::const_iterator it;
		for (it = _clientTCPSockets.cbegin(); it != _clientTCPSockets.cend(); ++it)
		{
			if (*it == nullptr)
			{
				break;
			}

			++index;
		}

		char buf[BUF_LEN];
		memset(buf, 0, BUF_LEN);
		//buf[0] = reinterpret_cast<char&>(msgType);
		buf[0] = MessageTypes::ConnectionAccepted;
		//buf[1] = reinterpret_cast<char&>(client->_id);
		buf[1] = index;

		if (send(client_socket, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			printf("Failed to send msg to client %d\n", WSAGetLastError());
			closesocket(client_socket);
			freeaddrinfo(_ptr);
			WSACleanup();
			system("pause");
			return;
		}


		cout << "Player connected" << endl;

		char ipbuf[INET_ADDRSTRLEN];
		inet_ntop(AF_INET, &fromAddr, ipbuf, sizeof(ipbuf));
		cout << "Socket Address: " << ipbuf << endl;

		char ip[BUF_LEN];
		inet_ntop(AF_INET, &fromAddr.sin_addr, ip, sizeof(ip));
		cout << "IP: " << ip << endl;

		Client* client = new Client();
		client->_ip = ipbuf;
		client->connected = true;
		client->_id = index;
		client->_sockAddr = (sockaddr*)&fromAddr;
		client->_sockAddrLen = fromUDPLen;
		client->fromAddr = fromUDPAddr;
		_clients.insert(_clients.begin() + index, client);


		if (_clients.size() == MAX_CLIENTS)
		{
			cout << "Both players have connected" << endl;

		}


		// Connection successfuly established.
		_clientTCPSockets.insert(_clientTCPSockets.begin() + index, &client_socket);

	}
}

#pragma region OLD_TRANSFORM
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

void Server::processTransform(char buf[BUF_LEN], const sockaddr_in& fromAddr, const int& fromLen)
{

	// Reveive transform updates from players.


	// Retrieve network id of incomming message.
	INT8 networkID = buf[1];



	// Extract data (FOR DEBUG ONLY).
	//size_t posSize = sizeof(float) * 3;
	//size_t rotSize = sizeof(float) * 4;
	//Vector3 posDebug;
	//Quaternion rotDebug;
	//memcpy(&posDebug._x, reinterpret_cast<float*>(&buf[2]), posSize);
	//memcpy(&rotDebug._x, reinterpret_cast<float*>(&buf[2 + posSize]), rotSize);
	//cout << "Msg Type: " << int(buf[0]) << ", ID: " << int(buf[1]) << endl;
	//cout << posDebug.toString() << rotDebug.toString();


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
		if (sendto(_serverUDP_socket, buf, BUF_LEN, 0, (sockaddr*)&client->fromAddr, client->_sockAddrLen) == SOCKET_ERROR)
		{
			printf("Failed to send transform data. %d\n", WSAGetLastError());
		}
	}
}

void Server::processAnim(char buf[BUF_LEN], SOCKET* socket)
{

	// Reveive anim updates from players.

	if (recv(*socket, buf, BUF_LEN, 0) == SOCKET_ERROR)
	{
		cout << "Could not read packet from TCP socket!" << endl;
	}

	// Retrieve network id of incomming message.
	INT8 networkID = buf[1];


	// Extract data (FOR DEBUG ONLY).
	size_t animStateSize = sizeof(int);
	int state = -1;
	memcpy(&state, reinterpret_cast<int*>(&buf[DATA_START_POS]), animStateSize);
	cout << "Msg Type: " << int(buf[0]) << ", ID: " << int(buf[1]) << endl;
	cout << state << endl;


	// Send data too all other clients.
	SOCKET* client;
	vector<SOCKET*>::const_iterator it;
	for (it = _clientTCPSockets.cbegin(); it != _clientTCPSockets.cend(); ++it)
	{
		client = *it;

		// Don't send back to the same client who sent the data.
		if (client == socket)
		{
			continue;
		}

		// Send data to other clients.
		if (send(*client, buf, BUF_LEN, 0) == SOCKET_ERROR)
		{
			printf("Failed to send anim data. %d\n", WSAGetLastError());
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

void Server::initUpdateThreads()
{
	_udpThread = thread(&Server::udpUpdate, this);
	_tcpThread = thread(&Server::tcpUpdate, this);
}

void Server::udpUpdate()
{
	cout << "UDP Listening" << endl;

	while (true)
	{
		char buf[BUF_LEN];
		//char* buf = new char[BUF_LEN];
		struct sockaddr_in fromAddr;
		int fromLen;
		fromLen = sizeof(fromAddr);

		memset(buf, '\0', BUF_LEN);

		int bytes_received = -1;
		int sError = -1;

		// Receive message from a client.
		bytes_received = recvfrom(_serverUDP_socket, buf, BUF_LEN, 0, (struct sockaddr*) & fromAddr, &fromLen);

		sError = WSAGetLastError();

		// Packet recieved
		// sError != WSAEWOULDBLOCK && 
		if (bytes_received > 0)
		{
			MessageTypes msgType = static_cast<MessageTypes>(buf[0]);

			switch (msgType)
			{
			case TransformMsg:
				processTransform(buf, fromAddr, fromLen);
				break;
			case ConnectionAttempt:
			{
				_udpListenInfo = new sockaddr_in(fromAddr);
			}
				break;
			default:
				cout << "Unexpected message type received!" << endl;
				break;
			}
		}
	}
}

void Server::tcpUpdate()
{
	cout << "TCP Listening" << endl;

	while (true)
	{
		if (_clientTCPSockets.empty())
			continue;

		char buf[BUF_LEN];
		memset(buf, '\0', BUF_LEN);

		int bytes_received = -1;
		int wsaError = -1;

		// Receive message from a client.
		fd_set fds;
		FD_ZERO(&fds);
		for (SOCKET* tcpSocket : _clientTCPSockets)
		{
			FD_SET(*tcpSocket, &fds);
		}

		if (fds.fd_count <= 0)
			continue;
		

		wsaError = select(NULL, &fds, NULL, NULL, NULL);

		if (wsaError == SOCKET_ERROR)
		{
			cout << "TCP Read Wait Error!" << endl;
		}
		else if (wsaError >= 1)
		{
			for (int i = 0; i < fds.fd_count; ++i)
			{
				bytes_received = recv(fds.fd_array[i], buf, BUF_LEN, 0);

				cout << bytes_received << endl;
			}
		}




		//sError = WSAGetLastError();

		//cout << sError << endl;
	}
}
