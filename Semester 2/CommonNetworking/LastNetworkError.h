#pragma once
#include <string>
class LastNetworkError
{
public:
	static std::string GetLastError();
	static void SetLastError(const char *head,int code);
private:
	static std::string last;
};