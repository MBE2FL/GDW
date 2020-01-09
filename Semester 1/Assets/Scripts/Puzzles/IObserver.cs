using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuzzleEvents
{
    ITEM_PLACED,
    PUZZLE_FINISHED
}

public interface IObserver
{
    void onNotify(GameObject obj, PuzzleEvents puzzleEvent);
}
