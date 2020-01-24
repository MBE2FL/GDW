using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeController : MonoBehaviour, IObserver
{
    [SerializeField]
    List<PuzzleItem> _objectives = new List<PuzzleItem>();
    [SerializeField]
    List<Animator> _animators = new List<Animator>();
    int _currObjectives = 0;
    bool _completed = false;

    public bool turnOn = false;

    public void onNotify(GameObject obj, PuzzleEvents puzzleEvent)
    {
        switch (puzzleEvent)
        {
            case PuzzleEvents.ITEM_PLACED:
                itemPlaced(obj);
                break;
            case PuzzleEvents.PUZZLE_FINISHED:
                break;
            default:
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Add all puzzle items
        foreach (PuzzleItem objective in _objectives)
        {
            objective.addObserver(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (turnOn)
        {
            foreach(Animator animator in _animators)
            {
                animator.SetTrigger("Merge");
            }

            turnOn = false;
        }
    }

    private void itemPlaced(GameObject obj)
    {
        ++_currObjectives;

        // Do some stuff to obj.
        //obj.GetComponent<Renderer>().material.SetColor("Color", Color.red);

        Animator animator = obj.GetComponent<Animator>();
        if (animator)
        {
            animator.enabled = true;
            _animators.Add(animator);
        }

        checkCompleted();
    }

    private void checkCompleted()
    {
        if (_currObjectives == _objectives.Count)
        {
            foreach (Animator animator in _animators)
            {
                animator.SetTrigger("Merge");
            }

            _completed = true;
        }
    }
}
