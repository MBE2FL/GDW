using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject player;
    public Vector3 posOffset = new Vector3(6,40,-4);
    public Quaternion rotOffset;
    private float mousePosX = 0.0f;
    private float mousePosY = 0.0f;


    public GameObject Player
    {
        get
        {
            return player;
        }
    }


    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!player)
            return;

        // Move with mouse
        mousePosX += Input.GetAxis("HorizontalC");
        mousePosY += Input.GetAxis("VerticalC");
        if (mousePosY > 45)
            mousePosY = 45;
        else if (mousePosY < -20)
            mousePosY = -20;
        Vector3 rot = new Vector3(mousePosY, mousePosX, 0);
            
        transform.position =(player.transform.position + Quaternion.Euler(rot) * posOffset);


        //// Switch to brother
        //if (Input.GetKeyDown(KeyCode.L) && player.tag == "Sister")
        //{
        //    player.GetComponent<Movement>().enabled = false;
        //    player = brother;
        //    player.GetComponent<Movement>().enabled = true;
        //}
        //// Switch to sister
        //else if (Input.GetKeyDown(KeyCode.L) && player.tag == "Brother")
        //{
        //    player.GetComponent<Movement>().enabled = false;
        //    player = sister;
        //    player.GetComponent<Movement>().enabled = true;
        //}

        // Rotate this camera to face the player.
        transform.LookAt(player.transform.position + new Vector3(0,1.8f,0));
        //transform.position = Vector3.Lerp(transform.position, player.transform.position + Quaternion.Euler(mousePosY, mousePosX, 0) * posOffset, 0.01f);
    }

    public void setPlayer(CharacterChoices characterChoice)
    {
        if (characterChoice == CharacterChoices.SisterChoice)
            player = GameObject.FindGameObjectWithTag("Sister");
        else
            player = GameObject.FindGameObjectWithTag("Brother");
    }

}
