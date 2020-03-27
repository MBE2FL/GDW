using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class memoryPlay : MonoBehaviour
{
    // Start is called before the first frame update
    private bool inPlayRange = false;
    private bool isPlaying = false;

    [SerializeField]
    private GameObject memoryPlayer;
    [SerializeField]
    private Camera _camera;

    void Start()
    {
        memoryPlayer.SetActive(false);
    }

    void enableMovement(bool active)
    {
        _camera.GetComponent<cameraMovement>().enabled = active;
        _camera.GetComponent<cameraMovement>().sister.GetComponent<Movement>().enabled = active;
        _camera.GetComponent<cameraMovement>().brother.GetComponent<Movement>().enabled = active;
    }
    // Update is called once per frame
    void Update()
    {
        if(inPlayRange && !isPlaying && Input.GetKeyDown(KeyCode.E) && GetComponent<minigame>().gameComplete)
        {
            memoryPlayer.SetActive(true);
            isPlaying = true;
            enableMovement(false);
        }
        else if (isPlaying && Input.GetKeyDown(KeyCode.E))
        {
            isPlaying = false;
            memoryPlayer.SetActive(false);
            enableMovement(true);
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
