using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class RMRenderPass : CustomPass
{
    [SerializeField]
    RMComputeRender _shader;
    [SerializeField]
    Transform _sunLight;
    Camera _cam;
    [SerializeField]
    Material _blitMat;
    [SerializeField]
    float _fade = 0.0f;
    [SerializeField]
    RayMarcher _rayMarcher;

    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Setup code here
        //_renderTex = RTHandles.Alloc(Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
        //                            colorFormat: GraphicsFormat.R8G8B8A8_UNorm);

        //_blitMat = CoreUtils.CreateEngineMaterial(Shader.Find("FullScreen/RMBlit"));

        _cam = Camera.main;
        _rayMarcher = RayMarcher.Instance;

        //_renderTex = new RenderTexture(_cam.pixelWidth, _cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        //_renderTex.enableRandomWrite = true;
        //_renderTex.Create();
    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
    {
        // Executed every frame for all the camera inside the pass volume
        _shader.render(cmd, hdCamera.camera.transform.localToWorldMatrix, hdCamera.camera.transform.position, _sunLight);

        int kernelIndex = _shader.Shader.FindKernel("CSMain");
        //_shader.Shader.SetTexture(kernelIndex, "_sceneCol", _renderTex, 0, RenderTextureSubElement.Color);
        //cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_sceneCol", _rayMarcher.RenderTex, 0, RenderTextureSubElement.Color);
        cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_sceneCol", _rayMarcher.RenderTex, 0, RenderTextureSubElement.Color);
        cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_depthTex", _rayMarcher.RenderDepthTex, 0, RenderTextureSubElement.Color);
        cmd.SetComputeMatrixParam(_shader.Shader, "_cameraInvMatrix", hdCamera.camera.transform.localToWorldMatrix.inverse);
        cmd.SetComputeMatrixParam(_shader.Shader, "_camLocalToWorldMatrix", hdCamera.camera.transform.localToWorldMatrix);
        cmd.SetComputeMatrixParam(_shader.Shader, "_cameraToWorldMatrix", hdCamera.camera.cameraToWorldMatrix);
        cmd.SetComputeMatrixParam(_shader.Shader, "_cameraToWorldInvMatrix", hdCamera.camera.cameraToWorldMatrix.inverse);
        cmd.SetComputeMatrixParam(_shader.Shader, "_cameraInvProj", hdCamera.camera.projectionMatrix.inverse);



        int Xgroups = Mathf.CeilToInt(hdCamera.actualWidth / 8.0f);
        int Ygroups = Mathf.CeilToInt(hdCamera.actualHeight / 8.0f);
        cmd.DispatchCompute(_shader.Shader, kernelIndex, Xgroups, Ygroups, 1);


        _blitMat.SetTexture("_sceneCol", _rayMarcher.RenderTex);
        _blitMat.SetFloat("_fade", _fade);
        CoreUtils.DrawFullScreen(cmd, _blitMat);



        //if (hdCamera.camera == Camera.main)
        //{
        //    _shader.render(cmd, hdCamera.camera.transform.localToWorldMatrix, hdCamera.camera.transform.position, _sunLight);

        //    int kernelIndex = _shader.Shader.FindKernel("CSMain");
        //    //_shader.Shader.SetTexture(kernelIndex, "_sceneCol", _renderTex, 0, RenderTextureSubElement.Color);
        //    //cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_sceneCol", _rayMarcher.RenderTex, 0, RenderTextureSubElement.Color);
        //    cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_sceneCol", _rayMarcher.RenderTex);
        //    cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_depthTex", _rayMarcher.RenderDepthTex, 0, RenderTextureSubElement.Color);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_cameraInvMatrix", hdCamera.camera.transform.localToWorldMatrix.inverse);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_camLocalToWorldMatrix", hdCamera.camera.transform.localToWorldMatrix);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_cameraToWorldMatrix", hdCamera.camera.cameraToWorldMatrix);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_cameraToWorldInvMatrix", hdCamera.camera.cameraToWorldMatrix.inverse);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_cameraInvProj", hdCamera.camera.projectionMatrix.inverse);



        //    int Xgroups = Mathf.CeilToInt(_cam.pixelWidth / 8.0f);
        //    int Ygroups = Mathf.CeilToInt(_cam.pixelHeight / 8.0f);
        //    //_shader.Shader.Dispatch(kernelIndex, Xgroups, Ygroups, 1);
        //    cmd.DispatchCompute(_shader.Shader, kernelIndex, Xgroups, Ygroups, 1);


        //    //_blitMat.SetTexture("_sceneCol", _rayMarcher.RenderTex, RenderTextureSubElement.Color);
        //    _blitMat.SetTexture("_sceneCol", _rayMarcher.RenderTex);
        //    _blitMat.SetFloat("_fade", _fade);
        //    CoreUtils.DrawFullScreen(cmd, _blitMat);
        //}
        //else
        //{
        //    //if (!_sceneRenderTex)
        //    //{
        //    //    _sceneRenderTex = new RenderTexture(hdCamera.actualWidth, hdCamera.actualHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        //    //    _sceneRenderTex.enableRandomWrite = true;
        //    //    _sceneRenderTex.Create();
        //    //}

        //    _shader.render(cmd, hdCamera.camera.transform.localToWorldMatrix, hdCamera.camera.transform.position, _sunLight);

        //    int kernelIndex = _shader.Shader.FindKernel("CSMain");
        //    cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_sceneCol", _rayMarcher.SceneRenderTex, 0, RenderTextureSubElement.Color);
        //    cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_depthTex", _rayMarcher.SceneDepthTex, 0, RenderTextureSubElement.Color);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_cameraInvMatrix", hdCamera.camera.transform.localToWorldMatrix.inverse);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_camLocalToWorldMatrix", hdCamera.camera.transform.localToWorldMatrix);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_cameraToWorldMatrix", hdCamera.camera.cameraToWorldMatrix);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_cameraToWorldInvMatrix", hdCamera.camera.cameraToWorldMatrix.inverse);
        //    cmd.SetComputeMatrixParam(_shader.Shader, "_cameraInvProj", hdCamera.camera.projectionMatrix.inverse);


        //    int Xgroups = Mathf.CeilToInt(_cam.pixelWidth / 8.0f);
        //    int Ygroups = Mathf.CeilToInt(_cam.pixelHeight / 8.0f);
        //    cmd.DispatchCompute(_shader.Shader, kernelIndex, Xgroups, Ygroups, 1);


        //    _blitMat.SetTexture("_sceneCol", _rayMarcher.SceneRenderTex, RenderTextureSubElement.Color);
        //    _blitMat.SetFloat("_fade", _fade);
        //    CoreUtils.DrawFullScreen(cmd, _blitMat);
        //}

    }

    protected override void Cleanup()
    {
        // Cleanup code
        //CoreUtils.Destroy(_blitMat);
        //_rayMarcher.RenderTex.Release();
        //_rayMarcher.RenderDepthTex.Release();
        //_rayMarcher.SceneRenderTex.Release();
        //_rayMarcher.SceneRenderTex = null;
        //_rayMarcher.SceneDepthTex.Release();
        //_rayMarcher.SceneDepthTex = null;
    }
}