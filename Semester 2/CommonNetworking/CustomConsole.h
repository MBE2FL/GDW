#pragma once
#include <Windows.h>
#include <stdio.h>
#include <ctime>

#define WIDTH 120
#define HEIGHT 30

#define STATUS_ROWS_START 1
#define STATUS_ROWS_END 4

#define BG_WHITE (BACKGROUND_RED | BACKGROUND_GREEN | BACKGROUND_BLUE | BACKGROUND_INTENSITY)

using std::clock;


class CustomConsole
{
public:
	~CustomConsole();

	static CustomConsole* getInstance();

	void update();

	void clearColour();
	void writeToStatus(unsigned int numClients);

private:
	HANDLE _consoleStdOut;
	CONSOLE_SCREEN_BUFFER_INFO _csbi;
	WORD _oldColourAttrs;

	CHAR_INFO _consoleBuffer[WIDTH * HEIGHT];

	SMALL_RECT _windowSize = { 0, 0, WIDTH - 1, HEIGHT - 1 };
	COORD _bufferSize = { WIDTH, HEIGHT };

	// WriteConsoleOutput variables.
	COORD _charBufferSize = { WIDTH, HEIGHT };
	COORD _charPos = { 0, 0 };
	SMALL_RECT _consoleWriteArea = { 0, 0, WIDTH - 1, HEIGHT - 1 };



	clock_t _startClock;
	float _totalTime = 0;

	CustomConsole();
	static CustomConsole* _instance;
};