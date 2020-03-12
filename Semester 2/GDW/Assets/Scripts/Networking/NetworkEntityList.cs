using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Network Entity List", menuName = "Networking/Create Entity List")]
public class NetworkEntityList : ScriptableObject
{
    [SerializeField]
    List<NetworkObject> _netObjs;

    public List<NetworkObject> NetObjs
    {
        get
        {
            return _netObjs;
        }
        set
        {
            _netObjs = value;
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(NetworkEntityList))]
public class NetworkEntityListEditor : Editor
{


    private void OnEnable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NetworkEntityList netEntList = target as NetworkEntityList;

        serializedObject.Update();

        GUIContent label = new GUIContent();

        List<NetworkObject> netObjs = netEntList.NetObjs;

        foreach (NetworkObject netObj in netObjs)
        {
            label.text = netObj.name;

            EditorGUILayout.LabelField(label);

            label.text = "Position";

            netObj.transform.position = EditorGUILayout.Vector3Field(label, netObj.transform.position);
            //netObj.transform.rotation = EditorGUILayout.(label, netObj.transform.rotation);


            if (GUILayout.Button("Remove"))
            {
                netObjs.Remove(netObj);
                return;
            }
        }


        serializedObject.ApplyModifiedProperties();
    }
}
#endif