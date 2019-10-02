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
    // Update is called once per frame
    void Update()
    {
        mousePosX += Input.GetAxis("Mouse X");
        mousePosY += Input.GetAxis("Mouse Y");

        transform.position = (player.transform.position + Quaternion.Euler(mousePosY, mousePosX, 0) * posOffset); 
        transform.LookAt(player.transform.position);
        //transform.rotation = player.transform.rotation;
    }
}
