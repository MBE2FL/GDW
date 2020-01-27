using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum NodeCombineOpsTypes
{
    Union,
    Subtraction,
    Intersection,
    SmoothUnion,
    SmoothSubtraction,
    SmoothIntersection,
    Lerp
}

[ExecuteInEditMode]
[AddComponentMenu("Ray Marching/CSG")]
[DisallowMultipleComponent]
// Constructive Solid Geomtry
public class CSG : RMObj
{
    [SerializeField]
    private RMObj _firstNode;
    [SerializeField]
    private RMObj _secondNode;

    //[SerializeField]
    //private bool _isFirstNodePrim = true;
    //[SerializeField]
    //private bool _isSecondNodePrim = true;

    [SerializeField]
    private NodeCombineOpsTypes _nodeCombineOpType = NodeCombineOpsTypes.Union;
    [SerializeField]
    [Range(0.0f, 50.0f)]
    private float _nodeCombineSmoothness = 0.0f;

    [SerializeField]
    private bool _isRoot = true;



    public RMObj FirstNode
    {
        get
        {
            return _firstNode;
        }
        set
        {
            _firstNode = value;
        }
    }

    public RMObj SecondNode
    {
        get
        {
            return _secondNode;
        }
        set
        {
            _secondNode = value;
        }
    }

    /// <summary>
    /// Returns true iff this CSG's first node is a primitive.
    /// Note: Will also return false if the first node is null.
    /// </summary>
    public bool IsFirstPrim
    {
        get
        {
            //return _isFirstNodePrim;
            if (_firstNode)
                return _firstNode.IsPrim;

            return false;
        }
        //set
        //{
        //    _isFirstNodePrim = value;
        //}
    }

    /// <summary>
    /// Returns true iff this CSG's second node is a primitive.
    /// Note: Will also return false if the second node is null.
    /// </summary>
    public bool IsSecondPrim
    {
        get
        {
            //return _isSecondNodePrim;
            if (_secondNode)
                return _secondNode.IsPrim;

            return false;
        }
        //set
        //{
        //    _isSecondNodePrim = value;
        //}
    }

    public NodeCombineOpsTypes NodeCombineOpType
    {
        get
        {
            return _nodeCombineOpType;
        }
    }

    public float NodeCombineSmoothness
    {
        get
        {
            return _nodeCombineSmoothness;
        }
        set
        {
            _nodeCombineSmoothness = value;
        }
    }

    public Vector4 CombineOp
    {
        get
        {
            return new Vector4((float)_nodeCombineOpType, _nodeCombineSmoothness, (float)_combineOpType, _combineSmoothness);
        }
    }

    public bool IsRoot
    {
        get
        {
            return _isRoot;
        }
        set
        {
            _isRoot = value;
        }
    }

    /// <summary>
    /// Returns true iff this CSG's first and second nodes are both primitives.
    /// Note: Will also return false if one of the nodes is null.
    /// </summary>
    public bool AllPrimNodes
    {
        get
        {
            //return (_isFirstNodePrim && _isSecondNodePrim);
            if (_firstNode && _secondNode)
                return (_firstNode.IsPrim && _secondNode.IsPrim);

            return false;
        }
    }

    /// <summary>
    /// Returns true iff this CSG, as well as any CSG child nodes it may have, are all valid.
    /// Valid meaning a CSG has both of it's nodes filled.
    /// </summary>
    public bool IsValid
    {
        get
        {
            bool resultFirst = IsFirstPrim;
            bool resultSecond = IsSecondPrim;

            if (_firstNode && !resultFirst)
                resultFirst = (_firstNode as CSG).IsValid;

            if (_secondNode && !resultSecond)
                resultSecond = (_secondNode as CSG).IsValid;

            return (resultFirst && resultSecond);
        }
    }

    private void Awake()
    {
        _isPrim = false;
    }

    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void resetCSG()
    {
        _firstNode = null;
        _secondNode = null;
        _nodeCombineOpType = NodeCombineOpsTypes.Union;
        _nodeCombineSmoothness = 0.0f;
        _combineOpType = CombineOpsTypes.Union;
        _combineSmoothness = 0.0f;
        _isRoot = true;
    }

    private void OnDestroy()
    {
        if (_firstNode)
        {
            if (_firstNode.IsPrim)
                ((RMPrimitive)_firstNode).CSGNode = false;
            else
                ((CSG)_firstNode)._isRoot = true;
        }

        if (_secondNode)
        {
            if (_secondNode.IsPrim)
                ((RMPrimitive)_secondNode).CSGNode = false;
            else
                ((CSG)_secondNode)._isRoot = true;
        }

        //if (Camera.main)
        //    Camera.main.GetComponent<RMMemoryManager>().Dirty = true;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CSG))]
public class CSGEditor : RMObjEditor
{
    SerializedProperty _firstPrimNode;
    SerializedProperty _secondPrimNode;
    SerializedProperty _firstCSGNode;
    SerializedProperty _secondCSGNode;
    SerializedProperty _isFirstNodePrim;
    SerializedProperty _isSecondNodePrim;
    SerializedProperty _nodeCombineOpType;
    SerializedProperty _nodeCombineSmoothness;

    SerializedProperty _isRoot;

    SerializedProperty _firstNode;
    SerializedProperty _secondNode;

    protected override void OnEnable()
    {
        base.OnEnable();

        _firstPrimNode = serializedObject.FindProperty("_firstPrimNode");
        _secondPrimNode = serializedObject.FindProperty("_secondPrimNode");
        _firstCSGNode = serializedObject.FindProperty("_firstCSGNode");
        _secondCSGNode = serializedObject.FindProperty("_secondCSGNode");
        _isFirstNodePrim = serializedObject.FindProperty("_isFirstNodePrim");
        _isSecondNodePrim = serializedObject.FindProperty("_isSecondNodePrim");
        _nodeCombineOpType = serializedObject.FindProperty("_nodeCombineOpType");
        _nodeCombineSmoothness = serializedObject.FindProperty("_nodeCombineSmoothness");

        _isRoot = serializedObject.FindProperty("_isRoot");

        _firstNode = serializedObject.FindProperty("_firstNode");
        _secondNode = serializedObject.FindProperty("_secondNode");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var csg = target as CSG;
        //bool firstPrimChanged = false;
        //bool secondPrimChanged = false;
        //RMPrimitive firstPrimRef = null;
        //RMPrimitive secondPrimRef = null;
        bool firstNodeChanged = false;
        bool secondNodeChanged = false;
        RMObj firstNodeRef = null;
        RMObj secondNodeRef = null;
        GUIContent label = new GUIContent("Node Combine Op", "How the two nodes will interact.");

        bool nodeCombineOpChanged = false;

        serializedObject.Update();

        _isRoot.boolValue = EditorGUILayout.Toggle("Is Root CSG", _isRoot.boolValue);
        EditorGUILayout.Toggle("Is Valid CSG", csg.IsValid);


        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_firstNode);
        firstNodeChanged = EditorGUI.EndChangeCheck();
        firstNodeRef = csg.FirstNode;


        // Determine how the two objects will interact.
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_nodeCombineOpType, label);
        nodeCombineOpChanged = EditorGUI.EndChangeCheck();


        // Display the smoothness property for certain operations.
        if (_nodeCombineOpType.intValue == (int)NodeCombineOpsTypes.SmoothUnion ||
            _nodeCombineOpType.intValue == (int)NodeCombineOpsTypes.SmoothSubtraction ||
            _nodeCombineOpType.intValue == (int)NodeCombineOpsTypes.SmoothIntersection)
        {
            //csg.NodeCombineSmoothness = Mathf.Clamp(EditorGUILayout.FloatField("Node Combine Smoothness", csg.NodeCombineSmoothness), 0.0f, Mathf.Infinity);
            EditorGUILayout.PropertyField(_nodeCombineSmoothness);
            csg.NodeCombineSmoothness = Mathf.Clamp(_nodeCombineSmoothness.floatValue, 0.0f, Mathf.Infinity);
        }
        else if (_nodeCombineOpType.intValue == (int)NodeCombineOpsTypes.Lerp)
        {
            label.text = "Interpolation Value";
            EditorGUILayout.PropertyField(_nodeCombineSmoothness, label);
            //csg.NodeCombineSmoothness = Mathf.Clamp(_nodeCombineSmoothness.floatValue, 0.0f, Mathf.Infinity);
        }


        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_secondNode);
        secondNodeChanged = EditorGUI.EndChangeCheck();
        secondNodeRef = csg.SecondNode;


        EditorGUI.BeginDisabledGroup(!csg.IsRoot);
        // Determine how the CSG will interact with the scene.
        label.text = "CSG Combine Op";
        if (csg.IsRoot)
            label.tooltip = "How the CSG will interact with the scene.";
        else
            label.tooltip = "Overridden by parent CSG.";

        displayCombineOp(label, csg);
        EditorGUI.EndDisabledGroup();



        // Display Alterations
        displayAlterations(label, csg);


        serializedObject.ApplyModifiedProperties();


        // Avoids change if the same node was used for both slots.
        if (csg.FirstNode && (csg.FirstNode == csg.SecondNode))
        {
            // First slot was the same as the second's slot.
            if (firstNodeChanged)
            {
                firstNodeChanged = false;
                csg.FirstNode = null;
            }
            // Second slot was the same as the first's slot.
            else
            {
                secondNodeChanged = false;
                csg.SecondNode = null;
            }
        }

        // First node has been added, changed, or removed. (Avoids change if user selected the same first node.)
        if (firstNodeChanged && (csg.FirstNode != firstNodeRef))
        {
            // First node was just added/changed.
            if (csg.FirstNode)
            {
                if (csg.IsFirstPrim)
                    (csg.FirstNode as RMPrimitive).CSGNode = true;
                else
                    (csg.FirstNode as CSG).IsRoot = false;
            }
            // Previous first node was just removed/changed, iff not previously empty.
            if (firstNodeRef)
            {
                if (firstNodeRef.IsPrim)
                    (firstNodeRef as RMPrimitive).CSGNode = false;
                else
                    (firstNodeRef as CSG).IsRoot = true;
            }
        }

        // Second node has been added, changed, or removed. (Avoids change if user selected the same second node.)
        if (secondNodeChanged && (csg.SecondNode != secondNodeRef))
        {
            // Second node was just added/changed.
            if (csg.SecondNode)
            {
                if (csg.IsSecondPrim)
                    (csg.SecondNode as RMPrimitive).CSGNode = true;
                else
                    (csg.SecondNode as CSG).IsRoot = false;
            }
            // Previous second node was just removed/changed, iff not previously empty.
            if (secondNodeRef)
            {
                if (secondNodeRef.IsPrim)
                    (secondNodeRef as RMPrimitive).CSGNode = false;
                else
                    (secondNodeRef as CSG).IsRoot = true;
            }
        }


        // Update and reload the shader.
        //if (nodeCombineOpChanged)
        //    Camera.main.GetComponent<ShaderBuilder>().build();

        //if (combineOpChanged)
        //    Camera.main.GetComponent<ShaderBuilder>().build();
    }
}
#endif
