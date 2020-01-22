using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleUI : MonoBehaviour
{
    // Start is called before the first frame update
    private bool activeUI;
    private bool showUI = false;
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
        activeUI = animator.GetBool("toggled");
        Debug.Log(activeUI);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && showUI)
        {
            showUI = false;
            animator.SetBool("toggled", false);
        }
        else if(Input.GetKeyDown(KeyCode.Tab) && !showUI)
        {
            showUI = true;
            animator.SetBool("toggled", true);
        }

    }
}
