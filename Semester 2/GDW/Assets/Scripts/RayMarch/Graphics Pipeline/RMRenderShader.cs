using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RMRenderShader : RayMarchShader
{
    Color[] _colours = new Color[32];                // The _colours of every object.
    Vector4[] _reflInfo = new Vector4[32];          // The reflection info for every primitive object.
    Vector4[] _refractInfo = new Vector4[32];       // The refractiin info for every primitive object.



    public void render(Material material, Matrix4x4 cameraInvViewMatrix, Vector3 camPos, Transform sunLight)
    {
        // Enable this shader's defines
        foreach (ShaderKeywords keyword in _settings.Keywords)
        {
            material.EnableKeyword(keyword.ToString());
        }

        material.SetMatrix("_CameraInvViewMatrix", cameraInvViewMatrix);
        material.SetVector("_CameraPos", camPos);
        //material.SetMatrix("_TorusMat_InvModel", torusMat.inverse);
        //material.SetTexture("_colourRamp", _settings.ColourRamp);
        material.SetTexture("_performanceRamp", _settings.PerformanceRamp);
        //material.SetTexture("_wood", _settings.Wood);
        //material.SetTexture("_brick", _settings.Brick);

        material.SetFloat("_specularExp", _settings.SpecularExp);
        material.SetFloat("_attenuationConstant", _settings.AttenuationConstant);
        material.SetFloat("_attenuationLinear", _settings.AttenuationLinear);
        material.SetFloat("_attenuationQuadratic", _settings.AttenuationQuadratic);
        material.SetVector("_LightDir", sunLight ? sunLight.forward : Vector3.down);
        material.SetVector("_lightPos", sunLight ? sunLight.position : Vector3.zero);
        material.SetColor("_ambientColour", _settings.AmbientColour);
        material.SetColor("_diffuseColour", _settings.DiffuseColour);
        material.SetColor("_specularColour", _settings.SpecualarColour);
        material.SetVector("_lightConstants", _settings.LightConstants);
        material.SetColor("_rimLightColour", _settings.RimLightColour);
        material.SetFloat("_totalTime", Time.time);

        // ######### Shadow Variables #########
        material.SetFloat("_penumbraFactor", _settings.PenumbraFactor);
        material.SetFloat("_shadowMinDist", _settings.ShadowmMinDist);
        material.SetFloat("_shadowIntensity", _settings.ShadowIntensity);

        // ######### Ray March Variables #########
        material.SetInt("_maxSteps", _settings.MaxSteps);
        material.SetFloat("_maxDrawDist", _settings.MaxDrawDist);

        // ######### Reflection Variables #########
        material.SetInt("_reflectionCount", _settings.ReflectionCount);
        material.SetFloat("_reflectionIntensity", _settings.ReflectionIntensity);
        material.SetFloat("_envReflIntensity", _settings.EnvReflIntensity);
        material.SetTexture("_skybox", _settings.SkyBox);

        // ######### Refraction Variables #########
        material.SetVectorArray("_refractInfo", _refractInfo);

        // ######### Ambient Occlusion Variables #########
        material.SetInt("_aoMaxSteps", _settings.AOMaxSteps);
        material.SetFloat("_aoStepSize", _settings.AOStepSize);
        material.SetFloat("_aoIntensity", _settings.AOItensity);

        // ######### Fog Variables #########
        material.SetFloat("_fogExtinction", _settings.FogExtinction);
        material.SetFloat("_fogInscattering", _settings.FogInscattering);
        material.SetColor("_fogColour", _settings.FogColour);

        // ######### Vignette Variables #########
        material.SetFloat("_vignetteIntensity", _settings.VignetteIntesnity);


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
            material.SetMatrixArray("_invModelMats", _invModelMats);
            material.SetColorArray("_rm_colours", _colours);
            material.SetVectorArray("_combineOps", _combineOps);
            material.SetVectorArray("_primitiveGeoInfo", _primitiveGeoInfo);
            material.SetVectorArray("_reflInfo", _reflInfo);
            material.SetVectorArray("_altInfo", _altInfo);

            material.SetVectorArray("_bufferedCSGs", _bufferedCSGs);
            material.SetVectorArray("_combineOpsCSGs", _combineOpsCSGs);

            material.SetVectorArray("_boundGeoInfo", _boundGeoInfo);
        }
    }

    protected override void renderPrimitive(RMPrimitive rmPrim, ref int primIndex, ref int altIndex)
    {
        // Homogeneous transformation matrices
        _invModelMats[primIndex] = rmPrim.transform.localToWorldMatrix.inverse;

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
}
