using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class betterJumping : MonoBehaviour
{
    private float fallMultiplier = 2.5f;
    private float jumpMultiplier = 2f;

    Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if(rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if(rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (jumpMultiplier - 1) * Time.deltaTime;
        }
    }
}
