using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interact : MonoBehaviour
{
    private GameObject interactingObject;
    private Rigidbody interRB;

    private Vector3 interObjPos;
    private Vector3 rayPos;

    private bool holdingObject = false;

    bool objectDetection()
    {
        rayPos = new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z);
        RaycastHit ray;
        if (Physics.Raycast(rayPos, transform.TransformDirection(Vector3.forward), out ray, 1f, 1 << 9))
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
        if (Input.GetKeyDown(KeyCode.E) && holdingObject)
        {
            interactingObject.transform.SetParent(null);
            interRB.isKinematic = false;
            interactingObject = null;
            holdingObject = false;
        }
        else if (Input.GetKeyDown(KeyCode.E) && objectDetection())
        {
            interactingObject.transform.SetParent(transform);
            interObjPos = interactingObject.transform.position;
            interactingObject.GetComponent("rigidbody");
            interactingObject.transform.position = new Vector3(interObjPos.x,(transform.position.y * 1.25f), interObjPos.z);
            interRB.isKinematic = true;
            holdingObject = true;
        }

        if(holdingObject)
            interObjPos = interactingObject.transform.position;
    }
}
