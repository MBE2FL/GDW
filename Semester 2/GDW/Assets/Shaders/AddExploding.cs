using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddExploding : MonoBehaviour
{
    private void Awake()
    {
        var meshList = GameObject.FindObjectsOfType<MeshRenderer>(); 

        for(int i = 0; i < meshList.Length; i++)
        {
            //ExplodingMesh explodingMesh = meshList[i].gameObject.AddComponent(typeof(ExplodingMesh)) as ExplodingMesh;
           
        }
    }

}
