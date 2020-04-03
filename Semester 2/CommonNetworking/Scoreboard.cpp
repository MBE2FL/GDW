#include "Scoreboard.h"
using std::ifstream;
using std::ofstream;


Scoreboard::Scoreboard()
{
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
		PlayerTime temp;

		while (!in.eof())
		{
			std::getline(in, buffer);
			if (buffer != "")
			{
				temp.teamName = buffer.c_str();

				std::getline(in, buffer);
				temp.totalTime.minutes = std::stoi(buffer);

				std::getline(in, buffer);
				temp.totalTime.seconds = std::stof(buffer);
				playerTimes.push_back(temp);
			}
		}
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
			output += playerTimes[i].totalTime.minutes;
			output += "\n";
			output += playerTimes[i].totalTime.seconds;
			output += "\n";
		}
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

