using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : Subject, IObserver
{
    [SerializeField]
    List<PuzzleItem> _objectives = new List<PuzzleItem>();
    int _currObjectives = 0;
    bool _completed = false;
    [SerializeField]
    GameObject _reward;
    [SerializeField]
    Transform _rewardSpawn;


    // Start is called before the first frame update
    void Start()
    {
        // Load reward
        //_reward = Resources.Load("Puzzles/key Variant") as GameObject;

        // Add all puzzle items
        foreach (PuzzleItem objective in _objectives)
        {
            objective.addObserver(this);
        }
    }

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

    private void itemPlaced(GameObject obj)
    {
        ++_currObjectives;

        // Do some stuff to obj.
        obj.GetComponent<Renderer>().material.SetColor("Color", Color.red);

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        //rb.useGravity = false;
        rb.isKinematic = true;

        Animator animator = obj.GetComponent<Animator>();
        if (animator)
        {
            animator.SetTrigger("Rotate");
        }

        checkCompleted();
    }

    private void checkCompleted()
    {
        if (_currObjectives == _objectives.Count)
        {
            _completed = true;

            //Instantiate(_reward, _rewardSpawn.position, _rewardSpawn.rotation);
            _reward.SetActive(true);

            // Notify puzzle manager
            notify(gameObject, PuzzleEvents.PUZZLE_FINISHED);
        }
    }
}
