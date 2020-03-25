using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeBuild : MonoBehaviour
{
    bool build = false;
    void Update()
    {
        if(!build)
            GetComponent<ExplodingMesh>().lerp = -0.6f;

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Sister" || other.transform.tag == "Brother")
        {
            build = true;
        }
    }
}
