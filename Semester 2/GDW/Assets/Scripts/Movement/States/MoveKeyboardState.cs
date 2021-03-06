﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveKeyboardState : IPlayerState
{
    Movement _movement;
    Rigidbody _rb;
    Transform _transform;
    Moveable _moveable;
    float _mousePosX;
    Transform camTransform;
    Animator _animator;

    Vector3 _force;
    float _speed = 0.0f;
    float _speedRamp = 1.0f;
    float idleValue = 1.0f;

    public void Entry(Movement movement, Rigidbody rb, Transform transform, Moveable moveable, Animator animator)
    {
        _movement = movement;
        _rb = rb;
        _transform = transform;
        _moveable = moveable;
        _animator = animator;
        camTransform = Camera.main.transform;
    }

    public IPlayerState input()
    {
        // Change to jump state.
        if (Input.GetKeyDown(KeyCode.Space) && _movement.OnGround && !_moveable.holdingObject)
            return Movement.JumpState;

        // Change to controller movement state.
        //if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Length != 0)
        //    return Movement.MoveControllerState;

        // Change to push pull state.
        if (_moveable.holdingObject)
            return Movement.PushPullState;

        // Remain in this state.
        return null;
    }

    public void update()
    {
        _mousePosX += Input.GetAxis("HorizontalC");
        _movement.rampDetection();
    }

    public void fixedUpdate()
    {
        _force = Vector3.zero;
        _speed = 0.0f;


        // Move only while on ground.
        if (_movement.OnGround)
        {
            _rb.drag = 17.5f;
            _rb.angularDrag = 0.05f;


            // Move forward
            if (Input.GetKey(KeyCode.W))
            {
                _transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);
               _animator.SetBool("forward", true);
                _force += _transform.forward;
            }
            else
            {
                _animator.SetBool("forward", false);
            }

            

            // Move backward
            if (Input.GetKey(KeyCode.S))
            {
                _transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);
                _force -= _transform.forward;
            }

            // Move left
            if (Input.GetKey(KeyCode.A))
            {
                _transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);
                //_animator.SetBool("left", true);
                _force -= _transform.right;
            }
            else
            {
                //_animator.SetBool("left", false);
            }

            // Move right
            if (Input.GetKey(KeyCode.D))
            {
                _transform.rotation = Quaternion.Euler(0, camTransform.rotation.eulerAngles.y, 0);
                //_animator.SetBool("right", true);
                _force += _transform.right;
            }
            else
            {
               // _animator.SetBool("right", false);
            }

            if (_movement.Angle > 5.0f)
                _speed = 8 * 2f * 7.0f;
            else
                _speed = 8.0f * 7.0f;

            _force.Normalize();

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (_speedRamp < 3.0f)
                    _speedRamp += 0.07f;
                
            }
            else
            {
                if(_speedRamp >1.0f)
                    _speedRamp -= 0.07f;

            }

            
            _speed *= _speedRamp;
            _rb.AddForce(_force * _speed);

            if (_force == Vector3.zero)
            {
                if (idleValue > 0.0f)
                    idleValue = -0.07f;
                else
                    idleValue = 0.0f;

                _animator.SetFloat("speed", idleValue);
            }
            else
                _animator.SetFloat("speed", _speedRamp);

            Vector3 localVel = _transform.InverseTransformDirection(_rb.velocity);
            if (_movement.Angle > 5.0f)
            {
                localVel.x = Mathf.Clamp(localVel.x, -15.0f, 15.0f);
                localVel.z = Mathf.Clamp(localVel.z, -15.0f, 15.0f);
            }
            else
            {
                localVel.x = Mathf.Clamp(localVel.x, -8.0f, 8.0f);
                localVel.z = Mathf.Clamp(localVel.z, -8.0f, 8.0f);
            }

            _rb.velocity = _transform.TransformDirection(localVel);
            

            _movement.Angle = 0.0f;
        }
        else
        {
            _rb.drag = 0;
            _rb.angularDrag = 0;
        }
    }
}
