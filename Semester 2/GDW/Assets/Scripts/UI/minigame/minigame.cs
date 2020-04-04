using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minigame: MonoBehaviour
{
    bool toggled = false;
    bool inPlayRange = false;
    int stage = 1;

    bool sisterStop = false;
    bool brotherStop = false;

    public bool gameComplete = false;

    float endDelay = 0.75f;

    [SerializeField]
    private GameObject sisterBar;
    [SerializeField]
    private GameObject brotherBar;
    [SerializeField]
    private GameObject hitBar;
    [SerializeField]
    private GameObject slideGame;
    [SerializeField]
    private Camera _camera;

    private Animator brotherAnimator;
    private Animator sisterAnimator;

    // Start is called before the first frame update
    void Start()
    {
        //finds the animators on the bars
        brotherAnimator = brotherBar.GetComponent<Animator>();
        sisterAnimator = sisterBar.GetComponent<Animator>();
    }

    void enableMovement(bool active)
    {
        _camera.GetComponent<cameraMovement>().enabled = active;
        _camera.GetComponent<cameraMovement>().Player.GetComponent<Movement>().enabled = active;
    }
    // Update is called once per frame
    void Update()
    {
        if (toggled)
        {
            //stops the animation of the bar for sister
            if (Input.GetKeyDown(KeyCode.E) && !sisterStop)
            {
                sisterAnimator.enabled = false;
                sisterStop = true;
            }

            //stops the animation of the bar for brother
            if (Input.GetKeyDown(KeyCode.R) && !brotherStop)
            {
                brotherAnimator.enabled = false;
                brotherStop = true;
            }

            if (sisterStop && brotherStop)//when both players have pressed the button to stop there bar
            {
                //counts down end delay
                endDelay -= Time.deltaTime;
                if (endDelay <= 0)
                {
                    //resets the end delay
                    endDelay = 0.75f;

                    //starts animation for sister again
                    sisterAnimator.enabled = true;
                    sisterStop = false;

                    //starts animation for brother again
                    brotherAnimator.enabled = true;
                    brotherStop = false;

                    //calculates how close the players got there bars
                    float offsetS = Vector3.Magnitude(hitBar.transform.localPosition - sisterBar.transform.localPosition);
                    float offsetB = Vector3.Magnitude (hitBar.transform.localPosition - brotherBar.transform.localPosition);

                    // if both players get it in the the green area
                    if(offsetS <=  hitBar.transform.lossyScale.x * 9.8f && offsetB <= hitBar.transform.lossyScale.x * 9.8f)
                    {
                        Debug.Log("ye did it");
                        stage++;
                        hitBar.transform.localScale = new Vector3(hitBar.transform.localScale.x - 0.25f, 0.84f,1);
                    }
                    else if(stage != 1)//if only 1 player or both dont get it in the bar goes bar to previous size(unless 1st stage)
                    {
                        stage--;
                        hitBar.transform.localScale = new Vector3(hitBar.transform.localScale.x + 0.25f, 0.84f, 1);
                    }

                    //checks if players have beat minigame
                    if (stage == 4)
                    {
                        //toggled = false;
                        Debug.Log("ye won");
                        gameComplete = true;
                        //deActivates the slide game
                        slideGame.SetActive(false);

                        //enables the movement of players and camera
                        enableMovement(true);
                    }
                }
            }
        }

        if (inPlayRange && Input.GetKeyDown(KeyCode.E) && !toggled)
        {
            toggled = true;

            //activates the slide game to appear on screen
            slideGame.SetActive(true);

            //starts animations
            brotherAnimator.SetBool("gameStarted", true);
            sisterAnimator.SetBool("gameStarted", true);

            //disables the movement of players and camera
            enableMovement(false);
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
