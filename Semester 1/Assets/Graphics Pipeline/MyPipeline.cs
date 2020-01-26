using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline
{
    CullingResults cullingResults;
    CommandBuffer buffer = new CommandBuffer
    {
        name = "Render Camera"
    };
    Material errorMaterial;

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //base.Render(context, cameras);

        foreach (var camera in cameras)
            Render(context, camera);
    }

    private void Render(ScriptableRenderContext context, Camera camera)
    {
        /* Attempt to cull the current camera.
         * If it fails to cull, don't render anything to this camera.
         */
        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(out cullingParameters))
            return;

#if UNITY_EDITOR
        // For rendering UI in the scene view
        if (camera.cameraType == CameraType.SceneView)
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
#endif

        cullingResults = context.Cull(ref cullingParameters);
        

        // Setup properties for a specific camera.
        context.SetupCameraProperties(camera);


        /* Creates a graphics command buffer.
         * This particular one is told to clear the depth buffer, not the colour, and to set the clear colour to transparent.
         * It finally releases the memory it has, as the commands have been sent to the internal buffer of the rendering context.
         */
        //var buffer = new CommandBuffer();
        //buffer.ClearRenderTarget(true, false, Color.clear);
        //context.ExecuteCommandBuffer(buffer);
        //buffer.Release();

        /* Creates a graphics command buffer.
         * This particular one is told to clear the depth buffer, not the colour, and to set the clear colour to transparent.
         * It finally releases the memory it has, as the commands have been sent to the internal buffer of the rendering context.
         */
        CameraClearFlags clearFlags = camera.clearFlags;
        buffer.ClearRenderTarget((clearFlags & CameraClearFlags.Depth) != 0, (clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor);
        buffer.BeginSample("Render Camera");    // Start nested grouping for command buffer (For Frame Debugger)
        context.ExecuteCommandBuffer(buffer);
        //buffer.Release();
        buffer.Clear();


        // Once culling and clearing have been completed we can draw the appropriate renderers.
        var drawSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), new SortingSettings(camera));
        var filterSettings = FilteringSettings.defaultValue;
        // Only render the opaque objects first. To avoid transparent objects being drawn behind the skybox.
        filterSettings.renderQueueRange = RenderQueueRange.opaque;
        context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);


        // Check for any problems drawing objects with custom pipeline.
        DrawDefaultPipeline(context, camera);


        // Draws a skybox to the specified camera.
        context.DrawSkybox(camera);


        // After all the opaque objects have been drawn, draw the transparent objects.
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);

        // End nested grouping for command buffer (For Frame Debugger)
        buffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();


        // Commands sent to the rendering context are just buffered. They are only excuted once we submit them.
        context.Submit();
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera)
    {
        // Creates an error material, if one does not already exist. Also hides itself from the project.
        if (errorMaterial == null)
        {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        var drawSettings = new DrawingSettings(new ShaderTagId("ForwardBase"), new SortingSettings(camera));
        drawSettings.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderTagId("Always"));
        drawSettings.SetShaderPassName(3, new ShaderTagId("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderTagId("VertexLM"));
        drawSettings.overrideMaterial = errorMaterial;
        drawSettings.overrideMaterialPassIndex = 0;

        var filterSettings = FilteringSettings.defaultValue;

        context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);
    }
}
