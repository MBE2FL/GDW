using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class BoneObjData
{
    public RMObj _obj;
    public Transform _target;
    public float _toTarget;
}



[System.Serializable]
public class Bone : MonoBehaviour
{
    //[SerializeField]
    //List<RMObj> _objs;

    [SerializeField]
    List<BoneObjData> _objData;

    [SerializeField]
    BoneObjData _testData;



    public List<BoneObjData> Objs
    {
        get
        {
            return _objData;
        }
    }
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Bone))]
public class BoneEditor : Editor
{
    //SerializedProperty _objs;
    //SerializedProperty _targets;
    RMObj _objField;


    private void OnEnable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        Bone bone = target as Bone;

        serializedObject.Update();


        GUIContent label = new GUIContent();


        // Add an object for bone rendering.
        label.text = "";
        _objField = EditorGUILayout.ObjectField(label, _objField, typeof(RMObj), true, GUILayout.MaxWidth(120.0f)) as RMObj;

        EditorGUI.BeginDisabledGroup(!_objField);
        label.text = "Add Obj";
        if (GUILayout.Button(label, GUILayout.MaxWidth(80.0f)))
        {
            BoneObjData objData = new BoneObjData()
            {
                _obj = _objField,
                _target = null
            };

            //bone.Objs.Add(_objField);
            bone.Objs.Add(objData);
            _objField.transform.SetParent(bone.transform, false);
            _objField = null;
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(30.0f);


        BoneObjData obj = null;
        for (int i = 0; i < bone.Objs.Count; ++i)
        {
            obj = bone.Objs[i];
            displayObj(label, ref obj, bone);
        }


        serializedObject.ApplyModifiedProperties();
    }


    void displayObj(GUIContent label, ref BoneObjData objData, Bone bone)
    {
        RMObj obj = objData._obj;

        label.text = obj.name;
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        label.text = "Target";
        EditorGUILayout.LabelField(label);

        objData._target = EditorGUILayout.ObjectField(GUIContent.none, objData._target, typeof(Transform), true, GUILayout.MaxWidth(200.0f)) as Transform;

        EditorGUI.BeginChangeCheck();
        label.text = "To Target";
        objData._toTarget = EditorGUILayout.Slider(label, objData._toTarget, 0.0f, 1.0f);
        if (EditorGUI.EndChangeCheck())
        {
            Transform trans = objData._obj.transform;
            trans.position = Vector3.Lerp(bone.transform.position, objData._target.position, objData._toTarget);
            trans.rotation = Quaternion.Slerp(bone.transform.rotation, objData._target.rotation, objData._toTarget);
        }


        label.text = "Remove";
        if (GUILayout.Button(label, GUILayout.MaxWidth(80.0f)))
        {
            bone.Objs.Remove(objData);
        }



        EditorGUILayout.Space(10.0f);
    }
}
#endif
