using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RMMarchingCubeShader : RayMarchShader
{
    [SerializeField]
    ComputeShader _sdfToMeshShader;


    public ComputeShader SDFtoMeshShader
    {
        get
        {
            return _sdfToMeshShader;
        }
        set
        {
            _sdfToMeshShader = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void sdfToMesh()
    {

    }
}
