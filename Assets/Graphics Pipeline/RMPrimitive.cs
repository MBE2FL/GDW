using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
//using System.Runtime.InteropServices;

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

public enum AlterationTypes
{
    Elongate1D,
    Elongate,
    Round,
    Onion,
    SymX,
    SymXZ,
    RepXZ,
    RepFinite,
    Twist,
    Displace,
    Bend,
    Custom
}

[System.Serializable]
public struct Alteration
{
    public AlterationTypes type;
    public Vector4 info;
    public bool posAlt;
    public string command;
    public bool active;
    public int order;
}

//[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
//public struct Test
//{
//    public int num;

//    public Vector4 vec;

//    [MarshalAs(UnmanagedType.U1)]
//    public bool boolie;

//    public AlterationTypes type;

//    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
//    public string msg;
//}



[AddComponentMenu("Ray Marching/RMPrimitive")]
[DisallowMultipleComponent]
[ExecuteInEditMode]
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

    //[SerializeField]
    //private Texture2D _texture = null;

    [SerializeField]
    private bool _static = false;



    [Header("Alterations")]
    [SerializeField]
    private List<Alteration> _alterations = new List<Alteration>();

    public Alteration alt1;
    public Alteration alt2;
    public Alteration alt3;
    public Alteration alt4;
    public Alteration alt5;
    public Alteration alt6;

    private bool _altsDirty = false;



    [SerializeField]
    private string _displaceFormula = "sin(c.x * pos.x) * sin(c.y * pos.y) * sin(c.z * pos.z);";


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

    //public List<AlterationTypes> AlterationTypes
    //{
    //    get
    //    {
    //        return _alterationTypes;
    //    }
    //}

    //public List<Vector4> AlterationInfo
    //{
    //    get
    //    {
    //        return _alterationInfo;
    //    }
    //}

    //public List<string> AltCustomInfo
    //{
    //    get
    //    {
    //        return _altCustomInfo;
    //    }
    //}

    public List<Alteration> Alterations
    {
        get
        {
                _alterations.Clear();

            // Add all active alterations.
            if (alt1.active)
                _alterations.Add(alt1);
            if (alt2.active)
                _alterations.Add(alt2);
            if (alt3.active)
                _alterations.Add(alt3);
            if (alt4.active)
                _alterations.Add(alt4);
            if (alt5.active)
                _alterations.Add(alt5);
            if (alt6.active)
                _alterations.Add(alt6);

            if (_altsDirty)
            {
                // Sort all active alterations by their order number.
                _alterations.Sort((altOne, altTwo) => altOne.order.CompareTo(altTwo.order));

                // Reset dirty flag.
                _altsDirty = false;
            }

            return _alterations;
        }
    }

    public bool AltsDirty
    {
        get
        {
            return _altsDirty;
        }
        set
        {
            _altsDirty = value;
        }
    }

    #region
    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    //void Update()
    //{
    //    foreach (Alteration alt in _alterations)
    //    {
    //        alt.info = 
    //    }
    //}

    //private void OnDestroy()
    //{
    //    //RMMemoryManager.Instance.reclaimRMPrimitive(this);
    //    // TO-DO Clean lists and arrays
    //}

    //public void loadSavedObj(RMPrimitive savedObj)
    //{
    //    _primitiveType = savedObj._primitiveType;
    //    _colour = savedObj._colour;
    //    _combineOpType = savedObj._combineOpType;
    //    _combineSmoothness = savedObj._combineSmoothness;
    //    _csgNode = savedObj._csgNode;
    //    _geoInfo = savedObj._geoInfo;
    //}

    //public void resetPrim()
    //{
    //    _primitiveType = PrimitiveTypes.Sphere;
    //    _colour = Color.white;
    //    _combineOpType = CombineOpsTypes.Union;
    //    _combineSmoothness = 0.0f;
    //    _csgNode = false;
    //    _geoInfo = new Vector4(1.0f, 1.0f, 1.0f);
    //}
    #endregion


    private void Reset()
    {
        clearAlts();
    }
    public void clearAlts()
    {
        alt1 = new Alteration();
        alt1.order = 0;
        alt2 = new Alteration();
        alt2.order = 1;
        alt3 = new Alteration();
        alt3.order = 2;
        alt4 = new Alteration();
        alt4.order = 3;
        alt5 = new Alteration();
        alt5.order = 4;
        alt6 = new Alteration();
        alt6.order = 5;

        _alterations.Clear();
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
    SerializedProperty _reflectionCount;
    SerializedProperty _reflectionIntensity;
    SerializedProperty _refractionCount;
    SerializedProperty _refractionIndex;
    //SerializedProperty _texture;
    SerializedProperty _static;

    //SerializedProperty _altInfo;
    //SerializedProperty _altCustomInfo;
    SerializedProperty _alterations;
    SerializedProperty _displaceFormula;



    List<SerializedProperty> _alts = new List<SerializedProperty>(6);
    SerializedProperty _currentAlt;
    SerializedProperty activeProperty;
    SerializedProperty typeProperty;
    SerializedProperty infoProperty;
    SerializedProperty posAltProperty;


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
        //_texture = serializedObject.FindProperty("_texture");
        _static = serializedObject.FindProperty("_static");

        //_altInfo = serializedObject.FindProperty("_alterationInfo");
        //_altCustomInfo = serializedObject.FindProperty("_altCustomInfo");
        _alterations = serializedObject.FindProperty("_alterations");
        _displaceFormula = serializedObject.FindProperty("_displaceFormula");


        for (int i = 1; i <= 6; ++i)
        {
            _alts.Add(serializedObject.FindProperty("alt" + i));
        }
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

        //EditorGUILayout.PropertyField(_texture);
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

        EditorGUILayout.Space();
        // Alterations
        label.text = "Alterations";
        label.tooltip = "";
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        // Clear all alterations.
        if (GUILayout.Button("Clear Alts"))
            rmComp.clearAlts();


        for (int i = 0; i < _alts.Count; ++i)
        {
            _currentAlt = _alts[i];

            activeProperty = _currentAlt.FindPropertyRelative("active");
            // No alterations are active.
            if (!activeProperty.boolValue)
                break;


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            label.text = "Alteration " + (i + 1);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            // Display the current alteration's information.
            DisplayAltInfo(_currentAlt);


            // Remove current alteration.
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove Alteration"))
            {
                removeAlt(_currentAlt);
                rmComp.AltsDirty = true;
            }

            // Move current alteration up.
            EditorGUI.BeginDisabledGroup(i == 0);
            if (GUILayout.Button("Move Up"))
            {
                moveUp(_currentAlt, i);
                rmComp.AltsDirty = true;
            }
            EditorGUI.EndDisabledGroup();


            bool disabled = (i == (_alts.Count - 1));
            if (i < (_alts.Count - 1))
                disabled = !_alts[i + 1].FindPropertyRelative("active").boolValue;

            // Move current alteration down.
            EditorGUI.BeginDisabledGroup(disabled);
            if (GUILayout.Button("Move Down"))
            {
                moveDown(_currentAlt, i);
                rmComp.AltsDirty = true;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        // Add an alteration.
        if (GUILayout.Button("Add Alteration"))
        {
            addAlt();
            rmComp.AltsDirty = true;
        }



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

    void DisplayAltInfo(RMPrimitive rmPrim, AlterationTypes type, Vector4 altInfo, int index, ref SerializedObject currentAltProp)
    {
        SerializedProperty infoProperty = currentAltProp.FindProperty("info");
        SerializedProperty posAltProperty = currentAltProp.FindProperty("posAlt");

        switch (type)
        {
            case AlterationTypes.Elongate1D:
                altInfo.x = EditorGUILayout.FloatField("h", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("h2", infoProperty.vector4Value.y);
                altInfo.z = EditorGUILayout.FloatField("h3", infoProperty.vector4Value.z);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Elongate:
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Round:
                altInfo.x = EditorGUILayout.FloatField("Roundness", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = false;
                break;
            case AlterationTypes.Onion:
                altInfo.x = EditorGUILayout.FloatField("Thickness", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = false;
                break;
            case AlterationTypes.SymX:
                altInfo.x = EditorGUILayout.FloatField("X-Axis Spacing", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.SymXZ:
                altInfo.x = EditorGUILayout.FloatField("X-Axis Spacing", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("Y-Axis Spacing", infoProperty.vector4Value.y);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.RepXZ:
                altInfo.x = EditorGUILayout.FloatField("X-Axis Spacing", infoProperty.vector4Value.x);
                altInfo.z = EditorGUILayout.FloatField("Z-Axis Spacing", infoProperty.vector4Value.z);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.RepFinite:
                altInfo.x = EditorGUILayout.FloatField("C", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("X-Axis Reps", infoProperty.vector4Value.y);
                altInfo.z = EditorGUILayout.FloatField("Y-Axis Reps", infoProperty.vector4Value.z);
                altInfo.w = EditorGUILayout.FloatField("Z-Axis Reps", infoProperty.vector4Value.w);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Twist:
                altInfo.x = EditorGUILayout.FloatField("Twistyness", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Displace:
                altInfo.x = EditorGUILayout.FloatField("X-Axis Displacement", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("Y-Axis Displacement", infoProperty.vector4Value.y);
                altInfo.z = EditorGUILayout.FloatField("Z-Axis Displacement", infoProperty.vector4Value.z);
                EditorGUILayout.PropertyField(_displaceFormula);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = false;
                break;
            case AlterationTypes.Bend:
                altInfo.x = EditorGUILayout.FloatField("Bendyness", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Custom:
                SerializedProperty command = currentAltProp.FindProperty("command");

                GUIContent label = new GUIContent();
                label.text = "Position Alt";
                label.tooltip = "Will this alter the position or the distance?";

                posAltProperty.boolValue = EditorGUILayout.Toggle(label, posAltProperty.boolValue);

                altInfo.x = EditorGUILayout.FloatField("altInfo.x", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("altInfo.y", infoProperty.vector4Value.y);
                altInfo.z = EditorGUILayout.FloatField("altInfo.z", infoProperty.vector4Value.z);
                altInfo.w = EditorGUILayout.FloatField("altInfo.w", infoProperty.vector4Value.w);
                infoProperty.vector4Value = altInfo;

                label.text = "Command";
                label.tooltip = "";
                EditorGUILayout.PropertyField(command, label);
                break;
            default:
                break;
        }
    }

    void DisplayAltInfo(SerializedProperty alt)
    {
        typeProperty = alt.FindPropertyRelative("type");
        AlterationTypes type = (AlterationTypes)typeProperty.enumValueIndex;
        typeProperty.enumValueIndex = (int)((AlterationTypes)EditorGUILayout.EnumPopup(type));

        infoProperty = alt.FindPropertyRelative("info");
        posAltProperty = alt.FindPropertyRelative("posAlt");
        Vector4 altInfo = Vector4.zero;


        switch (type)
        {
            case AlterationTypes.Elongate1D:
                altInfo.x = EditorGUILayout.FloatField("h", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("h2", infoProperty.vector4Value.y);
                altInfo.z = EditorGUILayout.FloatField("h3", infoProperty.vector4Value.z);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Elongate:
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Round:
                altInfo.x = EditorGUILayout.FloatField("Roundness", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = false;
                break;
            case AlterationTypes.Onion:
                altInfo.x = EditorGUILayout.FloatField("Thickness", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = false;
                break;
            case AlterationTypes.SymX:
                altInfo.x = EditorGUILayout.FloatField("X-Axis Spacing", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.SymXZ:
                altInfo.x = EditorGUILayout.FloatField("X-Axis Spacing", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("Y-Axis Spacing", infoProperty.vector4Value.y);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.RepXZ:
                altInfo.x = EditorGUILayout.FloatField("X-Axis Spacing", infoProperty.vector4Value.x);
                altInfo.z = EditorGUILayout.FloatField("Z-Axis Spacing", infoProperty.vector4Value.z);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.RepFinite:
                altInfo.x = EditorGUILayout.FloatField("C", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("X-Axis Reps", infoProperty.vector4Value.y);
                altInfo.z = EditorGUILayout.FloatField("Y-Axis Reps", infoProperty.vector4Value.z);
                altInfo.w = EditorGUILayout.FloatField("Z-Axis Reps", infoProperty.vector4Value.w);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Twist:
                altInfo.x = EditorGUILayout.FloatField("Twistyness", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Displace:
                altInfo.x = EditorGUILayout.FloatField("X-Axis Displacement", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("Y-Axis Displacement", infoProperty.vector4Value.y);
                altInfo.z = EditorGUILayout.FloatField("Z-Axis Displacement", infoProperty.vector4Value.z);
                EditorGUILayout.PropertyField(_displaceFormula);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = false;
                break;
            case AlterationTypes.Bend:
                altInfo.x = EditorGUILayout.FloatField("Bendyness", infoProperty.vector4Value.x);
                infoProperty.vector4Value = altInfo;
                posAltProperty.boolValue = true;
                break;
            case AlterationTypes.Custom:
                SerializedProperty command = alt.FindPropertyRelative("command");

                GUIContent label = new GUIContent();
                label.text = "Position Alt";
                label.tooltip = "Will this alter the position or the distance?";

                posAltProperty.boolValue = EditorGUILayout.Toggle(label, posAltProperty.boolValue);

                altInfo.x = EditorGUILayout.FloatField("altInfo.x", infoProperty.vector4Value.x);
                altInfo.y = EditorGUILayout.FloatField("altInfo.y", infoProperty.vector4Value.y);
                altInfo.z = EditorGUILayout.FloatField("altInfo.z", infoProperty.vector4Value.z);
                altInfo.w = EditorGUILayout.FloatField("altInfo.w", infoProperty.vector4Value.w);
                infoProperty.vector4Value = altInfo;

                label.text = "Command";
                label.tooltip = "";
                EditorGUILayout.PropertyField(command, label);
                break;
            default:
                break;
        }
    }

    void moveUp(SerializedProperty currentAlt, int index)
    {
        // Swap alts around.
        SerializedProperty otherAlt = _alts[index - 1];
        _alts[index - 1] = currentAlt;
        _alts[index] = otherAlt;

        // Update their order properties.
        otherAlt.FindPropertyRelative("order").intValue = index;
        currentAlt.FindPropertyRelative("order").intValue = index - 1;
    }

    void moveDown(SerializedProperty currentAlt, int index)
    {
        // Swap alts around.
        SerializedProperty otherAlt = _alts[index + 1];
        _alts[index + 1] = currentAlt;
        _alts[index] = otherAlt;

        // Update their order properties.
        otherAlt.FindPropertyRelative("order").intValue = index;
        currentAlt.FindPropertyRelative("order").intValue = index + 1;
    }

    void removeAlt(SerializedProperty currentAlt)
    {
        // Deactivate the current alteration.
        currentAlt.FindPropertyRelative("active").boolValue = false;

        serializedObject.ApplyModifiedProperties();

        // Sort the alterations so all the inactive alterations are behind the active alterations.
        // Note: Bools are, by default sorted false to true, hence the '-' sign to reverse this behaviour.
        _alts.Sort((alt, alt2) => -(alt.FindPropertyRelative("active").boolValue.CompareTo(alt2.FindPropertyRelative("active").boolValue)));

        // Re-assign each alteration a new order number.
        for (int i = 0; i < _alts.Count; ++i)
        {
            _alts[i].FindPropertyRelative("order").intValue = i;
        }
    }

    void addAlt()
    {
        SerializedProperty activeProperty;

        // Find first inactive alteration, and active it.
        foreach (SerializedProperty alt in _alts)
        {
            activeProperty = alt.FindPropertyRelative("active");

            if (!activeProperty.boolValue)
            {
                activeProperty.boolValue = true;
                break;
            }
        }
    }
}
#endif