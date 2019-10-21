using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveControllerState : IPlayerState
{
    Movement _movement;
    Rigidbody _rb;
    Transform _transform;
    Moveable _moveable;
    private float _controllerPosX = 0.0f;
    private float _controllerMovementVert = 1;
    private float _controllerMovementHori = 1;

    public void Entry(Movement movement, Rigidbody rb, Transform transform, Moveable moveable)
    {
        _movement = movement;
        _rb = rb;
        _transform = transform;
        _moveable = moveable;
    }

    public IPlayerState input()
    {
        // Change to jump state.
        if (Input.GetButtonDown("Fire1") && _movement.OnGround && !_moveable.holdingObject)
            return Movement.JumpState;

        // Change to keyboard movement state.
        if (Input.anyKey)
            return Movement.MoveKeyboardState;

        // Remain in this state.
        return null;
    }

    public void update()
    {
        _controllerPosX += Input.GetAxis("HorizontalC");
    }

    public void fixedUpdate()
    {
        _movement.rampDetection();

        _controllerMovementVert = Input.GetAxis("Vertical") * -1f;
        _controllerMovementHori = Input.GetAxis("Horizontal") * -1f;


        if (_movement.OnGround)
        {
            if (_controllerMovementVert == 1)
            {
                if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Length != 0)
                    _transform.rotation = Quaternion.Euler(0, _controllerPosX, 0);


                if (_movement.Angle > 0)
                    _rb.AddForce((_transform.forward * 8.0f) * 1.8f * 2.0f);
                else
                    _rb.AddForce(_transform.forward * 8.0f * 2.0f);

            }

            if (_controllerMovementVert == -1)
            {
                if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Length != 0)
                    _transform.rotation = Quaternion.Euler(0, _controllerPosX, 0);

                _rb.AddForce(_transform.forward * -8.0f * 2.0f);
            }

            if (_controllerMovementHori == 1)
            {
                if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Length != 0)
                    _transform.rotation = Quaternion.Euler(0, _controllerPosX, 0);

                _rb.AddForce(_transform.right * -8.0f * 2.0f);
            }

            if (_controllerMovementHori == -1)
            {
                if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Length != 0)
                    _transform.rotation = Quaternion.Euler(0, _controllerPosX, 0);

                _rb.AddForce(_transform.right * 8.0f * 2.0f);
            }
        }

        _movement.Angle = 0.0f;
    }
}
