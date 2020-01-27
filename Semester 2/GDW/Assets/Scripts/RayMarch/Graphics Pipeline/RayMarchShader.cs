using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[ExecuteInEditMode]
public class RayMarchShader : MonoBehaviour
{
    [SerializeField]
    private Shader _effectShader = null;
    [SerializeField]
    private string _shaderName;
    [SerializeField]
    RayMarchShaderSettings _settings;
    [SerializeField]
    List<RMObj> _renderList = new List<RMObj>();
    //private RMObj[] objects;                                // The array of objects to render.


    private Matrix4x4[] _invModelMats = new Matrix4x4[32];   // The inverse transformation matrices of every object.
    private Color[] _colours = new Color[32];                // The _colours of every object.
    private Vector4[] _combineOps = new Vector4[32];         // The object to scene combine operations, for every object.
    private Vector4[] _primitiveGeoInfo = new Vector4[32];   // The geometric info for every primitive object.
    private Vector4[] _reflInfo = new Vector4[32];          // The reflection info for every primitive object.
    private Vector4[] _refractInfo = new Vector4[32];       // The refractiin info for every primitive object.
    private Vector4[] _altInfo = new Vector4[32];           // The alteration info for every object.


    private Vector4[] _bufferedCSGs = new Vector4[16];       // The list of node indices for a each CSG.
    private Vector4[] _combineOpsCSGs = new Vector4[16];     // The node to node combine operations for each CSG.


    private Vector4[] _boundGeoInfo = new Vector4[32];      // The geometric info for every object's bounding volume.


    public RayMarchShaderSettings Settings
    {
        get
        {
            return _settings;
        }
        set
        {
            _settings = value;
        }
    }
    
    public Shader EffectShader
    {
        get
        {
            return _effectShader;
        }
        set
        {
            _effectShader = value;
        }
    }

    public List<RMObj> RenderList
    {
        get
        {
            return _renderList;
        }
    }

    public void AddToRenderList(RMObj rmObj)
    {
        // Check if the object is not already in the render list.
        if (!_renderList.Contains(rmObj))
        {
            // Add the object to the render list, and sort the list based on draw order.
            _renderList.Add(rmObj);
            _renderList.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        }
    }

    public void removeFromRenderList(RMObj rmObj)
    {
        int index = _renderList.IndexOf(rmObj);

        // Check if the object is in the list, if so then remove it.
        if (index > -1)
        {
            _renderList[index] = _renderList[_renderList.Count - 1];
            _renderList.RemoveAt(index);
        }
        
    }

    public void removeAllFromRenderList()
    {
        _renderList.Clear();
        // Also run rmObj remove actions
    }

    public string ShaderName
    {
        get
        {
            return _shaderName;
        }

        set
        {
            _shaderName = value;
        }
    }


    //public RayMarchShader()
    //{
    //    // TO-DO load saved render list

    //    _renderList = new List<RMObj>();
    //}


    //void Awake()
    //{
        //if (Application.isPlaying)
        //{
        //    objects = FindObjectsOfType<RMObj>();
        //    _renderList = new List<RMObj>(objects);

        //    _renderList.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        //}
    //}

    public void disableKeywords(Material material)
    {
        // Disable this shader's defines
        foreach (ShaderKeywords keyword in _settings.Keywords)
        {
            material.DisableKeyword(keyword.ToString());
        }
    }


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


    private void renderPrimitive(RMPrimitive rmPrim, ref int primIndex, ref int altIndex)
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

    private void renderCSG(CSG csg, ref int primIndex, ref int csgIndex, ref int altIndex)
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
           _boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
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
           _boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
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
           _boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
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
           _boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
           ++csgIndex;
           return;
       }
    }
}
