using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    static MoveKeyboardState _moveKeyboardState = new MoveKeyboardState();
    static JumpState _jumpState = new JumpState();
    static MoveControllerState _moveControllerState = new MoveControllerState();
    static PushPullState _pushPullState = new PushPullState();
    
    private IPlayerState _currentState = MoveKeyboardState;
    private IPlayerState _newState = null;

    public Rigidbody rb;
    CursorLockMode cursorLock;
    Vector3 rayPos;
    Vector3 otherNormal;
    Moveable _moveable;
    Animator _animator;

    private float angle = 0.0f;
    //private float mousePosX = 0.0f;
    private float controllerPosX = 0.0f;
    private float controllerMovementVert = 0.1f;
    private float controllerMovementHori = 0.1f;

    bool onGround = true;

    public float Angle
    {
        get
        {
            return angle;
        }
        set
        {
            angle = value;
        }
    }
    public bool OnGround
    {
        get
        {
            return onGround;
        }
        set
        {
            onGround = value;
        }
    }

    public static MoveKeyboardState MoveKeyboardState
    {
        get
        {
            return _moveKeyboardState;
        }
    }

    public static JumpState JumpState
    {
        get
        {
            return _jumpState;
        }
    }

    public static MoveControllerState MoveControllerState
    {
        get
        {
            return _moveControllerState;
        }
    }

    public static PushPullState PushPullState
    {
        get
        {
            return _pushPullState;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        _moveable = GetComponent<Moveable>();
        _animator = GetComponent<Animator>();
        _currentState.Entry(this, rb, transform, _moveable, _animator);
    }

    void doCursor()
    {
        Cursor.lockState = cursorLock;
        cursorLock = CursorLockMode.Locked;
    }

    public void rampDetection()
    {
        rayPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        otherNormal = transform.TransformDirection(Vector3.forward);
        RaycastHit ray;
        if (Physics.Raycast(rayPos, transform.TransformDirection(Vector3.forward), out ray, 0.5f, 1 << 10))
        {
            angle = Mathf.Acos(Vector3.Dot(ray.normal, otherNormal)) * Mathf.Rad2Deg;
            angle -= 90;
            //Debug.Log(angle);
        }
    }
    
    void Update()
    {

        doCursor();

        _newState = _currentState.input();

        if (_newState == null)
            _currentState.update();
        else
        {
            _currentState = _newState;
            _currentState.Entry(this, rb, transform, _moveable, _animator);
        }
    }

    private void FixedUpdate()
    {
        if (_newState == null)
            _currentState.fixedUpdate();
    }

    private void OnTriggerStay(Collider collision)
    {
        //if (collision.gameObject.tag == "ground" || collision.gameObject.tag == "Interactable")
            onGround = true;
    }

    private void OnTriggerExit(Collider collision)
    {
        //if(collision.gameObject.tag == "ground" || collision.gameObject.tag == "Interactable")
        onGround = false;

    }
}
