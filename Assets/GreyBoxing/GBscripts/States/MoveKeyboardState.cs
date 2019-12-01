using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveKeyboardState : IPlayerState
{
    Movement _movement;
    Rigidbody _rb;
    Transform _transform;
    Moveable _moveable;
    float _mousePosX;
    //Vector3 _force;
    Transform camTransform;

    Vector3 _force;
    float _speed = 0.0f;

    public void Entry(Movement movement, Rigidbody rb, Transform transform, Moveable moveable)
    {
        _movement = movement;
        _rb = rb;
        _transform = transform;
        _moveable = moveable;
        camTransform = Camera.main.transform;
    }

    public IPlayerState input()
    {
        // Change to jump state.
        if (Input.GetKeyDown(KeyCode.Space) && _movement.OnGround && !_moveable.holdingObject)
            return Movement.JumpState;

        // Change to controller movement state.
        if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Length != 0)
            return Movement.MoveControllerState;

        // Change to push pull state.
        if (_moveable.holdingObject)
            return Movement.PushPullState;

        // Remain in this state.
        return null;
    }

    public void update()
    {
        _mousePosX += Input.GetAxis("HorizontalC");
    }

    public void fixedUpdate()
    {
        //_force *= 2.0f;
        //_rb.AddForce(_force);
        //Debug.Log(_force);
        _force = Vector3.zero;
        _speed = 0.0f;

        _movement.rampDetection();

        // Move only while on ground.
        if (_movement.OnGround)
        {
            // Move forward
            if (Input.GetKey(KeyCode.W))
            {
                //_transform.rotation = Quaternion.Euler(0, _mousePosX, 0);
                _transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);

                // Increase forward speed while moving up ramps.
                //if (_movement.Angle > 5.0f)
                    //_rb.AddForce((_transform.forward * 8) * 1.8f * 2.0f);
                //else
                    //_rb.AddForce(_transform.forward * 8 * 2.0f);

                _force += _transform.forward;
            }

            // Move backward
            if (Input.GetKey(KeyCode.S))
            {
                _transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);
                //_rb.AddForce(_transform.forward * -8 * 2.0f);
                _force -= _transform.forward;
            }

            // Move left
            if (Input.GetKey(KeyCode.A))
            {
                _transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);
                //_rb.AddForce(_transform.right * -8 * 2.0f);
                _force -= _transform.right;
            }

            // Move right
            if (Input.GetKey(KeyCode.D))
            {
                _transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);
                //_rb.AddForce(_transform.right * 8 * 2.0f);
                _force += _transform.right;
            }

            if (_movement.Angle > 5.0f)
                _speed = 8 * 1.8f * 2.0f;
            else
                _speed = 8.0f * 2.0f;

            _force.Normalize();

            _rb.AddForce(_force * _speed);
        }

        Vector3 localVel = _transform.InverseTransformDirection(_rb.velocity);
        if (_movement.Angle > 5.0f)
        {
            localVel.x = Mathf.Clamp(localVel.x, -15.0f, 15.0f);
            localVel.z = Mathf.Clamp(localVel.z, -15.0f, 15.0f);
            Debug.Log("Ramp");
        }
        else
        {
            localVel.x = Mathf.Clamp(localVel.x, -8.0f, 8.0f);
            localVel.z = Mathf.Clamp(localVel.z, -8.0f, 8.0f);
            Debug.Log("Non-Ramp");
        }

        _rb.velocity = _transform.TransformDirection(localVel);


        _movement.Angle = 0.0f;
    }
}
