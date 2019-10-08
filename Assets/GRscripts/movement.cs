using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    // Update is called once per frame
    public Rigidbody rb;
    CursorLockMode cursorLock;
    Vector3 rayPos;
    Vector3 otherNormal;

    private float angle = 0.0f;
    private float mousePosX = 0.0f;
    private float mousePosY = 0.0f;

    bool onGround = true;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void doCursor()
    {
        Cursor.lockState = cursorLock;
        cursorLock = CursorLockMode.Locked;
    }

    void rampDetection()
    {
        rayPos = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);
        otherNormal = transform.TransformDirection(Vector3.forward);
        RaycastHit ray;
        if (Physics.Raycast(rayPos, transform.TransformDirection(Vector3.forward), out ray, 0.5f, 1 << 8))
        {
            angle = Mathf.Acos(Vector3.Dot(ray.normal, otherNormal)) * Mathf.Rad2Deg;
            angle -= 90;
        }
    }
    
    void Update()
    {

        doCursor();

        if (Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            rb.AddForce(new Vector3(0, 200, 0));
        }

        mousePosX += Input.GetAxis("Mouse X");
        mousePosY += Input.GetAxis("Mouse Y");

        
    }

    private void FixedUpdate()
    {
        rampDetection();


        if (Input.GetKey(KeyCode.W) && onGround)
        {
            if (angle > 0)
                rb.AddForce((transform.forward * 8) * 1.8f);
            else
                rb.AddForce(transform.forward * 8);
        }

        if (Input.GetKey(KeyCode.S) && onGround)
        {
            rb.AddForce(transform.forward * -8);
        }

        if (Input.GetKey(KeyCode.A) && onGround)
        {
            rb.AddForce(transform.right * -8);
        }

        if (Input.GetKey(KeyCode.D) && onGround)
        {
            rb.AddForce(transform.right * 8);
        }
        angle = 0.0f;
        transform.rotation = Quaternion.Euler(0, mousePosX, 0);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "ground" || collision.gameObject.tag == "interactable")
            onGround = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "ground" || collision.gameObject.tag == "interactable")
        onGround = false;

    }
}
