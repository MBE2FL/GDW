#include "Scoreboard.h"
using std::ifstream;
using std::ofstream;


void Scoreboard::Read()
{
	ifstream in("Scoreboard.txt");
	if (in.is_open())
	{
		std::string buffer;
		playerTime temp;

		while (!in.eof())
		{
			std::getline(in, buffer);
			if (buffer != "")
			{
				temp.teamName = buffer;

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

bool CompareTime(playerTime& t1, playerTime& t2)
{
	float sec1 = (float)t1.totalTime.minutes * 60 + t1.totalTime.seconds;
	float sec2 = (float)t2.totalTime.minutes * 60 + t2.totalTime.seconds;
	
	return (sec1 > sec2);
}

void Scoreboard::Sort()
{
	std::sort(playerTimes.begin(), playerTimes.end(), CompareTime);
}

