using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteInEditMode]
public class RMComputeRender : RayMarchShader
{
    [SerializeField]
    ComputeShader _computeShader;
    [SerializeField]
    OctreeMethod _octreeMethod = OctreeMethod.CPU;
    [SerializeField]
    ComputeShader _octreeGenShader;


    public Vector4[] _colours = new Vector4[32];                // The _colours of every object.
    public Vector4[] _reflInfo = new Vector4[32];          // The reflection info for every primitive object.
    public Vector4[] _refractInfo = new Vector4[32];       // The refractiin info for every primitive object.


    private void Awake()
    {
        _shaderType = ShaderType.Rendering;
    }


    public ComputeShader Shader
    {
        get
        {
            return _computeShader;
        }
        set
        {
            _computeShader = value;
        }
    }

    public OctreeMethod OctreeMethod
    {
        get
        {
            return _octreeMethod;
        }
        set
        {
            _octreeMethod = value;
        }
    }

    public ComputeShader OctreeGenShader
    {
        get
        {
            return _octreeGenShader;
        }
        set
        {
            _octreeGenShader = value;
        }
    }



    public override void AddToRenderList(RMObj rmObj)
    {
        base.AddToRenderList(rmObj);

        int primIndex = 0;
        int csgIndex = 0;
        int altIndex = 0;
        RMPrimitive prim;
        CSG csg;
        RMObj obj;

        for (int i = 0; i < _renderList.Count; ++i)
        {
            obj = _renderList[i];

            // Primitive
            if (obj.IsPrim)
            {
                prim = obj as RMPrimitive;

                // Skip any primitives belonging to a csg, as they will be rendered recursively by thier respective csgs.
                if (prim.CSGNode)
                    continue;

                renderPrimitive(prim, ref primIndex, ref altIndex);
            }
            // CSG
            else
            {
                csg = obj as CSG;

                // Skip any non-root CSGs, as they will be rendered recursively by thier parents.
                // Skip any CSGs which don't have two nodes.
                if (!csg.IsRoot || !csg.IsValid)
                    continue;

                renderCSG(csg, ref primIndex, ref csgIndex, ref altIndex);
            }
        }
    }

    public void render(Matrix4x4 cameraInvViewMatrix, Vector3 camPos, Transform sunLight)
    {
        // Enable this shader's defines
        //foreach (ShaderKeywords keyword in _settings.Keywords)
        //{
        //    _computeShader.EnableKeyword(keyword.ToString());
        //}
        

        _computeShader.SetMatrix("_CameraInvViewMatrix", cameraInvViewMatrix);
        _computeShader.SetVector("_CameraPos", camPos);
        _computeShader.SetTexture(0, "_performanceRamp", _settings.PerformanceRamp);

        _computeShader.SetFloat("_specularExp", _settings.SpecularExp);
        _computeShader.SetFloat("_attenuationConstant", _settings.AttenuationConstant);
        _computeShader.SetFloat("_attenuationLinear", _settings.AttenuationLinear);
        _computeShader.SetFloat("_attenuationQuadratic", _settings.AttenuationQuadratic);
        _computeShader.SetVector("_LightDir", sunLight ? sunLight.forward : Vector3.down);
        _computeShader.SetVector("_lightPos", sunLight ? sunLight.position : Vector3.zero);
        _computeShader.SetVector("_ambientColour", _settings.AmbientColour);
        _computeShader.SetVector("_diffuseColour", _settings.DiffuseColour);
        _computeShader.SetVector("_specularColour", _settings.SpecualarColour);
        _computeShader.SetVector("_lightConstants", _settings.LightConstants);
        _computeShader.SetVector("_rimLightColour", _settings.RimLightColour);
        _computeShader.SetFloat("_totalTime", Time.time);

        // ######### Shadow Variables #########
        _computeShader.SetFloat("_penumbraFactor", _settings.PenumbraFactor);
        _computeShader.SetFloat("_shadowMinDist", _settings.ShadowmMinDist);
        _computeShader.SetFloat("_shadowIntensity", _settings.ShadowIntensity);

        // ######### Ray March Variables #########
        _computeShader.SetInt("_maxSteps", _settings.MaxSteps);
        _computeShader.SetFloat("_maxDrawDist", _settings.MaxDrawDist);

        // ######### Reflection Variables #########
        _computeShader.SetInt("_reflectionCount", _settings.ReflectionCount);
        _computeShader.SetFloat("_reflectionIntensity", _settings.ReflectionIntensity);
        _computeShader.SetFloat("_envReflIntensity", _settings.EnvReflIntensity);
        //_computeShader.SetTexture(0, "_skybox", _settings.SkyBox);

        // ######### Refraction Variables #########
        _computeShader.SetVectorArray("_refractInfo", _refractInfo);

        // ######### Ambient Occlusion Variables #########
        _computeShader.SetInt("_aoMaxSteps", _settings.AOMaxSteps);
        _computeShader.SetFloat("_aoStepSize", _settings.AOStepSize);
        _computeShader.SetFloat("_aoIntensity", _settings.AOItensity);

        // ######### Fog Variables #########
        _computeShader.SetFloat("_fogExtinction", _settings.FogExtinction);
        _computeShader.SetFloat("_fogInscattering", _settings.FogInscattering);
        _computeShader.SetVector("_fogColour", _settings.FogColour);

        // ######### Vignette Variables #########
        _computeShader.SetFloat("_vignetteIntensity", _settings.VignetteIntesnity);


        int primIndex = 0;
        int csgIndex = 0;
        int altIndex = 0;


        //if (!Application.isPlaying)
        //{
        //    objects = FindObjectsOfType<RMObj>();
        //    _renderList = new List<RMObj>(objects);

        //    _renderList.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        //}

        RMPrimitive prim;
        CSG csg;
        RMObj obj;

        for (int i = 0; i < _renderList.Count; ++i)
        {
            obj = _renderList[i];

            // Primitive
            if (obj.IsPrim)
            {
                prim = obj as RMPrimitive;

                // Skip any primitives belonging to a csg, as they will be rendered recursively by thier respective csgs.
                if (prim.CSGNode)
                    continue;

                renderPrimitive(prim, ref primIndex, ref altIndex);
            }
            // CSG
            else
            {
                csg = obj as CSG;

                // Skip any non-root CSGs, as they will be rendered recursively by thier parents.
                // Skip any CSGs which don't have two nodes.
                if (!csg.IsRoot || !csg.IsValid)
                    continue;

                renderCSG(csg, ref primIndex, ref csgIndex, ref altIndex);
            }
        }

        if (primIndex > 0)
        {
            _computeShader.SetMatrixArray("_invModelMats", _invModelMats);
            _computeShader.SetFloats("_scaleBuffer", _scaleBuffer);
            _computeShader.SetVectorArray("_rm_colours", _colours);
            _computeShader.SetVectorArray("_combineOps", _combineOps);
            _computeShader.SetVectorArray("_primitiveGeoInfo", _primitiveGeoInfo);
            _computeShader.SetVectorArray("_reflInfo", _reflInfo);
            _computeShader.SetVectorArray("_altInfo", _altInfo);

            _computeShader.SetVectorArray("_bufferedCSGs", _bufferedCSGs);
            _computeShader.SetVectorArray("_combineOpsCSGs", _combineOpsCSGs);

            _computeShader.SetVectorArray("_boundGeoInfo", _boundGeoInfo);
        }
    }

    public void render(CommandBuffer cmd, Matrix4x4 cameraInvViewMatrix, Vector3 camPos, Transform sunLight)
    {
        // Enable this shader's defines
        //foreach (ShaderKeywords keyword in _settings.Keywords)
        //{
        //    _computeShader.EnableKeyword(keyword.ToString());
        //}


        cmd.SetComputeMatrixParam(_computeShader, "_CameraInvViewMatrix", cameraInvViewMatrix);
        cmd.SetComputeVectorParam(_computeShader, "_CameraPos", camPos);
        cmd.SetComputeTextureParam(_computeShader , 0, "_performanceRamp", _settings.PerformanceRamp);

        cmd.SetComputeFloatParam(_computeShader, "_specularExp", _settings.SpecularExp);
        cmd.SetComputeFloatParam(_computeShader, "_attenuationConstant", _settings.AttenuationConstant);
        cmd.SetComputeFloatParam(_computeShader, "_attenuationConstant", _settings.AttenuationConstant);
        cmd.SetComputeFloatParam(_computeShader, "_attenuationLinear", _settings.AttenuationLinear);
        cmd.SetComputeFloatParam(_computeShader, "_attenuationQuadratic", _settings.AttenuationQuadratic);
        cmd.SetComputeVectorParam(_computeShader, "_LightDir", sunLight ? sunLight.forward : Vector3.down);
        cmd.SetComputeVectorParam(_computeShader, "_lightPos", sunLight ? sunLight.position : Vector3.zero);
        cmd.SetComputeVectorParam(_computeShader, "_ambientColour", _settings.AmbientColour);
        cmd.SetComputeVectorParam(_computeShader, "_diffuseColour", _settings.DiffuseColour);
        cmd.SetComputeVectorParam(_computeShader, "_specularColour", _settings.SpecualarColour);
        cmd.SetComputeVectorParam(_computeShader, "_lightConstants", _settings.LightConstants);
        cmd.SetComputeVectorParam(_computeShader, "_rimLightColour", _settings.RimLightColour);
        cmd.SetComputeFloatParam(_computeShader, "_totalTime", Time.time);

        // ######### Shadow Variables #########
        cmd.SetComputeFloatParam(_computeShader, "_penumbraFactor", _settings.PenumbraFactor);
        cmd.SetComputeFloatParam(_computeShader, "_shadowMinDist", _settings.ShadowmMinDist);
        cmd.SetComputeFloatParam(_computeShader, "_shadowIntensity", _settings.ShadowIntensity);

        // ######### Ray March Variables #########
        cmd.SetComputeIntParam(_computeShader, "_maxSteps", _settings.MaxSteps);
        cmd.SetComputeFloatParam(_computeShader, "_maxDrawDist", _settings.MaxDrawDist);

        // ######### Reflection Variables #########
        cmd.SetComputeIntParam(_computeShader, "_reflectionCount", _settings.ReflectionCount);
        cmd.SetComputeFloatParam(_computeShader, "_reflectionIntensity", _settings.ReflectionIntensity);
        cmd.SetComputeFloatParam(_computeShader, "_envReflIntensity", _settings.EnvReflIntensity);
        //_computeShader.SetTexture(0, "_skybox", _settings.SkyBox);

        // ######### Refraction Variables #########
        cmd.SetComputeVectorArrayParam(_computeShader, "_refractInfo", _refractInfo);

        // ######### Ambient Occlusion Variables #########
        cmd.SetComputeIntParam(_computeShader, "_aoMaxSteps", _settings.AOMaxSteps);
        cmd.SetComputeFloatParam(_computeShader, "_aoStepSize", _settings.AOStepSize);
        cmd.SetComputeFloatParam(_computeShader, "_aoIntensity", _settings.AOItensity);

        // ######### Fog Variables #########
        cmd.SetComputeFloatParam(_computeShader, "_fogExtinction", _settings.FogExtinction);
        cmd.SetComputeFloatParam(_computeShader, "_fogInscattering", _settings.FogInscattering);
        cmd.SetComputeVectorParam(_computeShader, "_fogColour", _settings.FogColour);

        // ######### Vignette Variables #########
        cmd.SetComputeFloatParam(_computeShader, "_vignetteIntensity", _settings.VignetteIntesnity);


        int primIndex = 0;
        int csgIndex = 0;
        int altIndex = 0;


        //if (!Application.isPlaying)
        //{
        //    objects = FindObjectsOfType<RMObj>();
        //    _renderList = new List<RMObj>(objects);

        //    _renderList.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        //}

        RMPrimitive prim;
        CSG csg;
        RMObj obj;

        for (int i = 0; i < _renderList.Count; ++i)
        {
            obj = _renderList[i];

            // Primitive
            if (obj.IsPrim)
            {
                prim = obj as RMPrimitive;

                // Skip any primitives belonging to a csg, as they will be rendered recursively by thier respective csgs.
                if (prim.CSGNode)
                    continue;

                renderPrimitive(prim, ref primIndex, ref altIndex);
            }
            // CSG
            else
            {
                csg = obj as CSG;

                // Skip any non-root CSGs, as they will be rendered recursively by thier parents.
                // Skip any CSGs which don't have two nodes.
                if (!csg.IsRoot || !csg.IsValid)
                    continue;

                renderCSG(csg, ref primIndex, ref csgIndex, ref altIndex);
            }
        }

        if (primIndex > 0)
        {
            cmd.SetComputeMatrixArrayParam(_computeShader, "_invModelMats", _invModelMats);
            cmd.SetComputeFloatParams(_computeShader, "_scaleBuffer", _scaleBuffer);
            cmd.SetComputeVectorArrayParam(_computeShader, "_rm_colours", _colours);
            cmd.SetComputeVectorArrayParam(_computeShader, "_combineOps", _combineOps);
            cmd.SetComputeVectorArrayParam(_computeShader, "_primitiveGeoInfo", _primitiveGeoInfo);
            cmd.SetComputeVectorArrayParam(_computeShader, "_reflInfo", _reflInfo);
            cmd.SetComputeVectorArrayParam(_computeShader, "_altInfo", _altInfo);

            cmd.SetComputeVectorArrayParam(_computeShader, "_bufferedCSGs", _bufferedCSGs);
            cmd.SetComputeVectorArrayParam(_computeShader, "_combineOpsCSGs", _combineOpsCSGs);

            cmd.SetComputeVectorArrayParam(_computeShader, "_boundGeoInfo", _boundGeoInfo);
        }
    }

    public void render(CommandBuffer cmd, ref ComputeBuffer boundsBuf, ref ComputeBuffer octreeBuf, HDCamera hdCamera, RayMarcher rayMarcher, Transform sunlight)
    {
        Bounds[] bounds = new Bounds[1];
        List<Node> octree;
        int kernelIndex = _computeShader.FindKernel("CSMain");

        foreach (RMObj obj in _renderList)
        {
            bounds[0] = obj.OctreeBounds;
            octree = obj.Octree;

            if (boundsBuf == null)
            {
                boundsBuf = new ComputeBuffer(1, sizeof(float) * 6);
                boundsBuf.SetData(bounds);
            }

            if (octreeBuf == null)
            {
                octreeBuf = new ComputeBuffer(octree.Count, (sizeof(float) * 80) + sizeof(uint));
                octreeBuf.SetData(octree);
            }
            else if (octreeBuf.count != octree.Count)
            {
                octreeBuf.Release();
                octreeBuf = new ComputeBuffer(octree.Count, (sizeof(float) * 80) + sizeof(uint));
                octreeBuf.SetData(octree);
            }

            cmd.SetComputeBufferParam(_computeShader, kernelIndex, "_octreeBounds", boundsBuf);
            cmd.SetComputeBufferParam(_computeShader, kernelIndex, "_octree", octreeBuf);
            cmd.SetComputeIntParam(_computeShader, "_octreeTotalNodes", octree.Count);

            cmd.SetComputeTextureParam(_computeShader, kernelIndex, "_sceneCol", rayMarcher.RenderTex, 0, RenderTextureSubElement.Color);
            //cmd.SetComputeTextureParam(_computeShader, kernelIndex, "_depthTex", _rayMarcher.RenderDepthTex, 0, RenderTextureSubElement.Color);
            cmd.SetComputeVectorParam(_computeShader, "_CameraPos", hdCamera.camera.transform.position);
            //cmd.SetComputeMatrixParam(_computeShader, "_cameraInvMatrix", hdCamera.camera.transform.localToWorldMatrix.inverse);
            //cmd.SetComputeMatrixParam(_computeShader, "_camLocalToWorldMatrix", hdCamera.camera.transform.localToWorldMatrix);
            cmd.SetComputeMatrixParam(_computeShader, "_cameraToWorldMatrix", hdCamera.camera.cameraToWorldMatrix);
            //cmd.SetComputeMatrixParam(_computeShader, "_cameraToWorldInvMatrix", hdCamera.camera.cameraToWorldMatrix.inverse);
            cmd.SetComputeMatrixParam(_computeShader, "_cameraInvProj", hdCamera.camera.projectionMatrix.inverse);

            cmd.SetComputeVectorParam(_computeShader, "_lightDir", sunlight ? sunlight.forward : Vector3.down);
            cmd.SetComputeVectorParam(_computeShader, "_lightPos", sunlight ? sunlight.position : Vector3.zero);
            cmd.SetComputeFloatParam(_computeShader, "_specularExp", _settings.SpecularExp);
            cmd.SetComputeFloatParam(_computeShader, "_attenuationConstant", _settings.AttenuationConstant);
            cmd.SetComputeFloatParam(_computeShader, "_attenuationLinear", _settings.AttenuationLinear);
            cmd.SetComputeFloatParam(_computeShader, "_attenuationQuadratic", _settings.AttenuationQuadratic);
            cmd.SetComputeVectorParam(_computeShader, "_ambientColour", _settings.AmbientColour);
            cmd.SetComputeVectorParam(_computeShader, "_diffuseColour", _settings.DiffuseColour);
            cmd.SetComputeVectorParam(_computeShader, "_specularColour", _settings.SpecualarColour);
            cmd.SetComputeVectorParam(_computeShader, "_lightConstants", _settings.LightConstants);


            int Xgroups = Mathf.CeilToInt(hdCamera.actualWidth / 26.0f);
            int Ygroups = Mathf.CeilToInt(hdCamera.actualHeight / 26.0f);
            cmd.DispatchCompute(_computeShader, kernelIndex, Xgroups, Ygroups, 1);
        }
    }

    protected override void renderPrimitive(RMPrimitive rmPrim, ref int primIndex, ref int altIndex)
    {
        // Homogeneous transformation matrices

        _invModelMats[primIndex] = rmPrim.transform.localToWorldMatrix.inverse;

        //_invModelMats[primIndex] = rmPrim.transform.localToWorldMatrix;
        //_invModelMats[primIndex].m00 = 1.0f;
        //_invModelMats[primIndex].m11 = 1.0f;
        //_invModelMats[primIndex].m22 = 1.0f;
        //_invModelMats[primIndex] = _invModelMats[primIndex].inverse;

        //Matrix4x4 localToWorldNoScale = Matrix4x4.TRS(rmPrim.transform.position, rmPrim.transform.rotation, Vector3.one);

        //_invModelMats[primIndex] = localToWorldNoScale.inverse;

        // Store maximum un-inverted scale for each objects's transformation matrix.
        Matrix4x4 invModelMat = _invModelMats[primIndex];
        float xScale = new Vector3(invModelMat[0, 0], invModelMat[0, 1], invModelMat[0, 2]).magnitude;
        float yScale = new Vector3(invModelMat[1, 0], invModelMat[1, 1], invModelMat[1, 2]).magnitude;
        float zScale = new Vector3(invModelMat[2, 0], invModelMat[2, 1], invModelMat[2, 2]).magnitude;

        float maxScale = Mathf.Max(xScale, Mathf.Max(yScale, zScale));

        //maxScale = maxScale == 0.0f ? 0.2f : 1.0f / maxScale;

        _scaleBuffer[primIndex] = 1.0f / maxScale;


        // Colour information
        _colours[primIndex] = rmPrim.Colour;

        // Primitive to render
        //primitiveTypes[primIndex] = (float)rmPrim.PrimitiveType;

        // Combine Operation
        _combineOps[primIndex] = rmPrim.CombineOp;

        // Primitive Geometry Information
        _primitiveGeoInfo[primIndex] = rmPrim.GeoInfo;

        // Reflection Information
        _reflInfo[primIndex] = rmPrim.ReflectionInfo;

        // Refraction Information
        Vector4 info = rmPrim.RefractionInfo;
        if (info.x > 0.0f)
            info.x = 1.0f;
        _refractInfo[primIndex] = info;

        // Alterations' Information
        foreach (Alteration alt in rmPrim.Alterations)
        {
            _altInfo[altIndex] = alt.info;
            ++altIndex;
        }
        //foreach(Vector4 altInfo in rmPrim.AlterationInfo)
        //{
        //    _altInfo[altIndex] = altInfo;
        //    ++altIndex;
        //}

        _boundGeoInfo[primIndex] = rmPrim.BoundGeoInfo;

        ++primIndex;
    }

    protected override void renderCSG(CSG csg, ref int primIndex, ref int csgIndex, ref int altIndex)
    {
        // TO-DO Don't let incomplete CSG children nodes be added to other CSGs.
        // Base case: Both nodes are primitives.
        if (csg.AllPrimNodes)
        {
            // Render both nodes.
            renderPrimitive(csg.FirstNode as RMPrimitive, ref primIndex, ref altIndex);
            renderPrimitive(csg.SecondNode as RMPrimitive, ref primIndex, ref altIndex);

            // Buffer this CSG.
            _bufferedCSGs[csgIndex] = new Vector4(primIndex - 1, primIndex, -1, -1);
            _combineOpsCSGs[csgIndex] = csg.CombineOp;
            //_boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
            ++csgIndex;
            return;
        }
        // Only first node is a primitive.
        else if (csg.IsFirstPrim)
        {
            // Recurse through second node (Must be a CSG).
            renderCSG(csg.SecondNode as CSG, ref primIndex, ref csgIndex, ref altIndex);

            // Render first node.
            renderPrimitive(csg.FirstNode as RMPrimitive, ref primIndex, ref altIndex);

            // Buffer this CSG.
            _bufferedCSGs[csgIndex] = new Vector4(primIndex, -1, -1, csgIndex - 1);
            _combineOpsCSGs[csgIndex] = csg.CombineOp;
            //_boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
            ++csgIndex;
            return;
        }
        // Only second node is a primitive.
        else if (csg.IsSecondPrim)
        {
            // Recurse through first node (Must be a csg).
            renderCSG(csg.FirstNode as CSG, ref primIndex, ref csgIndex, ref altIndex);

            // Render second node.
            renderPrimitive(csg.SecondNode as RMPrimitive, ref primIndex, ref altIndex);

            // Buffer this CSG.
            _bufferedCSGs[csgIndex] = new Vector4(-1, primIndex, csgIndex - 1, -1);
            _combineOpsCSGs[csgIndex] = csg.CombineOp;
            //_boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
            ++csgIndex;
            return;
        }
        // Both nodes are CSGs.
        else
        {
            Vector4 tempCSG = new Vector4(-1, -1, -1, -1);

            // Recurse through first node.
            renderCSG(csg.FirstNode as CSG, ref primIndex, ref csgIndex, ref altIndex);
            tempCSG.z = csgIndex;

            // Recurse through second node.
            renderCSG(csg.SecondNode as CSG, ref primIndex, ref csgIndex, ref altIndex);
            tempCSG.w = csgIndex;

            // Buffer this CSG.
            _bufferedCSGs[csgIndex] = tempCSG;
            _combineOpsCSGs[csgIndex] = csg.CombineOp;
            //_boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
            ++csgIndex;
            return;
        }
    }
}
