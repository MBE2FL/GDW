#pragma once
#include <string>
#include <fstream>
#include <iostream>
#include <vector>
#include <algorithm>

struct time
{
	int minutes;
	float seconds;
};

struct playerTime
{
	std::string teamName;
	time totalTime;
};

class Scoreboard
{
public:
	Scoreboard();
	~Scoreboard();

	void Read();
	void Write();

	void Sort();


private:
	//struct time
	//{
	//	int minutes;
	//	float seconds;
	//};

	//struct playerTime
	//{
	//	std::string teamName;
	//	time totalTime;
	//};
	
	//bool CompareTime(playerTime& t1, playerTime& t2);
	
	std::vector<playerTime> playerTimes;
};

