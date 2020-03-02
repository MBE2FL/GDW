using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minigame: MonoBehaviour
{
    bool completed = false;
    bool toggled = false;
    bool inPlayRange = false;
    int stage = 1;

    public GameObject sisterPiece;
    public GameObject brotherPiece;
    public GameObject hitBar;

    private Animator brotherAnimator;
    private Animator sisterAnimator;

    Vector3 sisterStart = new Vector3(-390f, -244f, 0);
    Vector3 brotherStart = new Vector3(390f, -244f, 0);
    // Start is called before the first frame update
    void Start()
    {
        brotherAnimator = brotherPiece.GetComponent<Animator>();
        sisterAnimator = sisterPiece.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (toggled /*&& Input.GetKeyDown(KeyCode.E)*/)
        {
            //sisterAnimator.SetBool("press stop", true);
            sisterPiece.transform.position = Vector3.Lerp(sisterPiece.transform.position, brotherStart, 0.01f);
            // brotherPiece.transform.position = Vector3.Lerp(brotherPiece.transform.position, sisterStart, 0.01f);
        }

        if (toggled && Input.GetKeyDown(KeyCode.R))
        {
            brotherAnimator.SetBool("press stop", true);
            //sisterPiece.transform.position = Vector3.Lerp(sisterPiece.transform.position, brotherStart, 0.01f);
            // brotherPiece.transform.position = Vector3.Lerp(brotherPiece.transform.position, sisterStart, 0.01f);
        }

        if (inPlayRange && Input.GetKeyDown(KeyCode.E) && !toggled)
        {
            toggled = true;
            //brotherAnimator.SetBool("gameStarted", true);
            //sisterAnimator.SetBool("gameStarted", true);
        }

        if(stage == 4)
        {
            completed = true;
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
