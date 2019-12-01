#pragma once

#include "PluginSettings.h"
#include "FileManager.h"

#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

	PLUGIN_API void save(char* filePath, float* data, int numObjs, int stride);
	PLUGIN_API void load(char* filePath, int stride);
	PLUGIN_API float* getData();
	PLUGIN_API int getNumObjs();
	PLUGIN_API void logMetrics(char* filePath, int kills, float accuracy, int adrenCounter);


#ifdef __cplusplus
}
#endif // __cplusplus
