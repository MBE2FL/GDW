using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PortalRenderer : MonoBehaviour
{
    [SerializeField]
    Material _dissolve;
    [SerializeField]
    Camera _portalCam;
    [SerializeField]
    RenderTexture _test;
    [SerializeField]
    Material _cutout;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.SetRenderTarget(_portalCam.targetTexture);

        Graphics.Blit(source, destination, _dissolve);

        //Graphics.Blit(_portalCam.targetTexture, destination, _cutout);
    }

    private void OnEnable()
    {
        RenderPipeline.beginCameraRendering += beginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipeline.beginCameraRendering -= beginCameraRendering;
    }

    void beginCameraRendering(Camera camera)
    {
        Graphics.Blit(camera.targetTexture, null, _dissolve);
    }
}
