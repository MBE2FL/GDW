#include "Server.h"
//#include "CustomConsole.h"


int main()
{
	CustomConsole* cc = CustomConsole::getInstance();

	Server* server = new Server();

	//Initialize Network
	if (!server->initNetwork())
		return 1;

	cout << "Max number of threads: " << thread::hardware_concurrency() << endl;

	thread listenForConnections = thread(&Server::listenForConnections, server);
	server->initUpdateThreads();

	while (true)
	{
		server->update();
		

		

		//cc->update();
	}


	delete server;
	server = nullptr;

	return 0;
}
