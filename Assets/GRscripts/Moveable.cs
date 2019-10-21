using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    private GameObject interactingObject;
    public Rigidbody interRB;

    private Vector3 interObjPos;
    private Vector3 rayPos;

    [SerializeField]
    private float _distRange = 5.0f;
    [SerializeField]
    private float _maxSpeed = 60.0f;
    [SerializeField]
    private float _minDist = 1.3f;
    [SerializeField]
    private float _maxDist = 2.0f;

    public bool holdingObject = false;
    bool objectDetection()
    {
        rayPos = new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z);
        RaycastHit ray;
        if (Physics.Raycast(rayPos, transform.TransformDirection(Vector3.forward), out ray, 1f, 1 << 10))
        {
            interactingObject = ray.transform.gameObject;
            interRB = interactingObject.GetComponent<Rigidbody>();
        }

        if (interactingObject == null)
            return false;
        else
            return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && holdingObject || Input.GetButtonDown("Fire3") && holdingObject)
        {
            interactingObject.transform.SetParent(null);
            //interRB.isKinematic = false;
            //interRB.useGravity = true;
            interactingObject = null;
            holdingObject = false;

            Physics.IgnoreLayerCollision(9, 11, false);
            Physics.IgnoreLayerCollision(10, 11, false);
        }
        else if (Input.GetKeyDown(KeyCode.E) && objectDetection() || Input.GetButtonDown("Fire3") && objectDetection())
        {
            interactingObject.transform.SetParent(transform);
            interObjPos = interactingObject.transform.position;
            //interactingObject.transform.position = new Vector3(interObjPos.x, (transform.position.y + 0.1f), interObjPos.z);
            interactingObject.transform.position = interObjPos;
            //interRB.isKinematic = true;
            //interRB.useGravity = false;
            holdingObject = true;

            Physics.IgnoreLayerCollision(9, 11, true);
            Physics.IgnoreLayerCollision(10, 11, true);
        }

        if (holdingObject)
        {
            ////interactingObject.transform.localPosition = new Vector3(0.0f, 0.0f, 2.0f);
            //Rigidbody interRB = interactingObject.GetComponent<Rigidbody>();

            //Vector3 offset = (transform.forward * 2.0f) - interactingObject.transform.localPosition;
            Vector3 offset = new Vector3(0.0f, 0.0f, 2.0f) - interactingObject.transform.localPosition;
            float dist = offset.magnitude;
            //float distScaler = Mathf.Clamp(dist, 0.0f, _distRange) / _distRange;
            //float speed = Mathf.Lerp(0.0f, 60.0f, distScaler);
            //Vector3 force = (offset / dist) * speed;


            ////interRB.AddForceAtPosition(force, _rayHitPos);
            //interRB.AddForce(force);


            ////Debug.DrawLine(transform.position, transform.position + transform.forward * _distRange, Color.red);
            //Debug.DrawLine(transform.position, transform.position + transform.forward * dist, Color.cyan);

            ////if (distScaler > 0.1f)
            ////{
            ////    Debug.Log("Vel: " + interRB.velocity);
            ////    interRB.velocity = Vector3.Lerp(Vector3.zero, interRB.velocity, distScaler / 0.1f);
            ////}

            //if (dist < 0.5f)
            //{
            //    Debug.Log("Dist: " + dist);
            //    interRB.velocity = Vector3.zero;
            //}

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            float objSpeed = 1.0f;

            Debug.Log(dist);
            // Object is too close to the player.
            if (dist < _minDist)
            {
                // Find local velocity.
                Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

                Debug.Log("Stopping velocity" + localVelocity.z);
                // NOTE: Should be stopping the rigidbody. Most likely player's move is called after, thus still adding some force.
                // Solution: Make a pick up state where only W and S are allowed, and W is also toggeled by this if.
                if (localVelocity.z > 0.0f)
                {
                    
                    //localVelocity.z = 0.0f;
                    //localVelocity.z = Mathf.Lerp(0.0f, localVelocity.z, dist / _minDist);
                    //localVelocity.z = Mathf.SmoothStep(0.5f, localVelocity.z, dist / _minDist);

                    if (localVelocity.z == 0.0f)
                        Movement.PushPullState.ForwardMovement = false;
                }
                else
                    Movement.PushPullState.ForwardMovement = true;

                // Change global velocity, based on local velocity.
                rb.velocity = transform.TransformDirection(localVelocity);   
            }
            // Object is too far from the player.
            else if (dist > _maxDist)
            {
                objSpeed = 2.0f;
            }

            interRB.velocity = rb.velocity * objSpeed;
        }
    }
}
