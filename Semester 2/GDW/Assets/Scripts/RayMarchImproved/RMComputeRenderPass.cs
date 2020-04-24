using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class RMComputeRenderPass : CustomPass
{
    [SerializeField]
    RMComputeRender _shader;
    [SerializeField]
    RenderTexture _renderTex;
    [SerializeField]
    Transform _sunLight;
    Camera _cam;
    [SerializeField]
    Material _blitMat;
    [SerializeField]
    float _fade = 0.0f;

    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Setup code here
        //_renderTex = RTHandles.Alloc(Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
        //                            colorFormat: GraphicsFormat.R8G8B8A8_UNorm);

        _blitMat = CoreUtils.CreateEngineMaterial(Shader.Find("FullScreen/RMBlit"));

        _cam = Camera.main;
        //if (!_renderTex)
        //{
        //    _renderTex = new RenderTexture(_cam.pixelWidth, 397, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        //    _renderTex.enableRandomWrite = true;
        //    _renderTex.Create();
        //}

        _renderTex = new RenderTexture(_cam.pixelWidth, 397, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        _renderTex.enableRandomWrite = true;
        _renderTex.Create();
    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
    {
        // Executed every frame for all the camera inside the pass volume

        //cmd.SetComputeTextureParam(_shader.Shader, 0, "_sceneCol", )

        //if (hdCamera.camera == Camera.main)
        //    return;

        _shader.render(hdCamera.camera.transform.localToWorldMatrix, hdCamera.camera.transform.position, _sunLight);

        int kernelIndex = _shader.Shader.FindKernel("CSMain");
        _shader.Shader.SetTexture(kernelIndex, "_sceneCol", _renderTex, 0, RenderTextureSubElement.Color);
        _shader.Shader.SetMatrix("_cameraInvMatrix", hdCamera.camera.transform.localToWorldMatrix.inverse);
        _shader.Shader.SetMatrix("_cameraToWorldMatrix", hdCamera.camera.cameraToWorldMatrix);
        _shader.Shader.SetMatrix("_cameraInvProj", hdCamera.camera.projectionMatrix.inverse);


        int Xgroups = Mathf.CeilToInt(_cam.pixelWidth / 8.0f);
        int Ygroups = Mathf.CeilToInt(_cam.pixelHeight / 8.0f);
        _shader.Shader.Dispatch(kernelIndex, Xgroups, Ygroups, 1);


        _blitMat.SetTexture("_sceneCol", _renderTex, RenderTextureSubElement.Color);
        _blitMat.SetFloat("_fade", _fade);
        CoreUtils.DrawFullScreen(cmd, _blitMat);

    }

    protected override void Cleanup()
    {
        // Cleanup code
        CoreUtils.Destroy(_blitMat);
        _renderTex.Release();
    }
}