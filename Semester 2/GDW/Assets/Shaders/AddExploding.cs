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
                    Texture tex;
                    if (meshList[i].gameObject.GetComponent<MeshRenderer>().material.GetTexture("Base Texture") != null)
                    {
                        tex = meshList[i].gameObject.GetComponent<MeshRenderer>().material.GetTexture("Base Texture");
                    }
                    else if(meshList[i].gameObject.GetComponent<MeshRenderer>().material.GetTexture("_BaseColorMap") != null)
                    {
                        tex = meshList[i].gameObject.GetComponent<MeshRenderer>().material.GetTexture("_BaseColorMap");
                    }
                    else
                        tex = meshList[i].gameObject.GetComponent<MeshRenderer>().material.GetTexture("Base Texture");

                    //meshList[i].gameObject.GetComponent<MeshRenderer>().material.GetTexturePropertyNameIDs()

                    // meshList[i].gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Shader Graphs/GeoGraph");
                    // meshList[i].gameObject.GetComponent<MeshRenderer>().material.SetTexture("BIG atlas", tex);
                    meshList[i].gameObject.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Geo atlas");
                    meshList[i].gameObject.GetComponent<MeshRenderer>().material.SetTexture("tex", tex);
                }
            }
           
        }
    }

}
