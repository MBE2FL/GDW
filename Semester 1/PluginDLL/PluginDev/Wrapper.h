#pragma once

#include "PluginSettings.h"
#include "FileManager.h"

#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

	PLUGIN_API int load(char* filePath);
	PLUGIN_API void logCurrPuzzles(char* filePath, int currPuzzles);

	PLUGIN_API void sendTransform()


#ifdef __cplusplus
}
#endif // __cplusplus
