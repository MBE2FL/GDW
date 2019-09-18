using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class RMObj : MonoBehaviour
{
    [SerializeField]
    protected bool _isPrim = true;
    [SerializeField]
    protected int _drawOrder = 0;

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
}

#if UNITY_EDITOR
[CustomEditor(typeof(RMObj))]
[CanEditMultipleObjects]
public class RMObjEditor : Editor
{
    private SerializedProperty _drawOrder;

    protected virtual void OnEnable()
    {
        _drawOrder = serializedObject.FindProperty("_drawOrder");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        serializedObject.Update();

        GUIContent label = new GUIContent("Draw Order", "The order in which this object will be placed in the shader.");
        EditorGUILayout.PropertyField(_drawOrder, label);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
