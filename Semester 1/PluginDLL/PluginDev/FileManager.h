#pragma once
#include "PluginSettings.h"
#include <fstream>

using std::ofstream;
using std::ifstream;
using std::ios;
using std::streamsize;


class PLUGIN_API FileManager
{
public:
	FileManager();
	~FileManager();

	int load(char* filePath);
	void logCurrPuzzles(char* filePath, int currPuzzles);

private:
};