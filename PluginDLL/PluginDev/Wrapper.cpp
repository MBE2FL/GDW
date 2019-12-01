#include "Wrapper.h"
#include "FileManager.h"

FileManager fm;

PLUGIN_API
int load(char* filePath)
{
	return fm.load(filePath);
}
void logCurrPuzzles(char* filePath, int currPuzzles)
{
	return fm.logCurrPuzzles(filePath, currPuzzles);
}
