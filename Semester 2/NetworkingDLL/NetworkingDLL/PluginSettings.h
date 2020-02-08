#pragma once

#ifdef NETWORKINGDLL_EXPORTS
#define PLUGIN_API __declspec(dllexport)
#elif NETWORKINGDLL_IMPORTS
#define PLUGIN_API __declspec(dllimport)
#else
#define PLUGIN_API
#endif