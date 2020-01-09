#include "FileManager.h"

FileManager::FileManager()
{
}

FileManager::~FileManager()
{
}


int FileManager::load(char* filePath)
{
	ifstream file(filePath, ios::in);
	int currPuzzles = 0;

	if (file.is_open())
	{
		file >> currPuzzles;

		file.close();
		return currPuzzles;
	}
	return 0;
}


void FileManager::logCurrPuzzles(char* filePath, int currPuzzles)
{
	ofstream file(filePath, ios::out);

	if (file.is_open())
	{
		file << currPuzzles;

		file.close();
	}
}
