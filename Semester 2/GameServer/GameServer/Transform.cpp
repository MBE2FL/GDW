#include "Transform.h"

Transform::Transform()
{
	_position = Vector3();
	_rotation = Quaternion();
}

void Transform::send(const Vector3 &position, const Quaternion &rotation)
{
	_position = position;
	_rotation = rotation;
}
