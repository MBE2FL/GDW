using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleMenu : MonoBehaviour
{
    // Start is called before the first frame update
    private bool showMenu = false;
    private Animator animator;

    [SerializeField]
    private Camera _camera;
    void Start()
    {
        animator = GetComponent<Animator>();
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
        if (Input.GetKeyDown(KeyCode.Escape) && showMenu)
        {
            showMenu = false;
            animator.SetBool("toggled", false);
            enableMovement(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !showMenu)
        {
            showMenu = true;
            animator.SetBool("toggled", true);
            enableMovement(false);
        }

    }
}
