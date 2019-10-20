using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : MonoBehaviour
{
    private GameObject interactingObject;
    public Rigidbody interRB;

    private Vector3 interObjPos;
    private Vector3 rayPos;

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
            interRB.isKinematic = false;
            //interRB.useGravity = true;
            interactingObject = null;
            holdingObject = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) && objectDetection() || Input.GetButtonDown("Fire3") && objectDetection())
        {
            interactingObject.transform.SetParent(transform);
            interObjPos = interactingObject.transform.position;
            //interactingObject.transform.position = new Vector3(interObjPos.x, (transform.position.y + 0.1f), interObjPos.z);
            interactingObject.transform.position = interObjPos;
            interRB.isKinematic = true;
            //interRB.useGravity = false;
            holdingObject = true;
        }
    }
}
