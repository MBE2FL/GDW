using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class focusing : MonoBehaviour
{
    public Camera camera;
    private Vector3 currentCameraPos;
    private Quaternion currentCameraAngle;
    public Vector3 cameraPosition;
    public Vector3 cameraAngle;
    private Vector3 velocity = Vector3.zero;
    private bool activate = false;

    // Update is called once per frame
    void Update()
    {
        currentCameraPos = camera.transform.position;
        currentCameraAngle = camera.transform.rotation;

        if (activate && Input.GetKey(KeyCode.E))
        {
            camera.GetComponent<cameraMovement>().enabled = false;
            camera.transform.position = Vector3.SmoothDamp(currentCameraPos, cameraPosition, ref velocity, 0.75f);
            camera.transform.rotation = Quaternion.Slerp(currentCameraAngle, Quaternion.Euler(cameraAngle), 0.05f);
        }
        else
            camera.GetComponent<cameraMovement>().enabled = true;
        

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Sister" || collision.gameObject.tag == "Brother")
            activate = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Sister" || collision.gameObject.tag == "Brother")
            activate = false;
    }
}
