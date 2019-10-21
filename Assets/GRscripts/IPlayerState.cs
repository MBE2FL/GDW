using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IPlayerState
{
    void Entry(Movement movement, Rigidbody rb, Transform transform, Moveable moveable);
    IPlayerState input();
    void update();
    void fixedUpdate();
}
