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
        rayPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
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

    Vector3 keepScale(Transform denominator)
    {
        Vector3 temp = new Vector3(1, 1, 1);
        temp.x /= denominator.localScale.x;
        temp.y /= denominator.localScale.y;
        temp.z /= denominator.localScale.z;
        return temp;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 1.0f, Color.white);

        if (Input.GetKeyDown(KeyCode.E) && holdingObject || /*Input.GetButtonDown("Fire3") && holdingObject ||*/ Input.GetKeyDown(KeyCode.R) && holdingObject)
        {
            interactingObject.transform.SetParent(null);
            interRB.isKinematic = false;
            if (Input.GetKeyDown(KeyCode.R))
                interRB.AddForce(transform.forward * 500);
            interactingObject = null;
            holdingObject = false;
            Physics.IgnoreLayerCollision(9, 11, false);
        }
        else if (Input.GetKeyDown(KeyCode.E) && objectDetection() /*|| Input.GetButtonDown("Fire3") && objectDetection()*/)
        {
            interactingObject.transform.SetParent(transform, true);
           // interactingObject.transform.localScale = keepScale(transform);
            interObjPos = interactingObject.transform.position;
            //interactingObject.transform.position = new Vector3(interObjPos.x,transform.position.y, transform.position.z + 0.6f);

            interactingObject.transform.localPosition = new Vector3(0.0f, 4.0f, 4.0f);

            //Vector3 relativePos = transform.TransformPoint(new Vector3(0.0f, 0.0f, 1.0f));
            //interactingObject.transform.position = relativePos;

            interRB.isKinematic = true;
            holdingObject = true;
            Physics.IgnoreLayerCollision(9, 11, true);
        }
    }
}
