using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractPopUp : MonoBehaviour
{
    // Start is called before the first frame update
    bool inInteractRange = false;
    public GameObject popUp;
    public Transform camPos;
    private Vector3 popUpPos = new Vector3(0, -100, 0);
    
    void Start()
    {
        popUp.transform.position = popUpPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (inInteractRange)
        {
            popUp.transform.position = transform.position + new Vector3(0, 2, 0);
            popUp.transform.rotation = Quaternion.LookRotation(-camPos.transform.up);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Brother" || collision.transform.tag == "Sister")
        {
            inInteractRange = true;
            popUp.transform.position = transform.position + new Vector3(0, 2, 0);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Brother" || collision.transform.tag == "Sister")
        {
            inInteractRange = false;
            popUp.transform.position = popUpPos;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Brother" || other.transform.tag == "Sister")
        {
            inInteractRange = true;
            popUp.transform.position = transform.position + new Vector3(0, 2, 0);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Brother" || other.transform.tag == "Sister")
        {
            inInteractRange = false;
            popUp.transform.position = popUpPos;
        }
    }
}
