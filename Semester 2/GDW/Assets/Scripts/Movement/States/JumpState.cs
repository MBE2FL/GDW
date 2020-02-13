using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : IPlayerState
{
    Movement _movement;
    Rigidbody _rb;
    Transform _transform;
    Moveable _moveable;
    Animator _animator;

    public void Entry(Movement movement, Rigidbody rb, Transform transform, Moveable moveable, Animator animator)
    {
        _movement = movement;
        _rb = rb;
        _transform = transform;
        _moveable = moveable;
        _animator = animator;

        //_animator.SetBool("jump", true);
        _rb.AddForce(new Vector3(0, 300, 0));
        //_animator.SetBool("jump", false);
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
