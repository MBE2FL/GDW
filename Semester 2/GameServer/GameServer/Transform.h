#pragma once

struct Vector3
{
	Vector3() { x = 0; y = 0; z = 0; }
	Vector3(const Vector3 &other) { x = other.x; y = other.y; z = other.z; }
	float x;
	float y;
	float z;
};

struct Quaternion
{
	Quaternion() { x = 0; y = 0; z = 0; w = 1; }
	Quaternion(const Quaternion &other) { x = other.x; y = other.y; z = other.z; w = other.w; }
	float x;
	float y;
	float z;
	float w;
};

class Transform
{
public:
	Transform();
	
	void send(const Vector3 &position, const Quaternion &rotation);

	Vector3 _position;
	Quaternion _rotation;
};