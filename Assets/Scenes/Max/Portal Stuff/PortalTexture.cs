using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTexture : MonoBehaviour
{
    [SerializeField]
    private Camera _camera2;
    [SerializeField]
    private Material _portal2Mat;

    [SerializeField]
    private Camera _camera1;
    [SerializeField]
    private Material _portal1Mat;

    // Start is called before the first frame update
    void Start()
    {
        if (_camera2.targetTexture)
            _camera2.targetTexture.Release();

        // TO-DO Make this dynamic. Will only work on initial game start.
        _camera2.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _portal2Mat.mainTexture = _camera2.targetTexture;



        if (_camera1.targetTexture)
            _camera1.targetTexture.Release();

        // TO-DO Make this dynamic. Will only work on initial game start.
        _camera1.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
        _portal1Mat.mainTexture = _camera1.targetTexture;
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
