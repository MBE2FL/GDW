using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private GameObject interactingObject;
    private Rigidbody interRB;

    private Vector3 interObjPos;// a vector that is used to hold the position of the current interacting object(for ease of code)
    private Vector3 rayPos;

    private bool holdingObject = false;

    bool objectDetection()
    {
        rayPos = new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z);
        RaycastHit ray;
        
        if (Physics.Raycast(rayPos, transform.forward, out ray, 1f, 1 << 9))
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
        Debug.DrawRay(transform.position, transform.forward * 100.0f, Color.white);

        if (Input.GetKeyDown(KeyCode.E) && holdingObject || Input.GetButtonDown("Fire3") && holdingObject)
        {
            interactingObject.transform.SetParent(null);
            interRB.isKinematic = false;
            interactingObject = null;
            holdingObject = false;
            Physics.IgnoreLayerCollision(9, 11, false);
        }
        else if (Input.GetKeyDown(KeyCode.E) && objectDetection() || Input.GetButtonDown("Fire3") && objectDetection())
        {
            interactingObject.transform.SetParent(transform);
            interObjPos = interactingObject.transform.position;
            interactingObject.transform.position = new Vector3(interObjPos.x,(transform.position.y), transform.position.z + 0.6f);
            interRB.isKinematic = true;
            holdingObject = true;
            Physics.IgnoreLayerCollision(9, 11, true);
        }
    }
}
