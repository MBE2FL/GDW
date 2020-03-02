using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class memoryPlay : MonoBehaviour
{
    // Start is called before the first frame update
    private bool inPlayRange = false;
    private bool isPlaying = false;
    public GameObject memoryPlayer;
    void Start()
    {
        memoryPlayer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(inPlayRange && !isPlaying && Input.GetKeyDown(KeyCode.E))
        {
            memoryPlayer.SetActive(true);
            isPlaying = true;
        }
        else if (isPlaying && Input.GetKeyDown(KeyCode.E))
        {
            isPlaying = false;
            memoryPlayer.SetActive(false);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Brother" || collision.transform.tag == "Sister")
        {
            inPlayRange = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Brother" || collision.transform.tag == "Sister")
        {
            inPlayRange = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Brother" || other.transform.tag == "Sister")
        {
            inPlayRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Brother" || other.transform.tag == "Sister")
        {
            inPlayRange = false;
        }
    }
}
