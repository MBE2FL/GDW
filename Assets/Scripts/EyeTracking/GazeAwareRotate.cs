using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

public class GazeAwareRotate : MonoBehaviour
{
    // Start is called before the first frame update
    private GazeAware gazeAware;

    Shader normalShader;
    Shader glowShader;
    MeshRenderer meshRenderer;
    void Start()
    {
        gazeAware = GetComponent<GazeAware>();
        normalShader = Shader.Find("HDRP/Lit");
        glowShader = Shader.Find("Shader Graphs/HDR Shader Graph");
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.shader = normalShader;
    }

    // Update is called once per frame
    void Update()
    {
        if(gazeAware.HasGazeFocus)
        {
            //transform.Rotate(transform.forward);
            meshRenderer.material.shader = glowShader;
            return;
        }
        //meshRenderer.material.shader = normalShader;
    }
}
