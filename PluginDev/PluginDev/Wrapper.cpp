#include "Wrapper.h"
#include "FileManager.h"

FileManager fm;

PLUGIN_API void save(char* filePath, float* data, int numObjs, int stride)
{
	return fm.save(filePath, data, numObjs, stride);
}

PLUGIN_API void load(char* filePath, int stride)
{
	return fm.load(filePath, stride);
}

PLUGIN_API float* getData()
{
	return fm.getData();
}

PLUGIN_API int getNumObjs()
{
	return fm.getNumObjs();
}

PLUGIN_API void logMetrics(char* filePath, int kills, float accuracy, int adrenCounter)
{
	return fm.logMetrics(filePath, kills, accuracy, adrenCounter);
}
