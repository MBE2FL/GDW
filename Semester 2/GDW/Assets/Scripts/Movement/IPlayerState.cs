using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IPlayerState
{
    void Entry(Movement movement, Rigidbody rb, Transform transform, Moveable moveable, Animator animtor);
    IPlayerState input();
    void update();
    void fixedUpdate();
}
