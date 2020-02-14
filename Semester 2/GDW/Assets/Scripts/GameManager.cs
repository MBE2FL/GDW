using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject _sister;
    [SerializeField]
    GameObject _sisterPawn;
    [SerializeField]
    GameObject _sisterPP;
    [SerializeField]
    GameObject _brother;
    [SerializeField]
    GameObject _brotherPawn;
    [SerializeField]
    GameObject _brotherPP;

    [SerializeField]
    bool _levelInProgress = false;

    public bool LevelInProgress
    {
        get
        {
            return _levelInProgress;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        // TO-DO auto find brother and sister.
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playAsSister()
    {
        Instantiate(_sister);
        Instantiate(_brotherPawn);
        _sisterPP.SetActive(true);
    }

    public void playAsBrother()
    {
        Instantiate(_sisterPawn);
        Instantiate(_brother);
        _brotherPP.SetActive(true);
    }
}
