using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif
using System;
using UnityEngine.Events;

[ExecuteInEditMode]
[AddComponentMenu("Ray Marching/RayMarcher")]
[DisallowMultipleComponent]
public class RayMarcher : MonoBehaviour
{
    [SerializeField]
    private List<RayMarchShader> _shaders = new List<RayMarchShader>();

    [SerializeField]
    private Transform _sunLight;

    private static RayMarcher _instance;

    [SerializeField]
    private RenderTexture _distTex;


    [SerializeField]
    ComputeShader _collisionCompute;
    [SerializeField]
    RenderTexture _computeTex;

    struct ColliderInfo
    {
        public Vector3 pos;
        public Vector4 geoInfo;
        public int colliding;
    }

    public static RayMarcher Instance
    {
        get
        {

            if (!_instance)
            {
                _instance = GameObject.Find("RayMarch Manager").GetComponent<RayMarcher>();
            }

            return _instance;
        }
    }

    //public Material EffectMaterial
    //{
    //    get
    //    {
    //        if (!_effectMaterial)
    //        {
    //            _effectMaterial = new Material(Shader.Find("Standard"));
    //            _effectMaterial.hideFlags = HideFlags.HideAndDontSave;
    //        }

    //        return _effectMaterial;
    //    }
    //    set
    //    {
    //        _effectMaterial = value;
    //    }
    //}

    public Transform SunLight
    {
        get
        {
            return _sunLight;
        }
    }




    


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

    private void Start()
    {
        _shaders = new List<RayMarchShader>(GetComponents<RayMarchShader>());
        _distTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            //RenderTexture tex = new RenderTexture(256, 256, 24);
            //tex.enableRandomWrite = true;
            //tex.Create();


            // Get all ray march colliders.
            RMCollider[] _colliders = (RMCollider[])FindObjectsOfType(typeof(RMCollider));



            // Extract all collider info, and pack into float array.
            ColliderInfo[] colliderInfos = new ColliderInfo[_colliders.Length];
            Matrix4x4[] invModelMats = new Matrix4x4[_colliders.Length];
            int[] primitiveTypes = new int[_colliders.Length];
            Vector4[] combineOps = new Vector4[_colliders.Length];
            Vector4[] primitiveGeoInfo = new Vector4[_colliders.Length];

            ColliderInfo colInfo;
            GameObject obj;
            for (int i = 0; i < _colliders.Length; ++i)
            {
                obj = _colliders[i].gameObject;

                colInfo.pos = obj.transform.position;
                colInfo.geoInfo = obj.GetComponent<RMPrimitive>().GeoInfo;
                colInfo.colliding = -1;

                colliderInfos[i] = colInfo;


                invModelMats[i] = obj.transform.localToWorldMatrix.inverse;
                primitiveTypes[i] = (int)obj.GetComponent<RMPrimitive>().PrimitiveType;
                combineOps[i] = obj.GetComponent<RMPrimitive>().CombineOp;
                primitiveGeoInfo[i] = obj.GetComponent<RMPrimitive>().GeoInfo;


            }


            int kernel = _collisionCompute.FindKernel("CSMain");


            // Create a compute buffer.
            ComputeBuffer buffer = new ComputeBuffer(colliderInfos.Length, (sizeof(float) * 7) + sizeof(int));
            buffer.SetData(colliderInfos);
            _collisionCompute.SetBuffer(kernel, "_colliderInfo", buffer);
            //_collisionCompute.SetTexture(0, "Result", tex);

            _collisionCompute.SetMatrixArray("_invModelMats", invModelMats);
            _collisionCompute.SetInts("_primitiveTypes", primitiveTypes);
            _collisionCompute.SetVectorArray("_combineOps", combineOps);
            _collisionCompute.SetVectorArray("_primitiveGeoInfo", primitiveGeoInfo);

            int numThreadGroups = _colliders.Length;
            _collisionCompute.Dispatch(kernel, 1, 1, 1);
            //_collisionCompute.Dispatch(0, 256/8, 256/8, 1);


            buffer.GetData(colliderInfos);

            Debug.Log("Dist: " + colliderInfos[0].geoInfo.w);

            if (colliderInfos[0].colliding == 1)
            {
                Debug.Log("Colliding");

                _colliders[0].gameObject.transform.position = colliderInfos[0].pos;
            }



            //Graphics.CopyTexture(tex, _computeTex);

            buffer.Release();
            //tex.Release();
        }
    }


    public void addShader(ShaderType shaderType)
    {
        // RayMarchShader shader = ScriptableObject.CreateInstance<RayMarchShader>();
        // shader.ShaderName = "New Shader";
        // _shaders.Add(shader);


        RayMarchShader shader;

        switch (shaderType)
        {
            case ShaderType.Rendering:
                shader = gameObject.AddComponent<RMRenderShader>();
                break;
            case ShaderType.MarchingCube:
                shader = gameObject.AddComponent<RMMarchingCubeShader>();
                shader.ShaderType = ShaderType.MarchingCube;
                break;
            case ShaderType.Collision:
                return;
            default:
                return;
        }

        _shaders.Add(shader);
    }

    public void removeShader(RayMarchShader shader)
    {
        //int index = _shaders.IndexOf(shader);
        //_shaders[index] = _shaders[_shaders.Count - 1];
        //_shaders.RemoveAt(_shaders.Count - 1);

        _shaders.Remove(shader);
    }

    public void removeShader(int index)
    {
        //_shaders[index] = _shaders[_shaders.Count - 1];
        //_shaders.RemoveAt(_shaders.Count - 1);

        _shaders.RemoveAt(index);
    }

    public void removeAllShaders()
    {
        foreach (RayMarchShader shader in _shaders)
        {
#if UNITY_EDITOR
            DestroyImmediate(shader);
#else
            Destroy(shader);
#endif
        }

        _shaders.Clear();
    }

    public void moveUp(RayMarchShader shader)
    {
        // int index = _shaders.IndexOf(shader);
        // RayMarchShader temp = _shaders[index];
    }


    public void render(RenderTexture source, RenderTexture destination)
    {
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


        if (!_distTex)
        {
            _distTex = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.RFloat);
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
    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNum, bool finalPass, ref RenderTexture distTex)
    {
        //RenderTexture.active = dest;

        RenderTexture distanceMap = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RFloat);
        RenderTexture sceneTex = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        RenderBuffer[] buffers = new RenderBuffer[2] { sceneTex.colorBuffer, distanceMap.colorBuffer };
        //RenderBuffer[] buffers = new RenderBuffer[2] { sceneTex.colorBuffer, distTex.colorBuffer };
        Graphics.SetRenderTarget(buffers, dest.depthBuffer);


        fxMaterial.SetTexture("_MainTex", source);

        if (fxMaterial.IsKeywordEnabled("USE_DIST_TEX"))
        {
            fxMaterial.SetTexture("_distTex", distTex);
        }



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


        //Graphics.Blit(distanceMap, dest);

        if (finalPass)
        {
            Graphics.Blit(sceneTex, dest);
        }
        else
        {
            Graphics.Blit(sceneTex, source);
        }

        Graphics.Blit(distanceMap, distTex);

        //if (distTex)
        //{
        //    distTex.Release();
        //    distTex = new RenderTexture(distanceMap);
        //}

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


#if UNITY_EDITOR
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
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RayMarcher))]
public class RayMarcherEditor : Editor
{
    List<RayMarchShader> shaders;
    SerializedProperty _shaders;
    SerializedProperty _sunLight;
    SerializedProperty _distTex;
    SerializedProperty _collisionCompute;
    SerializedProperty _computeTex;
    ShaderType _shaderType = ShaderType.Rendering;
    int _selectedShaderIndex = 0;

    private void OnEnable()
    {
        _shaders = serializedObject.FindProperty("_shaders");
        _sunLight = serializedObject.FindProperty("_sunLight");
        _distTex = serializedObject.FindProperty("_distTex");
        _collisionCompute = serializedObject.FindProperty("_collisionCompute");
        _computeTex = serializedObject.FindProperty("_computeTex");
    }



    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        var rayMarcher = target as RayMarcher;

        serializedObject.Update();

        EditorGUILayout.PropertyField(_distTex);
        EditorGUILayout.PropertyField(_sunLight);
        EditorGUILayout.PropertyField(_collisionCompute);
        EditorGUILayout.PropertyField(_computeTex);


        EditorGUILayout.Space(6.0f);
        GUIContent label = new GUIContent("Shaders", "");
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        label.text = "Shader Type";
        _shaderType = (ShaderType)EditorGUILayout.EnumPopup(label, _shaderType);

        // Add a another shader to this RayMarcher.
        if (GUILayout.Button("Add Shader"))
        {
            rayMarcher.addShader(_shaderType);
        }

        // Remove all shaders from this RayMarcher.
        EditorGUILayout.Space(4.0f);
        if (GUILayout.Button("Remove All Shaders"))
        {
            rayMarcher.removeAllShaders();
        }
        EditorGUILayout.Space(10.0f);

        // Display all shaders.
        shaders = rayMarcher.Shaders;
        for (int i = 0; i < shaders.Count; ++i)
        {
            //EditorGUILayout.Space(2.0f);

            if (GUILayout.Button(shaders[i].ShaderName))
            {
                ShaderEditorWindow.Init(shaders[i]);
            }
        }



        serializedObject.ApplyModifiedProperties();

    }
}

public class ShaderEditorWindow : EditorWindow
{
    static Camera _camera;
    static List<System.Type> _desiredDockNextTo = new List<System.Type>();


    static RayMarcher _rayMarcher;


    Vector2 shaderScrollPos;
    Vector2 renderListScrollPos;
    bool _renderListFoldout = false;
    RayMarchShader _shader;
    //event Action<RMObj> _onRemoveObj;
    //event Action _onRemoveShader;

    // Add menu named "Shader Editor" to the Window menu
    //[MenuItem("Window/Shader Editor")]
    //public static void Init()
    //{
    //    // Get existing open window or if none, make a new one:
    //    //ShaderEditorWindow window = (ShaderEditorWindow)EditorWindow.GetWindow(typeof(ShaderEditorWindow));
    //    //window.Show();

    //    ShaderEditorWindow window = CreateWindow<ShaderEditorWindow>(_desiredDockNextTo.ToArray());
    //    window.Show();
    //    _desiredDockNextTo.Add(window.GetType());

    //    _rayMarcher = RayMarcher.Instance;
    //}

    public static void Init(RayMarchShader shader)
    {
        // Get existing open window or if none, make a new one:
        //ShaderEditorWindow window = (ShaderEditorWindow)EditorWindow.GetWindow(typeof(ShaderEditorWindow));
        //window.Show();

        ShaderEditorWindow window = CreateWindow<ShaderEditorWindow>(_desiredDockNextTo.ToArray());
        window.Show();
        _desiredDockNextTo.Add(window.GetType());
        window._shader = shader;

        window.titleContent = new GUIContent(shader.ShaderName);

        _rayMarcher = RayMarcher.Instance;

        //window._onRemoveObj += window.removeObj;
        //window._onRemoveShader += window.removeShader;
    }


    void OnGUI()
    {
        shaderScrollPos = GUILayout.BeginScrollView(shaderScrollPos);


        GUIContent label = new GUIContent();


        // Display the current shader's effect shader.
        if (_shader.ShaderType == ShaderType.Rendering)
        {
            label.text = "Effect Shader";
            label.tooltip = "";
            _shader.EffectShader = EditorGUILayout.ObjectField(label, _shader.EffectShader, typeof(Shader), true) as Shader;
        }
        else
        {
            label.text = "SDF To Mesh Shader";
            label.tooltip = "";
            (_shader as RMMarchingCubeShader).SDFtoMeshShader = EditorGUILayout.ObjectField(label, (_shader as RMMarchingCubeShader).SDFtoMeshShader, typeof(ComputeShader), true) as ComputeShader;
        }

        // Display the current shader's name.
        EditorGUILayout.BeginHorizontal();
        label.text = "Shader Name";
        EditorGUILayout.PrefixLabel(label);
        _shader.ShaderName = EditorGUILayout.TextField(_shader.ShaderName);
        EditorGUILayout.EndHorizontal();

        // Settings retrieved from a scriptable object.
        if (_shader.ShaderType == ShaderType.Rendering)
        {
            label.text = "Settings";
            label.tooltip = "";
            _shader.Settings = EditorGUILayout.ObjectField(label, _shader.Settings, typeof(RayMarchShaderSettings), true) as RayMarchShaderSettings;
        }

        // Objects in the current shader's render list.
        label.text = "Render List";
        _renderListFoldout = EditorGUILayout.Foldout(_renderListFoldout, label, true);
        List<RMObj> _renderList;
        renderListScrollPos = EditorGUILayout.BeginScrollView(renderListScrollPos);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(6.0f);

        if (_renderListFoldout)
        {
            _renderList = _shader.RenderList;
            foreach (RMObj obj in _renderList)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(obj.name, EditorStyles.centeredGreyMiniLabel);

                if (GUILayout.Button("Remove"))
                {
                    //_onRemoveObj.Invoke(obj);
                    removeObj(obj);
                    return;
                }

                EditorGUILayout.Space(2.0f);

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(6.0f);
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();


        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Remove Shader"))
        {
            //_onRemoveShader.Invoke();
            removeShader();
            return;
        }
    }

    //private void OnDestroy()
    //{
    //    _onRemoveObj -= removeObj;
    //    _onRemoveShader -= removeShader;
    //}

    private void removeObj(RMObj rmObj)
    {
        //_shader.removeFromRenderList(rmObj);
        //rmObj.removeFromShaderList(_shader);
        rmObj.remove();
    }

    private void removeShader()
    {
        RayMarcher.Instance.removeShader(_shader);
        //_shader.removeAllFromRenderList();
        _shader.remove();

        // Delete the shader.
#if UNITY_EDITOR
        DestroyImmediate(_shader);
#else
        Destroy(_shader);
#endif
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