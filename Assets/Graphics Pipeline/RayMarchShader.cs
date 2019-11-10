using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShaderKeywords
{
    BOUND_DEBUG
}

[CreateAssetMenu(fileName = "New Ray March Shader", menuName = "Ray March Shader")]
public class RayMarchShader : ScriptableObject
{
    private List<ShaderKeywords> _keywords = new List<ShaderKeywords>();// The material keywords to disable for this shader.
    private Shader _effectShader = null;
    private string _shaderName;

    private Matrix4x4[] invModelMats = new Matrix4x4[32];   // The inverse transformation matrices of every object.
    private Color[] colours = new Color[32];                // The colours of every object.
    private Vector4[] combineOps = new Vector4[32];         // The object to scene combine operations, for every object.
    private Vector4[] primitiveGeoInfo = new Vector4[32];   // The geometric info for every primitive object.
    private Vector4[] _reflInfo = new Vector4[32];          // The reflection info for every primitive object.
    private Vector4[] _refractInfo = new Vector4[32];       // The refractiin info for every primitive object.
    private Vector4[] _altInfo = new Vector4[32];           // The alteration info for every object.


    private Vector4[] bufferedCSGs = new Vector4[16];       // The list of node indices for a each CSG.
    private Vector4[] combineOpsCSGs = new Vector4[16];     // The node to node combine operations for each CSG.


    private Vector4[] _boundGeoInfo = new Vector4[32];      // The geometric info for every object's bounding volume.


    private RMObj[] objects;                                // The array of objects to render.
    private List<RMObj> _renderList = new List<RMObj>();    // The list of objects to render.


    [SerializeField]
    private Texture2D _colourRamp = null;
    [SerializeField]
    private Texture2D _performanceRamp = null;
    [SerializeField]
    private Texture2D _wood = null;
    [SerializeField]
    private Texture2D _brick = null;

    [Header("Ray March")]
    [SerializeField]
    [Range(0.0f, 600.0f)]
    private int _maxSteps = 100;
    [SerializeField]
    [Range(0.0f, 600.0f)]
    private float _maxDrawDist = 64.0f;//12

    // ######### Light Variables #########
    [Header("Light")]
    // Floats
    [SerializeField]
    private float _specularExp = 160.0f;
    [SerializeField]
    private float _attenuationConstant = 1.5f;
    [SerializeField]
    private float _attenuationLinear = 0.01f;
    [SerializeField]
    private float _attenuationQuadratic = 0.001f;

    // Vectors
    [SerializeField]
    private Color _ambientColour = new Color(137.0f, 137.0f, 137.0f, 0.0f);
    [SerializeField]
    private Color _diffuseColour = new Color(130.0f, 130.0f, 130.0f, 0.0f);
    [SerializeField]
    private Color _specularColour = new Color(255.0f, 255.0f, 255.0f, 0.0f);
    [SerializeField]
    private Vector3 _lightConstants = new Vector3(0.2f, 0.2f, 2.0f);
    [SerializeField]
    private Color _rimLightColour = new Color(255.0f, 0.0f, 219.0f, 0.0f);
    public Transform sunLight;
    // ######### Light Variables #########


    [Header("Shadow")]
    // ######### Shadow Variables #########
    [SerializeField]
    [Range(1.0f, 140.0f)]
    private float _penumbraFactor = 15.0f;
    [SerializeField]
    [Range(0.01f, 5.0f)]
    private float _shadowMinDist = 0.01f;
    [SerializeField]
    [Range(0.0f, 10.0f)]
    private float _shadowIntensity = 1.0f;
    // ######### Shadow Variables #########


    // ######### Reflection Variables #########
    [Header("Reflection")]
    [SerializeField]
    [Range(0.0f, 3.0f)]
    private int _reflectionCount = 0;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float _reflectionIntensity = 0.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float _envReflIntensity = 0.0f;
    [SerializeField]
    private Texture _skybox = null;
    // ######### Reflection Variables #########

    // ######### Ambient Occlusion Variables #########
    [Header("Ambient Occlusion")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    int _aoMaxSteps = 3;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _aoStepSize = 0.2f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _aoIntensity = 0.3f;
    // ######### Ambient Occlusion Variables #########

    [Header("Vignette")]
    [SerializeField]
    [Range(0.0f, 2.0f)]
    public float _vignetteIntensity = 0.0f;

    [Header("Fog")]
    [SerializeField]
    [Range(0.0f, 0.04f)]
    private float _fogExtinction = 0.0f;
    [SerializeField]
    [Range(0.0f, 0.04f)]
    private float _fogInscattering = 0.0f;
    [SerializeField]
    private Color _fogColour = Color.grey;

    // TO-DO delete unsued hidden shaders

    #region Getters and Setters
    public Shader EffectShader
    {
        get
        {
            return _effectShader;
        }
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

    public List<RMObj> RenderList
    {
        get
        {
            return _renderList;
        }
    }

    public int MaxSteps
    {
        get
        {
            return _maxSteps;
        }
        set
        {
            _maxSteps = value;
        }
    }

    public float MaxDrawDist
    {
        get
        {
            return _maxDrawDist;
        }
        set
        {
            _maxDrawDist = value;
        }
    }

    public float SpecularExp
    {
        get
        {
            return _specularExp;
        }
        set
        {
            _specularExp = value;
        }
    }

    public float AttenuationConstant
    {
        get
        {
            return _attenuationConstant;
        }
        set
        {
            _attenuationConstant = value;
        }
    }

    public float AttenuationLinear
    {
        get
        {
            return _attenuationLinear;
        }
        set
        {
            _attenuationLinear = value;
        }
    }

    public float AttenuationQuadratic
    {
        get
        {
            return _attenuationQuadratic;
        }
        set
        {
            _attenuationQuadratic = value;
        }
    }

    public Color AmbientColour
    {
        get
        {
            return _ambientColour;
        }
        set
        {
            _ambientColour = value;
        }
    }

    public Color DiffuseColour
    {
        get
        {
            return _ambientColour;
        }
        set
        {
            _ambientColour = value;
        }
    }

    public Color SpecualarColour
    {
        get
        {
            return _specularColour;
        }
        set
        {
            _specularColour = value;
        }
    }

    public Vector3 LightConstants
    {
        get
        {
            return _lightConstants;
        }
        set
        {
            _lightConstants = value;
        }
    }

    public Color RimLightColour
    {
        get
        {
            return _rimLightColour;
        }
        set
        {
            _rimLightColour = value;
        }
    }

    public Transform SunLight
    {
        get
        {
            return sunLight;
        }
        set
        {
            sunLight = value;
        }
    }

    public float PenumbraFactor
    {
        get
        {
            return _penumbraFactor;
        }
        set
        {
            _penumbraFactor = value;
        }
    }

    public float ShadowmMinDist
    {
        get
        {
            return _shadowMinDist;
        }
        set
        {
            _shadowMinDist = value;
        }
    }

    public float ShadowIntensity
    {
        get
        {
            return _shadowIntensity;
        }
        set
        {
            _shadowIntensity = value;
        }
    }

    public int ReflectionCount
    {
        get
        {
            return _reflectionCount;
        }
        set
        {
            _reflectionCount = value;
        }
    }

    public float ReflectionIntensity
    {
        get
        {
            return _reflectionIntensity;
        }
        set
        {
            _reflectionIntensity = value;
        }
    }

    public float EnvReflIntensity
    {
        get
        {
            return _envReflIntensity;
        }
        set
        {
            _envReflIntensity = value;
        }
    }

    public Texture SkyBox
    {
        get
        {
            return _skybox;
        }
        set
        {
            _skybox = value;
        }
    }

    public int AOMaxSteps
    {
        get
        {
            return _aoMaxSteps;
        }
        set
        {
            _aoMaxSteps = value;
        }
    }

    public float AOStepSize
    {
        get
        {
            return _aoStepSize;
        }
        set
        {
            _aoStepSize = value;
        }
    }

    public float AOItensity
    {
        get
        {
            return _aoIntensity;
        }
        set
        {
            _aoIntensity = value;
        }
    }

    public float VignetteIntesnity
    {
        get
        {
            return _vignetteIntensity;
        }
        set
        {
            _vignetteIntensity = value;
        }
    }

    public float FogExtinction
    {
        get
        {
            return _fogExtinction;
        }
        set
        {
            _fogExtinction = value;
        }
    }

    public float FogInscattering
    {
        get
        {
            return _fogInscattering;
        }
        set
        {
            _fogInscattering = value;
        }
    }

    public Color FogColour
    {
        get
        {
            return _fogColour;
        }
        set
        {
            _fogColour = value;
        }
    }
    #endregion Getters and Setters



    // Start is called before the first frame update
    void Awake()
    {
        if (Application.isPlaying)
        {
            objects = FindObjectsOfType<RMObj>();
            _renderList = new List<RMObj>(objects);

            _renderList.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        }
    }


    public void render(Material material, Matrix4x4 frustomCorners, Matrix4x4 cameraInvViewMatrix, Vector3 camPos)
    {
        // Enable this shader's defines
        foreach (ShaderKeywords keyword in _keywords)
        {
            material.EnableKeyword(keyword.ToString());
        }

        material.SetMatrix("_FrustumCornersES", frustomCorners);
        material.SetMatrix("_CameraInvViewMatrix", cameraInvViewMatrix);
        material.SetVector("_CameraPos", camPos);
        //material.SetMatrix("_TorusMat_InvModel", torusMat.inverse);
        material.SetTexture("_colourRamp", _colourRamp);
        material.SetTexture("_performanceRamp", _performanceRamp);
        material.SetTexture("_wood", _wood);
        material.SetTexture("_brick", _brick);

        material.SetFloat("_specularExp", _specularExp);
        material.SetFloat("_attenuationConstant", _attenuationConstant);
        material.SetFloat("_attenuationLinear", _attenuationLinear);
        material.SetFloat("_attenuationQuadratic", _attenuationQuadratic);
        material.SetVector("_LightDir", sunLight ? sunLight.forward : Vector3.down);
        material.SetVector("_lightPos", sunLight ? sunLight.position : Vector3.zero);
        material.SetColor("_ambientColour", _ambientColour);
        material.SetColor("_diffuseColour", _diffuseColour);
        material.SetColor("_specularColour", _specularColour);
        material.SetVector("_lightConstants", _lightConstants);
        material.SetColor("_rimLightColour", _rimLightColour);
        material.SetFloat("_totalTime", Time.time);

        // ######### Shadow Variables #########
        material.SetFloat("_penumbraFactor", _penumbraFactor);
        material.SetFloat("_shadowMinDist", _shadowMinDist);
        material.SetFloat("_shadowIntensity", _shadowIntensity);

        // ######### Ray March Variables #########
        material.SetInt("_maxSteps", _maxSteps);
        material.SetFloat("_maxDrawDist", _maxDrawDist);

        // ######### Reflection Variables #########
        material.SetInt("_reflectionCount", _reflectionCount);
        material.SetFloat("_reflectionIntensity", _reflectionIntensity);
        material.SetFloat("_envReflIntensity", _envReflIntensity);
        material.SetTexture("_skybox", _skybox);

        // ######### Refraction Variables #########
        material.SetVectorArray("_refractInfo", _refractInfo);

        // ######### Ambient Occlusion Variables #########
        material.SetInt("_aoMaxSteps", _aoMaxSteps);
        material.SetFloat("_aoStepSize", _aoStepSize);
        material.SetFloat("_aoIntensity", _aoIntensity);

        // ######### Fog Variables #########
        material.SetFloat("_fogExtinction", _fogExtinction);
        material.SetFloat("_fogInscattering", _fogInscattering);
        material.SetColor("_fogColour", _fogColour);

        // ######### Vignette Variables #########
        material.SetFloat("_vignetteIntensity", _vignetteIntensity);


        int primIndex = 0;
        int csgIndex = 0;
        int altIndex = 0;


        if (!Application.isPlaying)
        {
            objects = FindObjectsOfType<RMObj>();
            _renderList = new List<RMObj>(objects);

            _renderList.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        }

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
            material.SetMatrixArray("_invModelMats", invModelMats);
            material.SetColorArray("_rm_colours", colours);
            material.SetVectorArray("_combineOps", combineOps);
            material.SetVectorArray("_primitiveGeoInfo", primitiveGeoInfo);
            material.SetVectorArray("_reflInfo", _reflInfo);
            material.SetVectorArray("_altInfo", _altInfo);

            material.SetVectorArray("_bufferedCSGs", bufferedCSGs);
            material.SetVectorArray("_combineOpsCSGs", combineOpsCSGs);

            material.SetVectorArray("_boundGeoInfo", _boundGeoInfo);
        }


        // Disable this shader's defines
        foreach (ShaderKeywords keyword in _keywords)
        {
            material.DisableKeyword(keyword.ToString());
        }
    }


    private void renderPrimitive(RMPrimitive rmPrim, ref int primIndex, ref int altIndex)
    {
        // Homogeneous transformation matrices
        invModelMats[primIndex] = rmPrim.transform.localToWorldMatrix.inverse;

        // Colour information
        colours[primIndex] = rmPrim.Colour;

        // Primitive to render
        //primitiveTypes[primIndex] = (float)rmPrim.PrimitiveType;

        // Combine Operation
        combineOps[primIndex] = rmPrim.CombineOp;

        // Primitive Geometry Information
        primitiveGeoInfo[primIndex] = rmPrim.GeoInfo;

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
            bufferedCSGs[csgIndex] = new Vector4(primIndex - 1, primIndex, -1, -1);
            combineOpsCSGs[csgIndex] = csg.CombineOp;
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
            bufferedCSGs[csgIndex] = new Vector4(primIndex, -1, -1, csgIndex - 1);
            combineOpsCSGs[csgIndex] = csg.CombineOp;
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
            bufferedCSGs[csgIndex] = new Vector4(-1, primIndex, csgIndex - 1, -1);
            combineOpsCSGs[csgIndex] = csg.CombineOp;
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
            bufferedCSGs[csgIndex] = tempCSG;
            combineOpsCSGs[csgIndex] = csg.CombineOp;
            _boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
            ++csgIndex;
            return;
        }
    }
}
