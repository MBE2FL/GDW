using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PuzzleManager : MonoBehaviour, IObserver
{
    private const string DLL_NAME = "EditorPlugin";
    [SerializeField]
    List<Puzzle> _puzzles = new List<Puzzle>();
    int _currPuzzles = 0;
    bool _allPuzzlesCompleted = false;

    [DllImport(DLL_NAME)]
    private static extern int load(string filePath);
    [DllImport(DLL_NAME)]
    private static extern void logCurrPuzzles(string filePath, int currPuzzles);

    int minutes;
    float seconds;

    void LogCurrPuzzles()
    {
        logCurrPuzzles("CurrentPuzzles.txt", _currPuzzles);
    }

    int Load()
    {
        return load("CurrentPuzzles.txt");
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (Puzzle puzzle in _puzzles)
        {
            puzzle.addObserver(this);

            //loads number of puzzles currently completed from the dll
            _currPuzzles = Load();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        float sec = Time.time;
        minutes = (int)sec / 60;
        
        seconds = sec % 60;
        //Debug.Log("Seconds: " + seconds + " Minutes: " + minutes);

        if (_currPuzzles == _puzzles.Count)
        {
            // Activate portal;
            //Debug.Log("All puzzles completed!");
            //GetComponent<PortalManager>().activatePortals();

            //TODO send the time over to the server. 
            //Get the leader board data from the server
            //Display the leaderboard to the players.

        }
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

        LogCurrPuzzles();
        Debug.Log("puzzles completed!");

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
