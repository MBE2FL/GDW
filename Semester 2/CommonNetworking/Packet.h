#pragma once
//#include <iostream>

#define BUF_LEN 512

enum MessageTypes : INT8
{
	ConnectionAttempt,
	ConnectionAccepted,
	ConnectionFailed,
	ServerFull,
	TransformData
};


class Packet
{
public:
	virtual void serialize() = 0;
	virtual void deserialize() = 0;


	char _data[BUF_LEN];

private:
	Packet& operator=(const Packet& other) {};
};