#include "CustomConsole.h"
#include <cmath>
#include <string>

using std::string;

CustomConsole* CustomConsole::_instance = nullptr;

CustomConsole::CustomConsole()
{
	//HANDLE console;
	//console = GetStdHandle(STD_OUTPUT_HANDLE);
	//SetConsoleTextAttribute(console, 15);

	
	SetConsoleTitle("Before She Goes Dedicated Server");

	// Get handle.
	_consoleStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
	if (_consoleStdOut == INVALID_HANDLE_VALUE)
	{
		MessageBox(NULL, TEXT("GetStdHandle"), TEXT("Console Error"), MB_OK);
		system("pause");
		exit(1);
	}

	//// Set the window size.
	//SetConsoleWindowInfo(_consoleStdOut, TRUE, &_windowSize);

	//// Set the screen's buffer size.
	//SetConsoleScreenBufferSize(_consoleStdOut, _bufferSize);

	//// Save the current text colours.
	//if (!GetConsoleScreenBufferInfo(_consoleStdOut, &_csbi))
	//{
	//	MessageBox(NULL, TEXT("GetConsoleScreenBufferInfo"), TEXT("Console Error"), MB_OK);
	//	system("pause");
	//	exit(1);
	//}

	//_oldColourAttrs = _csbi.wAttributes;
	////_cellCount = _csbi.dwSize.X * _csbi.dwSize.Y;

	//// Set the text attributes to draw red text on black background.
	//if (!SetConsoleTextAttribute(_consoleStdOut, FOREGROUND_RED | FOREGROUND_INTENSITY | BG_WHITE))
	//{
	//	MessageBox(NULL, TEXT("SetConsoleTextAttribute"), TEXT("Console Error"), MB_OK);
	//	system("pause");
	//	exit(1);
	//}



	//for (int rowY = 0; rowY < HEIGHT; ++rowY)
	//{
	//	for (int colX = 0; colX < WIDTH; ++colX)
	//	{
	//		//_consoleBuffer[colX + (WIDTH * rowY)].Char.AsciiChar = static_cast<unsigned char>(219);
	//		//_consoleBuffer[colX + (WIDTH * rowY)].Attributes = 255 * abs(sin((rowY + colX) / 4));// * 255;
	//		_consoleBuffer[colX + (WIDTH * rowY)].Char.AsciiChar = ' ';
	//		_consoleBuffer[colX + (WIDTH * rowY)].Attributes = BG_WHITE;
	//	}
	//}

	//// Write out console buffer.
	//WriteConsoleOutputA(_consoleStdOut, _consoleBuffer, _charBufferSize, _charPos, &_consoleWriteArea);

	//for (int rowY = STATUS_ROWS_START; rowY < STATUS_ROWS_END; ++rowY)
	//{
	//	for (int colX = 0; colX < WIDTH; ++colX)
	//	{
	//		//_consoleBuffer[colX + (WIDTH * rowY)].Char.AsciiChar = static_cast<unsigned char>(219);
	//	}
	//}


	//COORD statusCoords;
	//statusCoords.X = STATUS_ROWS_START;
	//statusCoords.Y = 0;
	//DWORD _charsWritten;
	//string status;

	//status = "*************************";
	//WriteConsoleOutputCharacterA(_consoleStdOut, status.c_str(), status.length(), statusCoords, &_charsWritten);
	//++statusCoords.X;
	//status = "Clients Connected: 0";
	//WriteConsoleOutputCharacterA(_consoleStdOut, status.c_str(), status.length(), statusCoords, &_charsWritten);
	//++statusCoords.X;
	//status = "UDP Socket: Offline";
	//WriteConsoleOutputCharacterA(_consoleStdOut, status.c_str(), status.length(), statusCoords, &_charsWritten);
	//++statusCoords.X;
	//status = "TCP Socket: Offline";
	//WriteConsoleOutputCharacterA(_consoleStdOut, status.c_str(), status.length(), statusCoords, &_charsWritten);
	//++statusCoords.X;
	//status = "Server: Offline";
	//WriteConsoleOutputCharacterA(_consoleStdOut, status.c_str(), status.length(), statusCoords, &_charsWritten);
	//++statusCoords.X;
	//status = "*************************";
	//WriteConsoleOutputCharacterA(_consoleStdOut, status.c_str(), status.length(), statusCoords, &_charsWritten);




	_startClock = clock();
}

CustomConsole::~CustomConsole()
{
}

CustomConsole* CustomConsole::getInstance()
{
	if (!_instance)
		_instance = new CustomConsole();
	
	return _instance;
}

void CustomConsole::update()
{
	_totalTime = (clock() - _startClock) / static_cast<float>(CLOCKS_PER_SEC);


	char buf[WIDTH];
	memset(buf, 0, WIDTH);
	sprintf_s(buf, "Server Time: %u Hrs %u Mins, %u Secs", (static_cast<unsigned int>(_totalTime) / 60) / 60, static_cast<unsigned int>(_totalTime) / 60, static_cast<unsigned int>(_totalTime) % 60);
	for (int colX = 0; colX < WIDTH; ++colX)
	{
		if (buf[colX] == '\0')
			break;

		_consoleBuffer[colX].Char.AsciiChar = buf[colX];
		_consoleBuffer[colX].Attributes = FOREGROUND_GREEN | BG_WHITE;
	}

	// Write out console buffer.
	WriteConsoleOutputA(_consoleStdOut, _consoleBuffer, _charBufferSize, _charPos, &_consoleWriteArea);
}

void CustomConsole::clearColour()
{
	//for (int rowY = 0; rowY < HEIGHT; ++rowY)
	//{
	//	for (int colX = 0; colX < WIDTH; ++colX)
	//	{
	//		_consoleBuffer[colX + (WIDTH * rowY)].Char.AsciiChar = ' ';
	//		_consoleBuffer[colX + (WIDTH * rowY)].Attributes = BG_WHITE;
	//	}
	//}
}

void CustomConsole::writeToStatus(unsigned int numClients)
{
	char buf[WIDTH];
	memset(buf, 0, WIDTH);
	sprintf_s(buf, "Clients Connected: %u", numClients);

	for (int colX = 0; colX < WIDTH; ++colX)
	{
		if (buf[colX] == '\0')
			break;

		_consoleBuffer[colX + (WIDTH * 1)].Char.AsciiChar = buf[colX];
		_consoleBuffer[colX + (WIDTH * 1)].Attributes = FOREGROUND_GREEN | BG_WHITE;
	}
}
