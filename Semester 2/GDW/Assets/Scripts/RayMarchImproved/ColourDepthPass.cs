using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class ColourDepthPass : CustomPass
{
    [SerializeField]
    Material _colourDepthMat;
    //MaterialPropertyBlock customMaterialProperties;

    [SerializeField]
    RayMarcher _rayMarcher;

    //[SerializeField]
    //RTHandle _distTex;


    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Setup code here
        _rayMarcher = RayMarcher.Instance;
        Camera cam = Camera.main;

        //_rayMarcher.RenderTex = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        //_rayMarcher.RenderTex.enableRandomWrite = true;
        //_rayMarcher.RenderTex.useDynamicScale = true;
        //_rayMarcher.RenderTex.Create();

        //_rayMarcher.RenderDepthTex = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        //_rayMarcher.RenderDepthTex.enableRandomWrite = true;
        //_rayMarcher.RenderDepthTex.Create();

        //_rayMarcher.RenderDepthTex = RTHandles.Alloc(Vector2.one, TextureXR.slices, dimension: TextureXR.dimension, colorFormat: GraphicsFormat.R32_SFloat,
        //                                useDynamicScale: true, name: "Main Cam Render Tex", enableRandomWrite: true);

        //_colourDepthMat = CoreUtils.CreateEngineMaterial(Shader.Find("FullScreen/ColourDepthPass"));

        //customMaterialProperties = new MaterialPropertyBlock();
    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
    {
        // Executed every frame for all the camera inside the pass volume
        if (!_rayMarcher.RenderTex || _rayMarcher.RenderTex.width != hdCamera.actualWidth || _rayMarcher.RenderTex.height != hdCamera.actualHeight)
        {
            if (_rayMarcher.RenderTex)
            {
                _rayMarcher.RenderTex.Release();
            }

            _rayMarcher.RenderTex = new RenderTexture(hdCamera.actualWidth, hdCamera.actualHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _rayMarcher.RenderTex.enableRandomWrite = true;
            _rayMarcher.RenderTex.Create();
        }

        if (!_rayMarcher.RenderDepthTex || _rayMarcher.RenderDepthTex.width != hdCamera.actualWidth || _rayMarcher.RenderDepthTex.height != hdCamera.actualHeight)
        {
            if (_rayMarcher.RenderDepthTex)
            {
                _rayMarcher.RenderDepthTex.Release();
            }

            _rayMarcher.RenderDepthTex = new RenderTexture(hdCamera.actualWidth, hdCamera.actualHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            _rayMarcher.RenderDepthTex.enableRandomWrite = true;
            _rayMarcher.RenderDepthTex.Create();
        }

        RenderTargetIdentifier[] renderTargets = new RenderTargetIdentifier[2];
        renderTargets[0] = _rayMarcher.RenderTex;
        renderTargets[1] = _rayMarcher.RenderDepthTex;
        CoreUtils.SetRenderTarget(cmd, renderTargets, _rayMarcher.RenderTex);
        CoreUtils.DrawFullScreen(cmd, _colourDepthMat);


        //if (hdCamera.camera == Camera.main)
        //{
        //    RenderTargetIdentifier[] renderTargets = new RenderTargetIdentifier[2];
        //    renderTargets[0] = _rayMarcher.RenderTex;
        //    renderTargets[1] = _rayMarcher.RenderDepthTex;
        //    CoreUtils.SetRenderTarget(cmd, renderTargets, _rayMarcher.RenderTex);
        //}
        //else
        //{
        //    if (!_rayMarcher.SceneRenderTex)
        //    {
        //        _rayMarcher.SceneRenderTex = new RenderTexture(hdCamera.actualWidth, hdCamera.actualHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        //        _rayMarcher.SceneRenderTex.enableRandomWrite = true;
        //        _rayMarcher.SceneRenderTex.Create();
        //    }
        //    if (!_rayMarcher.SceneDepthTex)
        //    {
        //        _rayMarcher.SceneDepthTex = new RenderTexture(hdCamera.actualWidth, hdCamera.actualHeight, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        //        _rayMarcher.SceneDepthTex.enableRandomWrite = true;
        //        _rayMarcher.SceneDepthTex.Create();
        //    }

        //    RenderTargetIdentifier[] renderTargets = new RenderTargetIdentifier[2];
        //    renderTargets[0] = new RenderTargetIdentifier(_rayMarcher.SceneRenderTex);
        //    renderTargets[1] = new RenderTargetIdentifier(_rayMarcher.SceneDepthTex);
        //    CoreUtils.SetRenderTarget(cmd, renderTargets, _rayMarcher.SceneRenderTex);
        //}

        //CoreUtils.DrawFullScreen(cmd, _colourDepthMat);
    }

    protected override void Cleanup()
    {
        // Cleanup code

        //CoreUtils.Destroy(_colourDepthMat);
    }
}