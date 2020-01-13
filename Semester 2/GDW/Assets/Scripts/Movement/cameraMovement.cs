using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject sister;
    public GameObject brother;
    private GameObject player;
    public bool isSister = true;
    public Vector3 posOffset = new Vector3(6,40,-4);
    public Quaternion rotOffset;
    private float mousePosX = 0.0f;
    private float mousePosY = 0.0f;
    private float ControllerPosX = 0.0f;
    private float ControllerPosY = 0.0f;

    private void Start()
    {
        player = sister;
    }
    // Update is called once per frame
    void Update()
    {
        // Move with controller
        if (Input.GetJoystickNames().Length > 0 && Input.GetJoystickNames()[0].Length != 0)
        {
            ControllerPosX += Input.GetAxis("HorizontalC");
            ControllerPosY += Input.GetAxis("VerticalC");
            transform.position = (player.transform.position + Quaternion.Euler(ControllerPosY, ControllerPosX, 0) * posOffset);
        }
        // Move with mouse
        else
        {
            mousePosX += Input.GetAxis("HorizontalC");
            mousePosY += Input.GetAxis("VerticalC");
            transform.position =(player.transform.position + Quaternion.Euler(-mousePosY, mousePosX, 0) * posOffset);
        }

        // Switch to brother
        if (Input.GetKeyDown(KeyCode.L) && player.tag == "Sister")
        {
            player.GetComponent<Movement>().enabled = false;
            player = brother;
            player.GetComponent<Movement>().enabled = true;
        }
        // Switch to sister
        else if (Input.GetKeyDown(KeyCode.L) && player.tag == "Brother")
        {
            player.GetComponent<Movement>().enabled = false;
            player = sister;
            player.GetComponent<Movement>().enabled = true;
        }

        // Rotate this camera to face the player.
        transform.LookAt(player.transform.position + new Vector3(0,1.8f,0));
        //transform.position = Vector3.Lerp(transform.position, player.transform.position + Quaternion.Euler(mousePosY, mousePosX, 0) * posOffset, 0.01f);
    }

    
}
