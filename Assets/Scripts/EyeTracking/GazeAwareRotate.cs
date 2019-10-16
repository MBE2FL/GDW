using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

public class GazeAwareRotate : MonoBehaviour
{
    // Start is called before the first frame update
    private GazeAware gazeAware;
    void Start()
    {
        gazeAware = GetComponent<GazeAware>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gazeAware.HasGazeFocus)
        {
            transform.Rotate(transform.forward);
        }
    }
}
