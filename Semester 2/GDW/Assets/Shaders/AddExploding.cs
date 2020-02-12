using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddExploding : MonoBehaviour
{
    private void Awake()
    {
        var meshList = GameObject.FindObjectsOfType<MeshFilter>();

        for (int i = 0; i < meshList.Length; i++)
        {
            if (meshList[i].gameObject.GetComponent<ExplodingMesh>() == null && meshList[i].sharedMesh.isReadable)
            {
                meshList[i].gameObject.AddComponent(typeof(ExplodingMesh));
                if (meshList[i].gameObject.GetComponent<MeshRenderer>().material != null)
                {
                    Texture tex = meshList[i].gameObject.GetComponent<MeshRenderer>().material.mainTexture;
                    meshList[i].gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Shader Graphs/GeoGraph");
                    meshList[i].gameObject.GetComponent<MeshRenderer>().material.SetTexture("BIG atlas", tex);
                }
            }
           
        }
    }

}
