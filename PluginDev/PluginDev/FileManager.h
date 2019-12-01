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


	void save(char* filePath, float* data, int numObjs, int stride);
	void load(char* filePath, int stride);
	float* getData() const;
	int getNumObjs() const;

	void logMetrics(char* filePath, int kills, float accuracy, int adrenCounter);

private:
	float* _data = nullptr;
	int _numObjs = 0;
};