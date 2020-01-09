#pragma once

#ifdef EDITORPLUGIN_EXPORTS
#define PLUGIN_API __declspec(dllexport)
#elif EDITORPLUGIN_IMPORTS
#define PLUGIN_API __declspec(dllimport)
#else
#define PLUGIN_API
#endif