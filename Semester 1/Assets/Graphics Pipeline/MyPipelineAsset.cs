using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/MyPipeline")]
public class MyPipelineAsset : RenderPipelineAsset
{
    //protected override IRenderPipeline InternalCreatePipeline()
    //{
    //    return null;
    //}
    protected override RenderPipeline CreatePipeline()
    {
        return new MyPipeline();
    }
}
