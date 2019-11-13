using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour, IObserver
{
    [SerializeField]
    List<Puzzle> _puzzles = new List<Puzzle>();
    int _currPuzzles = 0;
    bool _allPuzzlesCompleted = false;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Puzzle puzzle in _puzzles)
        {
            puzzle.addObserver(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void onNotify(GameObject obj, PuzzleEvents puzzleEvent)
    {
        switch (puzzleEvent)
        {
            case PuzzleEvents.ITEM_PLACED:
                break;
            case PuzzleEvents.PUZZLE_FINISHED:
                puzzleFinished(obj);
                break;
            default:
                break;
        }
    }

    void puzzleFinished(GameObject obj)
    {
        ++_currPuzzles;



        checkCompleted();
    }

    void checkCompleted()
    {
        if (_currPuzzles == _puzzles.Count)
        {
            // Activate portal;
            Debug.Log("All puzzles completed!");
            GetComponent<PortalManager>().activatePortals();
        }
    }
}
