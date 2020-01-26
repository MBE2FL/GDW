using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushPullState : IPlayerState
{
    Movement _movement;
    Rigidbody _rb;
    Transform _transform;
    Moveable _moveable;
    float _mousePosX;
    Transform camTransform;
    bool _forwardMovement = true;

    public bool ForwardMovement
    {
        get
        {
            return _forwardMovement;
        }
        set
        {
            _forwardMovement = value;
        }
    }

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
        // Change to keyboard movement state.
        if (!_moveable.holdingObject)
            return Movement.MoveKeyboardState;

        // Change to controller movement state.
        //if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Length != 0)
        //    return Movement.MoveControllerState;

        // Remain in this state.
        return null;
    }

    public void update()
    {
        _mousePosX += Input.GetAxis("Mouse X");
    }

    public void fixedUpdate()
    {
        _movement.rampDetection();

        // Move only while on ground.
        if (_movement.OnGround)
        {
            // Move forward
            if (Input.GetKey(KeyCode.W) && _forwardMovement)
            {
                //_transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);

                // Increase forward speed while moving up ramps.
                if (_movement.Angle > 0)
                    _rb.AddForce((_transform.forward * 8) * 1.8f * 2.0f);
                else
                    _rb.AddForce(_transform.forward * 8 * 2.0f);
            }

            // Move backward
            if (Input.GetKey(KeyCode.S))
            {
                //_transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);
                _rb.AddForce(_transform.forward * -8 * 2.0f);
            }
        }

        _movement.Angle = 0.0f;
    }
}
