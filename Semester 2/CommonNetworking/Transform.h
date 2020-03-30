#pragma once
#include "../NetworkingDLL/NetworkingDLL/PluginSettings.h"
#include <string>

using std::string;
using std::to_string;

struct PLUGIN_OUT Vector3
{
	Vector3() { _x = 0; _y = 0; _z = 0; }
	Vector3(const Vector3 &other) { _x = other._x; _y = other._y; _z = other._z; }
	Vector3(float x, float y, float z) : _x(x), _y(y), _z(z) {}

	string toString()
	{
		return "x: " + to_string(_x) +
			"\ny: " + to_string(_y) +
			"\nz: " + to_string(_z) + "\n";
	}

	float _x;
	float _y;
	float _z;
};

struct PLUGIN_OUT Quaternion
{
	Quaternion() { _x = 0; _y = 0; _z = 0; _w = 1; }
	Quaternion(const Quaternion &other) { _x = other._x; _y = other._y; _z = other._z; _w = other._w; }

	string toString()
	{
		return "x: " + to_string(_x) +
			"\ny: " + to_string(_y) +
			"\nz: " + to_string(_z) +
			"\nw: " + to_string(_w) + "\n";
	}

	float _x;
	float _y;
	float _z;
	float _w;
};

class PLUGIN_OUT Transform
{
public:
	Transform();
	
	void send(const Vector3 &position, const Quaternion &rotation);

	Vector3 _position;
	Quaternion _rotation;
};