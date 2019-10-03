using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    // Update is called once per frame
    public Rigidbody rb;
    CursorLockMode cursorLock;
    bool actuallyOnGround = false;
    Vector3 rayPos;
    Vector3 otherNormal;
    float angle = 0.0f;

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
        rayPos = new Vector3(transform.position.x, transform.position.y - 0.85f, transform.position.z);
        otherNormal = transform.TransformDirection(Vector3.forward);
        RaycastHit ray;
        if (Physics.Raycast(rayPos, transform.TransformDirection(Vector3.forward), out ray, 0.5f, 1 << 8))
        {
            //Debug.DrawRay(rayPos, transform.TransformDirection(Vector3.forward) * ray.distance, Color.yellow);
            //Debug.Log("hit");
            angle = Mathf.Acos(Vector3.Dot(ray.normal, otherNormal)) * Mathf.Rad2Deg;
            angle -= 90;
            //Debug.Log(angle);
        }
    }
    
    void Update()
    {

        doCursor();

        if (Input.GetKeyDown(KeyCode.Space) && onGround)
        {
            rb.AddForce(new Vector3(0, 300, 0));
            Debug.Log("yes");
        }
    }

    private void FixedUpdate()
    {
        rampDetection();


        if (Input.GetKey(KeyCode.W) && onGround)
        {
            if (angle > 0)
                rb.AddForce(new Vector3(0, 0, 8 * 1.8f));
            else
                rb.AddForce(new Vector3(0, 0, 8));
        }

        if (Input.GetKey(KeyCode.S) && onGround)
        {
            rb.AddForce(new Vector3(0, 0, -8));
        }

        if (Input.GetKey(KeyCode.A) && onGround)
        {
            rb.AddForce(new Vector3(-8, 0, 0));
        }

        if (Input.GetKey(KeyCode.D) && onGround)
        {
            rb.AddForce(new Vector3(8, 0, 0));
        }
        angle = 0.0f;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject)
            onGround = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject)
        onGround = false;

    }
}
