using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CombineOpsTypes
{
    Union,
    Subtraction,
    Intersection,
    SmoothUnion,
    SmoothSubtraction,
    SmoothIntersection,
    AbsUnion
}

public enum PrimitiveTypes
{
    Sphere,
    Box,
    RoundBox,
    Torus,
    CappedTorus,
    Link,
    Cylinder,
    CappedCylinder,
    CappedCylinderSlower,
    RoundedCylinder,
    Cone,
    CappedCone,
    RoundCone,
    Plane,
    HexagonalPrism,
    TriangularPrism,
    Capsule,
    VerticalCapsule,
    SolidAngle,
    Ellipsoid,
    Octahedron,
    OctahedronBound,
    Triangle,
    Quad,
    Tetrahedron,
    Mandelbulb
}

[AddComponentMenu("Ray Marching/RMPrimitive")]
[DisallowMultipleComponent]
public class RMPrimitive : RMObj
{
    [SerializeField]
    private Color _colour = Color.white;
    [SerializeField]
    private Vector4 _combineOp;

    [SerializeField]
    private CombineOpsTypes _combineOpType = CombineOpsTypes.Union;

    [SerializeField]
    private float _combineSmoothness = 0.0f;

    [SerializeField]
    private PrimitiveTypes _primitiveType = PrimitiveTypes.Sphere;

    [SerializeField]
    private bool _csgNode = false;

    [SerializeField]
    public Vector4 _geoInfo = new Vector4(1.0f, 1.0f, 1.0f);

    [SerializeField]
    [Range(0.0f, 3.0f)]
    private int _reflectionCount = 0;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float _reflectionIntensity = 0.0f;

    [SerializeField]
    [Range(0.0f, 3.0f)]
    private int _refractionCount = 0;

    [SerializeField]
    [Range(1.0f, 5.0f)]
    private float _refractionIndex = 1.0f;

    [SerializeField]
    private Texture2D _texture = null;

    [SerializeField]
    private bool _static = false;


    public Color Colour
    {
        get
        {
            return _colour;
        }
    }

    public Vector4 CombineOp
    {
        get
        {
            return new Vector4((float)_combineOpType, _combineSmoothness, 0.0f, 0.0f);
        }
    }

    public float CombineSmoothness
    {
        get
        {
            return _combineSmoothness;
        }
        set
        {
            _combineSmoothness = value;
        }
    }

    public PrimitiveTypes PrimitiveType
    {
        get
        {
            return _primitiveType;
        }
        set
        {
            _primitiveType = value;
        }
    }

    public CombineOpsTypes CombineOpType
    {
        get
        {
            return _combineOpType;
        }
    }

    public bool CSGNode
    {
        get
        {
            return _csgNode;
        }
        set
        {
            _csgNode = value;
        }
    }

    public Vector4 GeoInfo
    {
        get
        {
            return _geoInfo;
        }
        set
        {
            _geoInfo = value;
        }
    }

    public int ReflectionCount
    {
        get
        {
            return _reflectionCount;
        }
    }

    public float ReflectionIntensity
    {
        get
        {
           return _reflectionIntensity;
        }
    }

    public Vector4 ReflectionInfo
    {
        get
        {
            Vector4 info = new Vector4();
            switch (_reflectionCount)
            {
                case 0:
                    break;
                case 1:
                    info.x = 1.0f;
                    break;
                case 2:
                    info.x = 1.0f;
                    info.y = 1.0f;
                    break;
                case 3:
                    info.x = 1.0f;
                    info.y = 1.0f;
                    info.z = 1.0f;
                    break;
            }

            info.w = _reflectionIntensity;

            return info;
        }
    }

    public int RefractionCount
    {
        get
        {
            return _refractionCount;
        }
    }

    public float RefractionIndex
    {
        get
        {
            return _refractionIndex;
        }
    }

    public Vector2 RefractionInfo
    {
        get
        {
            return new Vector2(_refractionCount, _refractionIndex);
        }
    }

    public bool Static
    {
        get
        {
            return _static;
        }
    }


    //private void Awake()
    //{
    //    _drawOrder = 0;
    //}

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    private void OnDestroy()
    {
        //RMMemoryManager.Instance.reclaimRMPrimitive(this);
        // TO-DO Clean lists and arrays
    }

    public void loadSavedObj(RMPrimitive savedObj)
    {
        _primitiveType = savedObj._primitiveType;
        _colour = savedObj._colour;
        _combineOpType = savedObj._combineOpType;
        _combineSmoothness = savedObj._combineSmoothness;
        _csgNode = savedObj._csgNode;
        _geoInfo = savedObj._geoInfo;
    }

    public void resetPrim()
    {
        _primitiveType = PrimitiveTypes.Sphere;
        _colour = Color.white;
        _combineOpType = CombineOpsTypes.Union;
        _combineSmoothness = 0.0f;
        _csgNode = false;
        _geoInfo = new Vector4(1.0f, 1.0f, 1.0f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RMPrimitive))]
[CanEditMultipleObjects]
public class RMComponentEditor : RMObjEditor
{
    SerializedProperty _primitiveType;
    SerializedProperty _combineOpType;
    SerializedProperty _colour;
    SerializedProperty _combineSmoothness;
    SerializedProperty _geoInfo;
    //SerializedProperty _csgNode;
    private SerializedProperty _reflectionCount;
    private SerializedProperty _reflectionIntensity;
    private SerializedProperty _refractionCount;
    private SerializedProperty _refractionIndex;
    private SerializedProperty _texture;
    private SerializedProperty _static;


    protected override void OnEnable()
    {
        base.OnEnable();
        _primitiveType = serializedObject.FindProperty("_primitiveType");
        _combineOpType = serializedObject.FindProperty("_combineOpType");
        _colour = serializedObject.FindProperty("_colour");
        _combineSmoothness = serializedObject.FindProperty("_combineSmoothness");
        _geoInfo = serializedObject.FindProperty("_geoInfo");
        //_csgNode = serializedObject.FindProperty("_csgNode");
        _reflectionCount = serializedObject.FindProperty("_reflectionCount");
        _reflectionIntensity = serializedObject.FindProperty("_reflectionIntensity");
        _refractionCount = serializedObject.FindProperty("_refractionCount");
        _refractionIndex = serializedObject.FindProperty("_refractionIndex");
        _texture = serializedObject.FindProperty("_texture");
        _static = serializedObject.FindProperty("_static");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var rmComp = target as RMPrimitive;
        bool primTypeChanged = false;
        bool combineOpChanged = false;

        serializedObject.Update();

        EditorGUILayout.PropertyField(_static);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_primitiveType);
        primTypeChanged = EditorGUI.EndChangeCheck();

        EditorGUILayout.PropertyField(_colour);


        // Display different gemotric properties based on the chosen primitive type.
        Vector4 geoInfo = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        DisplayGeoInfo(rmComp, rmComp.PrimitiveType, geoInfo);


        EditorGUI.BeginDisabledGroup(rmComp.CSGNode);

        GUIContent label = new GUIContent("Combine Op");

        if (rmComp.CSGNode)
            label.tooltip = "Overridden by CSG.";
        else
            label.tooltip = "How the primitive will interact with the scene.";

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_combineOpType, label);
        combineOpChanged = EditorGUI.EndChangeCheck();

        label.text = "Combine Smoothness";
        //GUIContent label = new GUIContent("Combine Smoothness" ,"Overridden by CSG");

        if (_combineOpType.intValue == (int)CombineOpsTypes.SmoothUnion ||
            _combineOpType.intValue == (int)CombineOpsTypes.SmoothSubtraction ||
            _combineOpType.intValue == (int)CombineOpsTypes.SmoothIntersection)
        {
            EditorGUILayout.PropertyField(_combineSmoothness);
            rmComp.CombineSmoothness = Mathf.Clamp(_combineSmoothness.floatValue, 0.0f, Mathf.Infinity);
        }

        EditorGUI.EndDisabledGroup();


        // Reflection
        label.text = "Reflection Count";
        label.tooltip = "The amount of reflection rays this object will cast.";

        EditorGUILayout.PropertyField(_texture);
        EditorGUILayout.PropertyField(_reflectionCount, label);

        label.text = "Reflection Intensity";
        label.tooltip = "The intensity of the reflection.";

        EditorGUILayout.PropertyField(_reflectionIntensity, label);


        // Refraction
        label.text = "Refraction Count";
        label.tooltip = "The amount of refraction rays this object will cast.";

        EditorGUILayout.PropertyField(_refractionCount, label);

        label.text = "Refraction Index";
        label.tooltip = "Determines how much light bends when travelling through this medium.";

        EditorGUILayout.PropertyField(_refractionIndex, label);


        serializedObject.ApplyModifiedProperties();


        // Update and reload the shader.
        //if (primTypeChanged)
        //    Camera.main.GetComponent<ShaderBuilder>().build();

        //if (combineOpChanged)
        //    Camera.main.GetComponent<ShaderBuilder>().build();
    }

    void DisplayGeoInfo(RMPrimitive rmComp, PrimitiveTypes type, Vector4 geoInfo)
    {
        switch (type)
        {
            case PrimitiveTypes.Sphere:
                geoInfo.x = EditorGUILayout.FloatField("Radius", _geoInfo.vector4Value.x);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Box:
                geoInfo.x = EditorGUILayout.FloatField("Length", _geoInfo.vector4Value.x);
                geoInfo.z = EditorGUILayout.FloatField("Breadth", _geoInfo.vector4Value.z);
                geoInfo.y = EditorGUILayout.FloatField("Height", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.RoundBox:
                geoInfo.x = EditorGUILayout.FloatField("Length", _geoInfo.vector4Value.x);
                geoInfo.z = EditorGUILayout.FloatField("Breadth", _geoInfo.vector4Value.z);
                geoInfo.y = EditorGUILayout.FloatField("Height", _geoInfo.vector4Value.y);
                geoInfo.w = EditorGUILayout.FloatField("Roundness", _geoInfo.vector4Value.w);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Torus:
                geoInfo.x = EditorGUILayout.FloatField("Radius", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Thickness", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.CappedTorus:
                geoInfo.x = EditorGUILayout.FloatField("Radius", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Thickness", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("RA", _geoInfo.vector4Value.z);
                geoInfo.w = EditorGUILayout.FloatField("RB", _geoInfo.vector4Value.w);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Link:
                geoInfo.x = EditorGUILayout.FloatField("Length", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("R1", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("R2", _geoInfo.vector4Value.z);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Cylinder:
                geoInfo.x = EditorGUILayout.FloatField("Radius", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Height", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.CappedCylinder:
                geoInfo.x = EditorGUILayout.FloatField("Radius", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Hieght", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.CappedCylinderSlower:
                geoInfo.x = EditorGUILayout.FloatField("Radius", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Hieght", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.RoundedCylinder:
                geoInfo.x = EditorGUILayout.FloatField("RA", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("RB", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("Hieght", _geoInfo.vector4Value.z);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Cone:
                geoInfo.x = EditorGUILayout.FloatField("Radius", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Hieght", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.CappedCone:
                geoInfo.x = EditorGUILayout.FloatField("H", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("R1", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("R2", _geoInfo.vector4Value.z);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.RoundCone:
                geoInfo.x = EditorGUILayout.FloatField("R1", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("R2", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("H", _geoInfo.vector4Value.z);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Plane:
                geoInfo.x = EditorGUILayout.FloatField("X", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Y", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("Z", _geoInfo.vector4Value.z);
                geoInfo.w = EditorGUILayout.FloatField("W", _geoInfo.vector4Value.w);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.HexagonalPrism:
                geoInfo.x = EditorGUILayout.FloatField("X", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Y", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.TriangularPrism:
                geoInfo.x = EditorGUILayout.FloatField("X", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Y", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Capsule:
                geoInfo.x = EditorGUILayout.FloatField("X", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Y", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("Z", _geoInfo.vector4Value.z);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.VerticalCapsule:
                geoInfo.x = EditorGUILayout.FloatField("Hieght", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Radius", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.SolidAngle:
                geoInfo.x = EditorGUILayout.FloatField("X", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Y", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("Z", _geoInfo.vector4Value.z);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Ellipsoid:
                geoInfo.x = EditorGUILayout.FloatField("X", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Y", _geoInfo.vector4Value.y);
                geoInfo.z = EditorGUILayout.FloatField("Z", _geoInfo.vector4Value.z);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Octahedron:
                geoInfo.x = EditorGUILayout.FloatField("S", _geoInfo.vector4Value.x);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.OctahedronBound:
                geoInfo.x = EditorGUILayout.FloatField("S", _geoInfo.vector4Value.x);
                _geoInfo.vector4Value = geoInfo;
                break;
            case PrimitiveTypes.Triangle:
                break;
            case PrimitiveTypes.Quad:
                break;
            case PrimitiveTypes.Tetrahedron:
                break;
            case PrimitiveTypes.Mandelbulb:
                geoInfo.x = EditorGUILayout.FloatField("Iterations", _geoInfo.vector4Value.x);
                geoInfo.y = EditorGUILayout.FloatField("Power", _geoInfo.vector4Value.y);
                _geoInfo.vector4Value = geoInfo;
                break;
            default:
                break;
        }
    }
}
#endif