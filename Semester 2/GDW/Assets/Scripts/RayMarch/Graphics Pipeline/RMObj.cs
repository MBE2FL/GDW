using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif
using System.Linq;

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

public enum BoundingShapes
{
    Sphere,
    Box
}



public abstract class RMObj : MonoBehaviour
{
    [SerializeField]
    protected bool _isPrim = true;
    [SerializeField]
    protected int _drawOrder = 0;

    [SerializeField]
    protected bool _static = false;

    [SerializeField]
    protected CombineOpsTypes _combineOpType = CombineOpsTypes.Union;

    [SerializeField]
    [Range(0.0f, 50.0f)]
    protected float _combineSmoothness = 0.0f;

    [SerializeField]
    protected List<Alteration> _alterations = new List<Alteration>(5);

    public Alteration alt1;
    public Alteration alt2;
    public Alteration alt3;
    public Alteration alt4;
    public Alteration alt5;


    protected bool _altsDirty = false;

    [SerializeField]
    protected BoundingShapes _boundShape = BoundingShapes.Sphere;
    [SerializeField]
    protected Vector4 _boundGeoInfo = Vector4.one;

    [SerializeField]
    protected List<RayMarchShader> _shaderList = new List<RayMarchShader>();

    protected event System.Action<RayMarchShader, RMObj> test;
    

    public bool IsPrim
    {
        get
        {
            return _isPrim;
        }
    }
    
    public int DrawOrder
    {
        get
        {
            return _drawOrder;
        }
        set
        {
            _drawOrder = value;
        }
    }

    public bool Static
    {
        get
        {
            return _static;
        }
    }

    public CombineOpsTypes CombineOpType
    {
        get
        {
            return _combineOpType;
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

            // TO-DO re-compile list only if alterations are animating or changing.
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

    public BoundingShapes BoundShape
    {
        get
        {
            return _boundShape;
        }
    }

    public Vector3 BoundGeoInfo
    {
        get
        {
            return _boundGeoInfo;
        }
    }

    public List<RayMarchShader> ShaderList
    {
        get
        {
            return _shaderList;
        }
    }

    public void AddToShaderList(RayMarchShader shader)
    {
        _shaderList.Add(shader);
    }

    public void removeFromShaderList(RayMarchShader shader)
    {
        // Remove this object from the shader.
        shader.removeFromRenderList(this);

        // Remove the shader from this object.
        int index =_shaderList.IndexOf(shader);
        _shaderList[index] = _shaderList[_shaderList.Count - 1];
        _shaderList.RemoveAt(_shaderList.Count - 1);
    }

    public void removeFromShaderList(int index)
    {
        // Remove this object from the shader.
        _shaderList[index].removeFromRenderList(this);

        // Remove the shader from this object.
        _shaderList[index] = _shaderList[_shaderList.Count - 1];
        _shaderList.RemoveAt(_shaderList.Count - 1);
    }

    public void removeAllFromShaderList()
    {
        // Remove this object from all the shaders.
        foreach (RayMarchShader shader in _shaderList)
        {
            shader.removeFromRenderList(this);
        }

        // Remove all the shaders from this object.
        _shaderList.Clear();
    }

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

        _alterations.Clear();
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(RMObj))]
[CanEditMultipleObjects]
public class RMObjEditor : Editor
{
    private SerializedProperty _drawOrder;
    private SerializedProperty _static;
    private SerializedProperty _combineOpType;
    private SerializedProperty _combineSmoothness;
    private SerializedProperty _shaderList;

    // Alteration variables
    private List<SerializedProperty> _alts = new List<SerializedProperty>(5);
    private SerializedProperty _currentAlt;
    private SerializedProperty activeProperty;
    private SerializedProperty typeProperty;
    private SerializedProperty infoProperty;
    private SerializedProperty posAltProperty;
    private SerializedProperty _displaceFormula;
    private SerializedProperty _boundShape;
    private SerializedProperty _boundGeoInfo;
    private RayMarcher _rayMarcher;

    private int _selectedShaderIndex = 0;
    private string[] _shaderNames;

    AnimBool _showShaderList;

    protected virtual void OnEnable()
    {
        _drawOrder = serializedObject.FindProperty("_drawOrder");
        _static = serializedObject.FindProperty("_static");
        _combineOpType = serializedObject.FindProperty("_combineOpType");
        _combineSmoothness = serializedObject.FindProperty("_combineSmoothness");
        _shaderList = serializedObject.FindProperty("_shaderList");

        // Alteration stuff
        _displaceFormula = serializedObject.FindProperty("_displaceFormula");

        // Store all alterations for later use.
        for (int i = 1; i <= 5; ++i)
        {
            _alts.Add(serializedObject.FindProperty("alt" + i));
        }

        _boundShape = serializedObject.FindProperty("_boundShape");
        _boundGeoInfo = serializedObject.FindProperty("_boundGeoInfo");

        _rayMarcher = RayMarcher.Instance;

        _showShaderList = new AnimBool(true);
        _showShaderList.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        RMObj rmObj = target as RMObj;

        serializedObject.Update();


        GUIContent label = new GUIContent("Shader List", "");
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        // Retrieve all the ray march shaders, and display them as options to be added to.
        List<RayMarchShader> shaders = _rayMarcher.Shaders;
        shaders = shaders.Except(rmObj.ShaderList).ToList();
        label.text = "Shader Options";
        label.tooltip = "All ray marching shaders you can add this object to.";

        if (shaders.Count > 0)
        {
            // Retrieve all the shaders' names.
            _shaderNames = new string[shaders.Count];
            for (int i = 0; i < shaders.Count; ++i)
            {
                _shaderNames[i] = shaders[i].ShaderName;
            }

            // Display them.
            _selectedShaderIndex = EditorGUILayout.Popup(label, _selectedShaderIndex, _shaderNames);

            if (GUILayout.Button("Add To Shader"))
            {
                // Add to object's shader list.
                rmObj.AddToShaderList(shaders[_selectedShaderIndex]);

                // Add to shader's render list.
                //shaders[_selectedShaderIndex].RenderList.Add(rmObj);
                shaders[_selectedShaderIndex].AddToRenderList(rmObj);
            }
        }
        else
        {
            _shaderNames = new string[1] { "No Shaders Available" };
            _selectedShaderIndex = EditorGUILayout.Popup(label, _selectedShaderIndex, _shaderNames);
        }

        EditorGUILayout.Space(4.0f);
        if (GUILayout.Button("Remove From All Shaders"))
        {
            rmObj.removeAllFromShaderList();
        }
        EditorGUILayout.Space(4.0f);

        // Display all shaders in this object's shader list.
        label.tooltip = "";
        _showShaderList.target = EditorGUILayout.ToggleLeft("Show/Hide Shader List", _showShaderList.target);
        if (EditorGUILayout.BeginFadeGroup(_showShaderList.faded))
        {
            shaders = rmObj.ShaderList;
            Rect r = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Rect shaderRect;

            for (int i = 0; i < shaders.Count; ++i)
            {
                shaderRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                label.text = shaders[i].ShaderName;
                EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(2.0f);

                label.text = "Draw Order";
                label.tooltip = "The order in which this object will be placed in the shader.";
                EditorGUILayout.PropertyField(_drawOrder, label);
                EditorGUILayout.Space(2.0f);
                
                if (GUILayout.Button("Remove From Shader"))
                {
                    rmObj.removeFromShaderList(i);
                    break;
                }
                EditorGUILayout.Space(1.0f);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();

        EditorGUILayout.Space(10.0f);




        label.text = "Draw Order";
        label.tooltip = "The order in which this object will be placed in the shader.";
        EditorGUILayout.PropertyField(_drawOrder, label);

        label.text = "Static";
        label.tooltip = "Will hard code all info into the shader.";
        EditorGUILayout.PropertyField(_static, label);

        label.text = "Bound Shape";
        label.tooltip = "";
        EditorGUILayout.PropertyField(_boundShape, label);

        label.text = "Bound Geo Info";
        label.tooltip = "The dimensions of the bounding shape for this object.";
        Vector4 boundGeoInfo = new Vector4(1.0f, 1.0f, 1.0f, 0.0f);

        switch (_boundShape.enumValueIndex)
        {
            case (int)BoundingShapes.Sphere:
                boundGeoInfo.x = EditorGUILayout.FloatField("Radius", _boundGeoInfo.vector4Value.x);
                _boundGeoInfo.vector4Value = boundGeoInfo;
                break;
            case (int)BoundingShapes.Box:
                boundGeoInfo.x = EditorGUILayout.FloatField("Length", _boundGeoInfo.vector4Value.x);
                boundGeoInfo.z = EditorGUILayout.FloatField("Breadth", _boundGeoInfo.vector4Value.z);
                boundGeoInfo.y = EditorGUILayout.FloatField("Height", _boundGeoInfo.vector4Value.y);
                _boundGeoInfo.vector4Value = boundGeoInfo;
                break;
            default:
                break;
        }


        serializedObject.ApplyModifiedProperties();
    }

    public void displayCombineOp(GUIContent label, RMObj obj)
    {
        EditorGUILayout.PropertyField(_combineOpType, label);

        label.text = "Combine Smoothness";

        if (_combineOpType.intValue == (int)CombineOpsTypes.SmoothUnion ||
            _combineOpType.intValue == (int)CombineOpsTypes.SmoothSubtraction ||
            _combineOpType.intValue == (int)CombineOpsTypes.SmoothIntersection)
        {
            EditorGUILayout.PropertyField(_combineSmoothness);
            obj.CombineSmoothness = Mathf.Clamp(_combineSmoothness.floatValue, 0.0f, Mathf.Infinity);
        }
    }

    public void displayAlterations(GUIContent label, RMObj obj)
    {
        EditorGUILayout.Space();
        // Alterations
        label.text = "Alterations";
        label.tooltip = "";
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        // Clear all alterations.
        if (GUILayout.Button("Clear Alts"))
            obj.clearAlts();


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
                obj.AltsDirty = true;
            }

            // Move current alteration up.
            EditorGUI.BeginDisabledGroup(i == 0);
            if (GUILayout.Button("Move Up"))
            {
                moveUp(_currentAlt, i);
                obj.AltsDirty = true;
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
                obj.AltsDirty = true;
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
            obj.AltsDirty = true;
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
