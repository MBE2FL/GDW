using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Subject : MonoBehaviour
{
    List<IObserver> _observers = new List<IObserver>();

    // Start is called before the first frame update
    void Start()
    {

    }

    public void addObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void removeObserver(IObserver observer)
    {
        if (_observers.Count > 0)
        {
            int index = _observers.IndexOf(observer);
            IObserver temp = _observers[_observers.Count - 1];
            _observers[index] = temp;
        }

        _observers.RemoveAt(_observers.Count - 1);
    }

    protected void notify(GameObject obj, PuzzleEvents puzzleEvent)
    {
        foreach (IObserver observer in _observers)
        {
            observer.onNotify(obj, puzzleEvent);
        }
    }

}
