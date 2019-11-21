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
    private List<RayMarchShader> _shaders = new List<RayMarchShader>();

    //[SerializeField]
    //private bool _showShaderWindow = false;

    [SerializeField]
    private Shader _effectShader = null;
    private Material _effectMaterial;
    private Camera _currentCamera;
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
    //private const int MAX_PRIMS = 128;
    //static private RMPrimitive[] _prims = new RMPrimitive[MAX_PRIMS];
    //static private List<RMPrimitive> _prims = new List<RMPrimitive>(MAX_PRIMS);
    //static private uint _currentObjs = 0;
    // ######### Ray Marcher Inspector Variables #########


    private Matrix4x4[] invModelMats = new Matrix4x4[32];
    private Color[] colours = new Color[32];
    //private float[] primitiveTypes = new float[32];
    private Vector4[] combineOps = new Vector4[32];
    private Vector4[] primitiveGeoInfo = new Vector4[32];
    private Vector4[] _reflInfo = new Vector4[32];
    private Vector4[] _refractInfo = new Vector4[32];
    private Vector4[] _altInfo = new Vector4[32];


    private Vector4[] bufferedCSGs = new Vector4[16];
    private Vector4[] combineOpsCSGs = new Vector4[16];
    //private float[] csgNodesPerRoot = new float[16];

    private Vector4[] _boundGeoInfo = new Vector4[32];


    RMObj[] objects;
    List<RMObj> objs = new List<RMObj>();

    public List<RMObj> RenderList
    {
        get
        {
            return objs;
        }
    }

    public List<RayMarchShader> Shaders
    {
        get
        {
            return _shaders;
        }
    }

    //public bool ShowShaderWindow
    //{
    //    get
    //    {
    //        return _showShaderWindow;
    //    }
    //    set
    //    {
    //        _showShaderWindow = value;
    //    }
    //}

    private void Start()
    {
        if (Application.isPlaying)
        {
            objects = FindObjectsOfType<RMObj>();
            objs = new List<RMObj>(objects);

            objs.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        }


        //RayMarchShader[] allShaders = GetComponents<RayMarchShader>();
        //RayMarchShader[] allShaders = (RayMarchShader[])AssetDatabase.LoadAllAssetsAtPath("Assets/Graphics Pipeline/Shader Objects/TestOne");
        //_shaders = new List<RayMarchShader>(allShaders);
    }


    public void addShader()
    {
        //RayMarchShader shader = gameObject.AddComponent<RayMarchShader>();
        //shader.hideFlags = HideFlags.HideInInspector;
        //shader.ShaderName = "New Shader";
        //_shaders.Add(shader);

        // RayMarchShader shader = ScriptableObject.CreateInstance<RayMarchShader>();
        // shader.ShaderName = "New Shader";
        // _shaders.Add(shader);

        // ShaderEditorWindow.rebuildNames();

        RayMarchShader shader = new RayMarchShader();
        _shaders.Add(shader);
    }

    public void removeShader(RayMarchShader shader)
    {

        // if (_shaders.Count > 0)
        // {
        //     int index = _shaders.IndexOf(shader);
        //     _shaders[index] = _shaders[_shaders.Count - 1];
        // }

        // _shaders.RemoveAt(_shaders.Count - 1);

        // ShaderEditorWindow.rebuildNames();
    }

    public void removeShader(int index)
    {

        // if (_shaders.Count > 0)
        // {
        //     _shaders[index] = _shaders[_shaders.Count - 1];
        // }

        // _shaders.RemoveAt(_shaders.Count - 1);

        // ShaderEditorWindow.rebuildNames();
    }

    public void removeAllShaders()
    {
        _shaders.Clear();
    }

    public void moveUp(RayMarchShader shader)
    {
        // int index = _shaders.IndexOf(shader);
        // RayMarchShader temp = _shaders[index];

        // ShaderEditorWindow.rebuildNames();
    }

    public void unHide()
    {
        // RayMarchShader[] shaders = GetComponents<RayMarchShader>();
        // Component[] allComps = GetComponents<Component>();

        // foreach(RayMarchShader shader in shaders)
        // {
        //     shader.hideFlags = HideFlags.None;
        // }
    }


    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!EffectMaterial)
        {
            Graphics.Blit(source, destination); // Do Nothing
            return;
        }
        //EffectMaterial.EnableKeyword("BOUND_DEBUG");  // TO-DO Perform this only when debug is enabled.
        //EffectMaterial.shaderKeywords = new string[1] { "BOUNDING_SPHERE_DEBUG" };
        //Matrix4x4 torusMat = Matrix4x4.TRS(
        //                                    Vector3.right * Mathf.Sin(Time.time) * 5.0f,
        //                                    Quaternion.identity,
        //                                    Vector3.one);
        //torusMat *= Matrix4x4.TRS(
        //                           Vector3.zero,
        //                           Quaternion.Euler(new Vector3(0.0f, 0.0f, (Time.time * 200.0f) % 360.0f)),
        //                           Vector3.one);

        //EffectMaterial.SetMatrix("_TorusMat_InvModel", torusMat.inverse);


        Matrix4x4 frustomCorners = GetFrustumCorners(CurrentCamera);
        Matrix4x4 cameraInvViewMatrix = CurrentCamera.cameraToWorldMatrix;
        Vector3 camPos = CurrentCamera.transform.position;
        
        
        // Render all shaders.
        foreach (RayMarchShader shader in _shaders)
        {
            _effectMaterial.shader = shader.EffectShader;
            shader.render(_effectMaterial, frustomCorners, cameraInvViewMatrix, camPos);

            CustomGraphicsBlit(source, destination, EffectMaterial, 0);
        }
        


        //Graphics.Blit(source, destination, EffectMaterial, 0); // Use given effect shader as an image effect
        //CustomGraphicsBlit(source, destination, EffectMaterial, 0);
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
        //RenderTexture.active = dest;

        RenderTexture distanceMap = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RFloat);
        //RenderTexture sceneTex = new RenderTexture(source);
        RenderTexture sceneTex = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        RenderBuffer[] buffers = new RenderBuffer[2] { sceneTex.colorBuffer, distanceMap.colorBuffer };
        Graphics.SetRenderTarget(buffers, dest.depthBuffer);


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


        Graphics.Blit(distanceMap, dest);
        //Graphics.Blit(sceneTex, dest);

        //sceneTex.Release();
        //distanceMap.Release();
        RenderTexture.ReleaseTemporary(sceneTex);
        RenderTexture.ReleaseTemporary(distanceMap);
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


    [MenuItem("GameObject/Ray Marched/Sphere", false, 10)]
    static void CreateBox(MenuCommand menuCommand)
    {
        GameObject obj = new GameObject();
        obj.AddComponent<RMPrimitive>();

        // Ensure the obj gets parented if this was a context click (otherwise does nothing).
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(obj, obj.name);
    }

    [MenuItem("GameObject/Ray Marched/CSG", false, 10)]
    static void CreateCSG(MenuCommand menuCommand)
    {
        GameObject obj = new GameObject();
        obj.AddComponent<CSG>();

        // Ensure the obj gets parented if this was a context click (otherwise does nothing).
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(obj, obj.name);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RayMarcher))]
public class RayMarcherEditor : Editor
{

    private bool _boundDebug = false;

    private List<RayMarchShader> shaders;
    private List<RMObj> _renderList;
    private bool _renderListFoldout = false;
    private SerializedProperty _shaders;

    private void OnEnable()
    {
        _shaders = serializedObject.FindProperty("_shaders");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        var rayMarcher = target as RayMarcher;

        serializedObject.Update();


        GUIContent label = new GUIContent("Shaders", "");
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (GUILayout.Button("Remove All Shaders"))
        {
            rayMarcher.removeAllShaders();
        }

        shaders = rayMarcher.Shaders;
        for (int i = 0; i < shaders.Count; ++i)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //string textBuffer = shaders[i].ShaderName;
            //shaders[i].ShaderName = EditorGUILayout.TextField(textBuffer);
            //shaders[i].ShaderName = EditorGUILayout.TextField(shaders[i].ShaderName);

            //EditorGUILayout.TextField(shaders[i].ShaderName);
            label.text = "Settings";
            label.tooltip = "";
            shaders[i].Settings = EditorGUILayout.ObjectField(label, shaders[i].Settings, typeof(RayMarchShaderSettings), true) as RayMarchShaderSettings;

            label.text = "Render List";
            _renderListFoldout = EditorGUILayout.Foldout(_renderListFoldout, "Render List");
            if (_renderListFoldout)
            {
                _renderList = shaders[i].RenderList;
                foreach (RMObj obj in _renderList)
                {
                    EditorGUILayout.TextField(obj.name);
                }
            }

           GUILayout.BeginHorizontal();
           if (GUILayout.Button("Remove Shader"))
           {
               rayMarcher.removeShader(i);
               break;
           }
           GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (GUILayout.Button("Add Shader"))
        {
           rayMarcher.addShader();
        }





        //if (GUILayout.Button("Shader Editor"))
        //{
        //    //rayMarcher.ShowShaderWindow = !rayMarcher.ShowShaderWindow;
        //    ShaderEditorWindow.Init();
        //}



        serializedObject.ApplyModifiedProperties();


        //if (GUILayout.Button("Bound Debug"))
        //{
        //    _boundDebug = !_boundDebug;

        //    if (_boundDebug)
        //        rayMarcher.EffectMaterial.EnableKeyword("BOUND_DEBUG");
        //    else
        //        rayMarcher.EffectMaterial.DisableKeyword("BOUND_DEBUG");
        //}
    }
}


/* public class ShaderEditorWindow : EditorWindow
{
    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;
    static Camera _camera;
    static RayMarcher _rayMarcher;
    static List<RayMarchShader> _shaders;

    static List<string> _toolbarNames = new List<string>();
    static int _toolbarSelected = 0;
    Vector2 scrollPos;


    // Add menu named "Shader Editor" to the Window menu
    [MenuItem("Window/Shader Editor")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        ShaderEditorWindow window = (ShaderEditorWindow)EditorWindow.GetWindow(typeof(ShaderEditorWindow));
        window.Show();

        _camera = Camera.main;
        _rayMarcher = _camera.GetComponent<RayMarcher>();
    }

    public static void rebuildNames()
    {
        _toolbarNames.Clear();

        _shaders = _rayMarcher.Shaders;

        foreach (RayMarchShader shader in _shaders)
        {
            _toolbarNames.Add(shader.ShaderName);
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        _toolbarSelected = GUILayout.Toolbar(_toolbarSelected, _toolbarNames.ToArray());
        GUILayout.EndHorizontal();


        scrollPos = GUILayout.BeginScrollView(scrollPos);


        GUIContent label = new GUIContent();

        _shaders = _rayMarcher.Shaders;
        foreach (RayMarchShader shader in _shaders)
        {
            // ######### General Variables #########
            label.text = "General Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Shader Name";
            shader.ShaderName = EditorGUILayout.TextField(label, shader.ShaderName);

            label.text = "Max Steps";
            label.tooltip = "The maximum number of steps each ray can take.";
            shader.MaxSteps = EditorGUILayout.IntField(label, shader.MaxSteps);

            label.text = "Max Draw Dist";
            label.tooltip = "The maximum distance each pixel can travel.";
            shader.MaxDrawDist = EditorGUILayout.FloatField(label, shader.MaxDrawDist);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Light Variables #########
            label.text = "Light Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Specular Exp";
            label.tooltip = "Affects the size of the specular highlight";
            shader.SpecularExp = EditorGUILayout.FloatField(label, shader.SpecularExp);

            label.text = "Attenuation Constant";
            label.tooltip = "";
            shader.AttenuationConstant = EditorGUILayout.FloatField(label, shader.AttenuationConstant);

            label.text = "Attenuation Linear";
            label.tooltip = "";
            shader.AttenuationLinear = EditorGUILayout.FloatField(label, shader.AttenuationLinear);

            label.text = "Attenuation Quadratic";
            label.tooltip = "";
            shader.AttenuationQuadratic = EditorGUILayout.FloatField(label, shader.AttenuationQuadratic);

            label.text = "Ambient Colour";
            label.tooltip = "";
            shader.AmbientColour = EditorGUILayout.ColorField(label, shader.AmbientColour);

            label.text = "Diffuse Colour";
            label.tooltip = "";
            shader.DiffuseColour = EditorGUILayout.ColorField(label, shader.DiffuseColour);

            label.text = "Specular Colour";
            label.tooltip = "";
            shader.SpecualarColour = EditorGUILayout.ColorField(label, shader.SpecualarColour);

            label.text = "Light Constants";
            label.tooltip = "";
            shader.LightConstants = EditorGUILayout.Vector3Field(label, shader.LightConstants);

            label.text = "Rim Light Colour";
            label.tooltip = "";
            shader.RimLightColour = EditorGUILayout.ColorField(label, shader.RimLightColour);

            label.text = "Sun Light";
            label.tooltip = "Directional light representing the sun.";
            shader.sunLight = EditorGUILayout.ObjectField(label, shader.sunLight, typeof(Transform), true) as Transform;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Shadow Variables #########
            label.text = "Shadow Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Penumbra Factor";
            label.tooltip = "How soft the shadows appear, the further away they are from the occluder.";
            shader.PenumbraFactor = EditorGUILayout.FloatField(label, shader.PenumbraFactor);

            label.text = "Shadow Min Dist";
            label.tooltip = "A bias to prevent the shadow rays from getting stuck inside of their origin surface.";
            shader.ShadowmMinDist = EditorGUILayout.FloatField(label, shader.ShadowmMinDist);

            label.text = "Shadow Intensity";
            label.tooltip = "How strong the shadows appear.";
            shader.ShadowIntensity = EditorGUILayout.FloatField(label, shader.ShadowIntensity);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Reflection Variables #########
            label.text = "Reflection Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Reflection Count";
            label.tooltip = "The maximum amount of reflection rays sllowed.";
            shader.ReflectionCount = EditorGUILayout.IntField(label, shader.ReflectionCount);

            label.text = "Reflection Intensity";
            label.tooltip = "The strength of the reflection.";
            shader.ReflectionIntensity = EditorGUILayout.FloatField(label, shader.ReflectionIntensity);

            label.text = "Env Refl Intensity";
            label.tooltip = "The strength of the environment (skybox) reflection.";
            shader.ReflectionIntensity = EditorGUILayout.FloatField(label, shader.ReflectionIntensity);

            label.text = "Skybox";
            label.tooltip = "";
            shader.SkyBox = EditorGUILayout.ObjectField(label, shader.SkyBox, typeof(Texture), true) as Texture;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Ambient Occlusion Variables #########
            label.text = "Ambient Occlusion Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "AO Max Steps";
            label.tooltip = "The maximum number of steps each AO ray can take.";
            shader.AOMaxSteps = EditorGUILayout.IntField(label, shader.AOMaxSteps);

            label.text = "AO Step Size";
            label.tooltip = "The size of each step an AO ray marches.";
            shader.AOStepSize = EditorGUILayout.FloatField(label, shader.AOStepSize);

            label.text = "AO Intensity";
            label.tooltip = "The intensity of the AO effect.";
            shader.AOItensity = EditorGUILayout.FloatField(label, shader.AOItensity);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Vignette Variables #########
            label.text = "Vignette Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Vignette Intensity";
            label.tooltip = "";
            shader.VignetteIntesnity = EditorGUILayout.FloatField(label, shader.VignetteIntesnity);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Fog Variables #########
            label.text = "Fog Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Fog Extinction";
            label.tooltip = "TO-DO";
            shader.FogExtinction = EditorGUILayout.FloatField(label, shader.FogExtinction);

            label.text = "Fog Inscattering";
            label.tooltip = "TO-DO";
            shader.FogInscattering = EditorGUILayout.FloatField(label, shader.FogInscattering);

            label.text = "Fog Colour";
            label.tooltip = "";
            shader.FogColour = EditorGUILayout.ColorField(label, shader.FogColour);

            GUILayout.EndScrollView();
        }
    }
} */
#endif