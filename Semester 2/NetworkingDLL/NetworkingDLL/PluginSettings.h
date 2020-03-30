#pragma once

//#ifdef NETWORKINGDLL_EXPORTS
//#define PLUGIN_API __declspec(dllexport)
//#elif NETWORKINGDLL_IMPORTS
//#define PLUGIN_API __declspec(dllimport)
//#else
//#define PLUGIN_API
//#endif

#ifndef PLUGIN_OUT
// Put this in front of each function you want to export
#define PLUGIN_OUT __declspec(dllexport)
#endif