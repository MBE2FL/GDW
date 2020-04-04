#include "Scoreboard.h"
#include <stdlib.h>
using std::ifstream;
using std::ofstream;
using std::to_string;


Scoreboard::Scoreboard()
{
//	for (int i = 0; i < 50; i++)
//	{
//		PlayerTime temp;
//	
//		std::string str = "Name: ";
//		str += to_string(i);
//
//
//
//		temp.teamName = new char[512];
//		memset(temp.teamName, 0, 512);
//		memcpy(temp.teamName, str.c_str(), sizeof(str.c_str()));
//		//temp.teamName = str.c_str();
//		//temp.teamName = itoa(i, temp.teamName);
//		temp.totalTime.minutes = rand()% 60;
//		temp.totalTime.seconds = rand()% 60;
//		//std::cout << temp.teamName << "\n\n\n\n\n\n";
//		playerTimes.push_back(temp);
//
//	}
//	std::cout << "Defaut values:\n";
//	for (int i = 0; i < playerTimes.size(); i++)
//	{
//		std::cout << playerTimes[i].teamName << ": " <<
//			playerTimes[i].totalTime.minutes << ":" << playerTimes[i].totalTime.seconds << std::endl;
//	}
//	std::cout << "\n\n\n";
//
//	Write();
//	//std::cin >> temp;
//
//	Sort();
//	std::cout << "Sorted:\n";
//	for (int i = 0; i < playerTimes.size(); i++)
//	{
//		std::cout << playerTimes[i].teamName << ": " << 
//			playerTimes[i].totalTime.minutes << ":" << playerTimes[i].totalTime.seconds << std::endl;
//	}
//	std::cout << "\n\n\n";
//	std::cout << "Read Values:\n";
//	//std::cin >> temp;
//	Read();
//	for (int i = 0; i < playerTimes.size(); i++)
//	{
//		std::cout << playerTimes[i].teamName << ": " <<
//			playerTimes[i].totalTime.minutes << ":" << playerTimes[i].totalTime.seconds << std::endl;
//	}
//
//	//std::cin >> temp;
//
//	std::cout << "\n\n\n";
//	std::cout << "Sorted Read Values:\n";
//	Sort();
//	for (int i = 0; i < playerTimes.size(); i++)
//	{
//		std::cout << playerTimes[i].teamName << ": " <<
//			playerTimes[i].totalTime.minutes << ":" << playerTimes[i].totalTime.seconds << std::endl;
//	}
//
}

Scoreboard::~Scoreboard()
{
}

void Scoreboard::Read()
{
	ifstream in("Scoreboard.txt");
	if (in.is_open())
	{
		std::string buffer;
		
		playerTimes.clear();
		while (!in.eof())
		{
			PlayerTime temp;
			std::getline(in, buffer);
			if (buffer != "")
			{
				//std::cout << "teamName: " << buffer << "\n";
				temp.teamName = new char[512];
				memset(temp.teamName, 0, 512);
				memcpy(temp.teamName, buffer.c_str(), sizeof(buffer.c_str()));
				//std::cout << "teamName: " << temp.teamName << "\n";
				
				std::getline(in, buffer);
				//std::cout << "minutes: " << buffer << "\n";
				
				temp.totalTime.minutes = std::stoi(buffer);
				//std::cout << "minutes: " << temp.totalTime.minutes << "\n";
				

				std::getline(in, buffer);
				//std::cout << "seconds: " << buffer << "\n";
				temp.totalTime.seconds = std::stof(buffer);
				//std::cout << "seconds: " << temp.totalTime.seconds << "\n";
				playerTimes.push_back(temp);
			}
		}
	}
	else
	{
		std::cout << "Did not open.\n";
	}
}

void Scoreboard::Write()
{
	ofstream out("Scoreboard.txt", std::ios::trunc);
	if (out.is_open())
	{
		std::string output = "";
		for (int i = 0; i < playerTimes.size(); i++)
		{
			output += playerTimes[i].teamName;
			output += "\n";
			output += to_string(playerTimes[i].totalTime.minutes);
			output += "\n";
			output += to_string(playerTimes[i].totalTime.seconds);
			output += "\n";
		}
		out << output;
		out.close();
	}
}

bool CompareTime(PlayerTime& t1, PlayerTime& t2)
{
	float sec1 = (float)t1.totalTime.minutes * 60 + t1.totalTime.seconds;
	float sec2 = (float)t2.totalTime.minutes * 60 + t2.totalTime.seconds;
	
	return (sec1 < sec2);
}

void Scoreboard::Sort()
{
	std::sort(playerTimes.begin(), playerTimes.end(), CompareTime);
}

std::vector<PlayerTime>& Scoreboard::getTimes()
{
	return playerTimes;
}

