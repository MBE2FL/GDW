using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;

class RayMarchPass : CustomPass
{
    Material _rayMarchMaterial;
    MaterialPropertyBlock customMaterialProperties;
    //RTHandle colourBuffer;

    [SerializeField]
    RayMarcher _rayMarcher;

    [SerializeField]
    List<RayMarchShader> _shaders = new List<RayMarchShader>();

    [SerializeField]
    RTHandle _distTex;

    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Setup code here
        _rayMarcher = RayMarcher.Instance;

        _rayMarchMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Standard"));

        customMaterialProperties = new MaterialPropertyBlock();

        //colourBuffer = RTHandles.Alloc(Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
        //                                colorFormat: GraphicsFormat.)
        _distTex = RTHandles.Alloc(Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
                                    colorFormat: GraphicsFormat.R32_SFloat);

    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
    {
        // Executed every frame for all the camera inside the pass volume

        //colourBuffer = camera.GetCurrentFrameRT(0);

        // Render all shaders.
        List<RayMarchShader> _shaders = _rayMarcher.Shaders;
        RayMarchShader shader;
        Matrix4x4 cameraInvViewMatrix = camera.camera.cameraToWorldMatrix;
        Vector3 camPos = camera.camera.transform.position;
        Matrix4x4 cameraInvMatrix = camera.camera.transform.localToWorldMatrix.inverse;
        Matrix4x4 cameraMatrix = camera.camera.transform.localToWorldMatrix;
        

        for (int i = 0; i < _shaders.Count; ++i)
        {
            shader = _shaders[i];
            _rayMarchMaterial.shader = shader.EffectShader;

            _rayMarchMaterial.SetMatrix("_cameraInvMatrix", cameraInvMatrix);
            _rayMarchMaterial.SetMatrix("_cameraMatrix", cameraMatrix);

            shader.render(_rayMarchMaterial, cameraInvViewMatrix, camPos, _rayMarcher.SunLight);

            //CustomGraphicsBlit(source, destination, EffectMaterial, 0, i == (_shaders.Count - 1), ref _distTex);
            CoreUtils.DrawFullScreen(cmd, _rayMarchMaterial, customMaterialProperties, 0);

            shader.disableKeywords(_rayMarchMaterial);
        }


        //customMaterialProperties.SetFloat("_FadeValue", 0.0f);

        //HDUtils
        //CoreUtils.DrawFullScreen(cmd, _rayMarchMaterial, customMaterialProperties, 0);
    }

    protected override void Cleanup()
    {
        // Cleanup code
        //colourBuffer.Release();
        CoreUtils.Destroy(_rayMarchMaterial);
        _distTex.Release();
    }
}