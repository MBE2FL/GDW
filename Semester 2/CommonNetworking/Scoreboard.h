#pragma once
#include <string>
#include <fstream>
#include <iostream>
#include <vector>
#include <algorithm>
#include "ScorePacket.h"

//struct Time
//{
//	int minutes;
//	float seconds;
//};
//
//struct PlayerTime
//{
//	char* teamName;
//	Time totalTime;
//};

class Scoreboard
{
public:
	Scoreboard();
	~Scoreboard();

	void Read();
	void Write();

	void Sort();

	std::vector<ScoreData>& getTimes();


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
	
	std::vector<ScoreData> playerTimes;
};

