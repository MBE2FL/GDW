using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Ray Marching/RayMarcher")]
[DisallowMultipleComponent]

public class RayMarcher : SceneViewFilter
{
    [SerializeField]
    private Shader _effectShader = null;
    private Material _effectMaterial;
    private Camera _currentCamera;
    [SerializeField]
    private Texture2D _colourRamp = null;
    [SerializeField]
    private Texture2D _performanceRamp = null;
    [SerializeField]
    private float _smoothUnionFactor = 0.0f;
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
    private float _maxDrawDist = 64.0f;

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

    public float _refractIndex = 0.0f;
    
    [Header("Vignette")]
    [SerializeField]
    [Range(0.0f, 2.0f)]
    public float _vignetteIntensity = 0.0f;

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

    public Material EffectMaterial
    {
        get
        {
            if (!_effectMaterial && _effectShader)
            {
                _effectMaterial = new Material(_effectShader);
                _effectMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return _effectMaterial;
        }
        set
        {
            _effectMaterial = value;
        }
    }

    public Camera CurrentCamera
    {
        get
        {
            if (!_currentCamera)
                _currentCamera = GetComponent<Camera>();

            return _currentCamera;
        }
    }

    public int ReflectionCount
    {
        get
        {
            return _reflectionCount;
        }
    }


    // ######### Ray Marcher Inspector Variables #########
    //private const int MAX_RM_PRIMS = 128;
    //static private RMPrimitive[] _rmPrims = new RMPrimitive[MAX_RM_PRIMS];
    //static private List<RMPrimitive> _rmPrims = new List<RMPrimitive>(MAX_RM_PRIMS);
    //static private uint _currentObjs = 0;
    [HideInInspector]
    private RMMemoryManager _rmMemoryManager;
    // ######### Ray Marcher Inspector Variables #########


    private Matrix4x4[] invModelMats = new Matrix4x4[32];
    private Color[] colours = new Color[32];
    //private float[] primitiveTypes = new float[32];
    private Vector4[] combineOps = new Vector4[32];
    private Vector4[] primitiveGeoInfo = new Vector4[32];
    private Vector4[] _reflInfo = new Vector4[32];
    private Vector4[] _refractInfo = new Vector4[32];


    private Vector4[] bufferedCSGs = new Vector4[16];
    private Vector4[] combineOpsCSGs = new Vector4[16];
    //private float[] csgNodesPerRoot = new float[16];



    private void Start()
    {
        // Retrieve a reference to the ray marching memory manager from the main camera.
        _rmMemoryManager = Camera.main.GetComponent<RMMemoryManager>();

        // Make sure a memory manager exists, else create one.
        if (!_rmMemoryManager)
        {
            _rmMemoryManager = Camera.main.gameObject.AddComponent<RMMemoryManager>();
        }
    }


    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!EffectMaterial)
        {
            Graphics.Blit(source, destination); // Do Nothing
            return;
        }

        Matrix4x4 torusMat = Matrix4x4.TRS(
                                            Vector3.right * Mathf.Sin(Time.time) * 5.0f,
                                            Quaternion.identity,
                                            Vector3.one);
        torusMat *= Matrix4x4.TRS(
                                   Vector3.zero,
                                   Quaternion.Euler(new Vector3(0.0f, 0.0f, (Time.time * 200.0f) % 360.0f)),
                                   Vector3.one);

        EffectMaterial.SetMatrix("_FrustumCornersES", GetFrustumCorners(CurrentCamera));
        EffectMaterial.SetMatrix("_CameraInvMatrix", CurrentCamera.cameraToWorldMatrix);
        EffectMaterial.SetVector("_CameraPos", CurrentCamera.transform.position);
        EffectMaterial.SetMatrix("_TorusMat_InvModel", torusMat.inverse);
        EffectMaterial.SetTexture("_colourRamp", _colourRamp);
        EffectMaterial.SetTexture("_performanceRamp", _performanceRamp);
        EffectMaterial.SetFloat("_smoothUnionFactor", _smoothUnionFactor);
        EffectMaterial.SetTexture("_wood", _wood);
        EffectMaterial.SetTexture("_brick", _brick);

        EffectMaterial.SetFloat("_specularExp", _specularExp);
        EffectMaterial.SetFloat("_attenuationConstant", _attenuationConstant);
        EffectMaterial.SetFloat("_attenuationLinear", _attenuationLinear);
        EffectMaterial.SetFloat("_attenuationQuadratic", _attenuationQuadratic);
        EffectMaterial.SetFloat("_attenuationQuadratic", _attenuationQuadratic);
        EffectMaterial.SetVector("_LightDir", sunLight ? sunLight.forward : Vector3.down);
        EffectMaterial.SetVector("_lightPos", sunLight ? sunLight.position : Vector3.zero);
        EffectMaterial.SetColor("_ambientColour", _ambientColour);
        EffectMaterial.SetColor("_diffuseColour", _diffuseColour);
        EffectMaterial.SetColor("_specularColour", _specularColour);
        EffectMaterial.SetVector("_lightConstants", _lightConstants);
        EffectMaterial.SetColor("_rimLightColour", _rimLightColour);
        EffectMaterial.SetFloat("_totalTime", Time.time);

        // ######### Shadow Variables #########
        EffectMaterial.SetFloat("_penumbraFactor", _penumbraFactor);
        EffectMaterial.SetFloat("_shadowMinDist", _shadowMinDist);
        EffectMaterial.SetFloat("_shadowIntensity", _shadowIntensity);

        // ######### Ray March Variables #########
        EffectMaterial.SetInt("_maxSteps", _maxSteps);
        EffectMaterial.SetFloat("_maxDrawDist", _maxDrawDist);

        // ######### Reflection Variables #########
        EffectMaterial.SetInt("_reflectionCount", _reflectionCount);
        EffectMaterial.SetFloat("_reflectionIntensity", _reflectionIntensity);
        EffectMaterial.SetFloat("_envReflIntensity", _envReflIntensity);
        EffectMaterial.SetTexture("_skybox", _skybox);

        // ######### Refraction Variables #########
        EffectMaterial.SetVectorArray("_refractInfo", _refractInfo);

        // ######### Ambient Occlusion Variables #########
        EffectMaterial.SetInt("_aoMaxSteps", _aoMaxSteps);
        EffectMaterial.SetFloat("_aoStepSize", _aoStepSize);
        EffectMaterial.SetFloat("_aoIntensity", _aoIntensity);


        EffectMaterial.SetFloat("_refractIndex", _refractIndex);
        EffectMaterial.SetFloat("_vignetteIntensity", _vignetteIntensity);



        //List<Matrix4x4> invModelMats = new List<Matrix4x4>(new Matrix4x4[128]);
        //List<Color> colours = new List<Color>(new Color[128]);
        //List<float> primitiveTypes = new List<float>(new float[128]);
        //_rmMemoryManager = RMMemoryManager.Instance;
        if (!_rmMemoryManager)
        {
            _rmMemoryManager = Camera.main.GetComponent<RMMemoryManager>();
        }

        int primIndex = 0;
        int csgIndex = 0;
        //int totalRootCSGs = -1;
        //int csgNodes = 0;
        invModelMats = new Matrix4x4[32];
        colours = new Color[32];
        //primitiveTypes = new float[32];
        _reflInfo = new Vector4[32];
        combineOps = new Vector4[32];
        bufferedCSGs = new Vector4[32];

        List<RMPrimitive> rmPrims = _rmMemoryManager.RM_Prims;
        List<CSG> csgs = _rmMemoryManager.CSGs;
        List<RMObj> objs = new List<RMObj>(rmPrims.Count + csgs.Count);

        objs.AddRange(rmPrims);
        objs.AddRange(csgs);
        objs.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));

        RMPrimitive prim;
        CSG csg;
        RMObj obj;

        for (int i = 0; i < objs.Count; ++i)
        {
            obj = objs[i];

            // Primitive
            if (obj.IsPrim)
            {
                prim = obj as RMPrimitive;

                // Skip any primitives belonging to a csg, as they will be rendered recursively by thier respective csgs.
                if (prim.CSGNode)
                    continue;

                renderPrimitive(prim, ref primIndex);
            }
            // CSG
            else
            {
                csg = obj as CSG;

                // Skip any non-root CSGs, as they will be rendered recursively by thier parents.
                // Skip any CSGs which don't have two nodes.
                if (!csg.IsRoot || !csg.IsValid)
                    continue;

                renderCSG(csg, ref primIndex, ref csgIndex);
            }
        }

        if (primIndex > 0)
        {
            EffectMaterial.SetMatrixArray("_invModelMats", invModelMats);
            EffectMaterial.SetColorArray("_rm_colours", colours);
            //EffectMaterial.SetFloatArray("_primitiveTypes", primitiveTypes);
            EffectMaterial.SetVectorArray("_combineOps", combineOps);
            EffectMaterial.SetVectorArray("_primitiveGeoInfo", primitiveGeoInfo);
            EffectMaterial.SetVectorArray("_reflInfo", _reflInfo);

            EffectMaterial.SetVectorArray("_bufferedCSGs", bufferedCSGs);
            EffectMaterial.SetVectorArray("_combineOpsCSGs", combineOpsCSGs);
        }


        //Graphics.Blit(source, destination, EffectMaterial, 0); // Use given effect shader as an image effect
        CustomGraphicsBlit(source, destination, EffectMaterial, 0);


        // Cleanup arrays
    }

    private void renderPrimitive(RMPrimitive rmPrim, ref int primIndex)
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

        ++primIndex;
    }

    private void renderCSG(CSG csg, ref int primIndex, ref int csgIndex)
    {
        // TO-DO Don't let incomplete CSG children nodes be added to other CSGs.
        // Base case: Both nodes are primitives.
        if (csg.AllPrimNodes)
        {
            // Render both nodes.
            renderPrimitive(csg.FirstNode as RMPrimitive, ref primIndex);
            renderPrimitive(csg.SecondNode as RMPrimitive, ref primIndex);

            // Buffer this CSG.
            bufferedCSGs[csgIndex] = new Vector4(primIndex - 1, primIndex, -1, -1);
            combineOpsCSGs[csgIndex] = csg.CombineOp;
            ++csgIndex;
            return;
        }
        // Only first node is a primitive.
        else if (csg.IsFirstPrim)
        {
            // Recurse through second node (Must be a CSG).
            renderCSG(csg.SecondNode as CSG, ref primIndex, ref csgIndex);

            // Render first node.
            renderPrimitive(csg.FirstNode as RMPrimitive, ref primIndex);

            // Buffer this CSG.
            bufferedCSGs[csgIndex] = new Vector4(primIndex, -1, -1, csgIndex - 1);
            combineOpsCSGs[csgIndex] = csg.CombineOp;
            ++csgIndex;
            return;
        }
        // Only second node is a primitive.
        else if (csg.IsSecondPrim)
        {
            // Recurse through first node (Must be a csg).
            renderCSG(csg.FirstNode as CSG, ref primIndex, ref csgIndex);

            // Render second node.
            renderPrimitive(csg.SecondNode as RMPrimitive, ref primIndex);

            // Buffer this CSG.
            bufferedCSGs[csgIndex] = new Vector4(-1, primIndex, csgIndex - 1, -1);
            combineOpsCSGs[csgIndex] = csg.CombineOp;
            ++csgIndex;
            return;
        }
        // Both nodes are CSGs.
        else
        {
            Vector4 tempCSG = new Vector4(-1, -1, -1, -1);

            // Recurse through first node.
            renderCSG(csg.FirstNode as CSG, ref primIndex, ref csgIndex);
            tempCSG.z = csgIndex;

            // Recurse through second node.
            renderCSG(csg.SecondNode as CSG, ref primIndex, ref csgIndex);
            tempCSG.w = csgIndex;

            // Buffer this CSG.
            bufferedCSGs[csgIndex] = tempCSG;
            combineOpsCSGs[csgIndex] = csg.CombineOp;
            ++csgIndex;
            return;
        }
    }

    /// <summary>
    /// Custom version of Graphics.Blit that encodes frustum indices into the input vertices.
    /// 
    /// Top Left vertex:        z=0, u=0, v=0
    /// Top Right vertex:       z=1, u=1, v=0
    /// Bottom Right vertex:    z=2, u=1, v=1
    /// Bottom Left vertex:     z=3, u=1, v=0
    /// </summary>
    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNum)
    {
        RenderTexture.active = dest;

        fxMaterial.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        fxMaterial.SetPass(passNum);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);   // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);   // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);   // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);   // TL

        GL.End();
        GL.PopMatrix();

    }


    /// <summary>
    /// Stores the normalized rays representing the camera frustum in a 4x4 matrix. Each row is a vector.
    /// 
    /// The following rays are stored in each row (in eyespace, not worldspace):
    /// Top Left corner:        row=0
    /// Top Right corner:       row=1
    /// Bottom Right corner:    row=2
    /// Bottom Left  corner:    row=3
    /// </summary>
    /// <param name="cam">The camera to calculate the frustum corner rays.</param>
    /// <returns>A 4x4 matrix containing the 4 corner frustum rays.</returns>
    private Matrix4x4 GetFrustumCorners(Camera cam)
    {
        float camFov = cam.fieldOfView;
        float camAspect = cam.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovWHalf = camFov * 0.5f;

        float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 toRight = Vector3.right * tan_fov * camAspect;
        Vector3 toTop = Vector3.up * tan_fov;

        Vector3 topLeft = (-Vector3.forward - toRight + toTop);
        Vector3 topRight = (-Vector3.forward + toRight + toTop);
        Vector3 bottomRight = (-Vector3.forward + toRight - toTop);
        Vector3 bottomLeft = (-Vector3.forward - toRight - toTop);

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        return frustumCorners;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(RayMarcher))]
public class RayMarcherEditor : Editor
{



    private void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var rayMarcher = target as RayMarcher;

        serializedObject.Update();



        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Reload shader"))
        {
            //rayMarcher.EffectMaterial.shader = rayMarcher.EffectShader;
            rayMarcher.EffectMaterial = new Material(rayMarcher.EffectShader);
            rayMarcher.EffectMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
#endif