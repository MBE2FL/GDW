using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleItem : Subject
{
    bool _completed = false;

    void Start()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.transform.tag == "DropZone" && !_completed))
        {
            _completed = true;
            notify(gameObject, PuzzleEvents.ITEM_PLACED);
        }
    }
}
