using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class focusing : MonoBehaviour
{
    public Camera camera;
    public Vector3 cameraPosition;
    public Vector3 cameraAngle;
    private bool activate = false;

    // Update is called once per frame
    void Update()
    {
        if(activate && Input.GetKey(KeyCode.E))
        {
            camera.GetComponent<cameraMovement>().enabled = false;
            camera.transform.position = cameraPosition;
            camera.transform.rotation = Quaternion.Euler(cameraAngle);
        }
        else
            camera.GetComponent<cameraMovement>().enabled = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Sister")
            activate = true;
        else
            activate = false;
    }
}
