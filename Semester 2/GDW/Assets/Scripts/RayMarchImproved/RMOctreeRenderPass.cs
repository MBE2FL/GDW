using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class RMOctreeRenderPass : CustomPass
{
    [SerializeField]
    RMComputeRender _shader;
    [SerializeField]
    Transform _sunlight;
    [SerializeField]
    Material _blitMat;
    [SerializeField]
    float _fade = 0.0f;
    [SerializeField]
    RayMarcher _rayMarcher;
    ComputeBuffer _boundsBuf;
    ComputeBuffer _octreeBuf;


    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Setup code here
        _rayMarcher = RayMarcher.Instance;
    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera hdCamera, CullingResults cullingResult)
    {
        // Executed every frame for all the camera inside the pass volume
        _shader.render(cmd, ref _boundsBuf, ref _octreeBuf, hdCamera, _rayMarcher, _sunlight);

        //int kernelIndex = _shader.Shader.FindKernel("CSMain");
        //cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_sceneCol", _rayMarcher.RenderTex, 0, RenderTextureSubElement.Color);
        ////cmd.SetComputeTextureParam(_shader.Shader, kernelIndex, "_depthTex", _rayMarcher.RenderDepthTex, 0, RenderTextureSubElement.Color);
        //cmd.SetComputeVectorParam(_shader.Shader, "_CameraPos", hdCamera.camera.transform.position);
        ////cmd.SetComputeMatrixParam(_shader.Shader, "_cameraInvMatrix", hdCamera.camera.transform.localToWorldMatrix.inverse);
        ////cmd.SetComputeMatrixParam(_shader.Shader, "_camLocalToWorldMatrix", hdCamera.camera.transform.localToWorldMatrix);
        //cmd.SetComputeMatrixParam(_shader.Shader, "_cameraToWorldMatrix", hdCamera.camera.cameraToWorldMatrix);
        ////cmd.SetComputeMatrixParam(_shader.Shader, "_cameraToWorldInvMatrix", hdCamera.camera.cameraToWorldMatrix.inverse);
        //cmd.SetComputeMatrixParam(_shader.Shader, "_cameraInvProj", hdCamera.camera.projectionMatrix.inverse);


        //int Xgroups = Mathf.CeilToInt(hdCamera.actualWidth / 8.0f);
        //int Ygroups = Mathf.CeilToInt(hdCamera.actualHeight / 8.0f);
        //cmd.DispatchCompute(_shader.Shader, kernelIndex, Xgroups, Ygroups, 1);


        _blitMat.SetTexture("_sceneCol", _rayMarcher.RenderTex);
        _blitMat.SetFloat("_fade", _fade);
        CoreUtils.DrawFullScreen(cmd, _blitMat);

        //_boundsBuf.Release();
        //_octreeBuf.Release();
    }

    protected override void Cleanup()
    {
        // Cleanup code
        _boundsBuf.Release();
        _octreeBuf.Release();
    }
}