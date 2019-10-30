using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : IPlayerState
{
    Movement _movement;
    Rigidbody _rb;
    Transform _transform;
    Moveable _moveable;

    public void Entry(Movement movement, Rigidbody rb, Transform transform, Moveable moveable)
    {
        _movement = movement;
        _rb = rb;
        _transform = transform;
        _moveable = moveable;


        _rb.AddForce(new Vector3(0, 200, 0));
    }

    public IPlayerState input()
    {
        // Change to move state.
        if (_movement.OnGround)
            return Movement.MoveKeyboardState;

        // Remain in this state.
        return null;
    }

    public void update()
    {

    }

    public void fixedUpdate()
    {
        // Implement air controls
    }
}
