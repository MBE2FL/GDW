using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player;
    public Vector3 posOffset = new Vector3(0,2,-4);
    public Quaternion rotOffset;
    private float mousePosX = 0.0f;
    private float mousePosY = 0.0f;
    private float ControllerPosX = 0.0f;
    private float ControllerPosY = 0.0f;
    // Update is called once per frame
    void Update()
    {

        if (Input.GetJoystickNames()[0].Length != 0)
        {
            ControllerPosX += Input.GetAxis("HorizontalC");
            ControllerPosY += Input.GetAxis("VerticalC");
            transform.position = (player.transform.position + Quaternion.Euler(ControllerPosY, ControllerPosX, 0) * posOffset);
        }
        else
        {
            mousePosX += Input.GetAxis("Mouse X");
            mousePosY += Input.GetAxis("Mouse Y");
            transform.position = (player.transform.position + Quaternion.Euler(mousePosY, mousePosX, 0) * posOffset);
        }
        transform.LookAt(player.transform.position);
    }

    
}
