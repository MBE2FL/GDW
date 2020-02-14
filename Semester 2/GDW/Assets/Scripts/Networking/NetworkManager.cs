using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetworkManager : MonoBehaviour
{
    const string DLL_NAME = "NETWORKINGDLL";

    [DllImport(DLL_NAME)]
    public static extern bool connectToServer(string id);

    [DllImport(DLL_NAME)]
    public static extern bool initNetwork(string ip, string id);


    [SerializeField]
    bool _initialized = false;

    [SerializeField]
    bool _connected = false;

    [SerializeField]
    string _ip = "127.0.0.1";

    [SerializeField]
    string _id = "1";


    public event Action onServerConnect;
    public event Action onDataSend;
    public event Action onDataReceive;


    GameManager _gameManager;

    // Start is called before the first frame update
    void Start()
    {
        initializeNetworkManager();

        _gameManager = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_connected || !_gameManager.LevelInProgress)
            return;

        // Send this player's transform data to server.
        if (onDataSend != null)
            onDataSend.Invoke();


        // Retrieve server interpolated transform data of other player.
        if (onDataReceive != null)
            onDataReceive.Invoke();
    }

    void initializeNetworkManager()
    {
        _initialized = initNetwork(_ip, _id);
    }

    public void connect()
    {
        // Establish connection to server.
        if (!_connected)
        {
            _connected = connectToServer(_id);
            Debug.Log("Attemp to connect to server failed!");
        }


        // Connection to server established.
        if (_connected)
        {
            Debug.Log("Successfully connected to server.");

            // Notify all listeners.
            if (onServerConnect != null)
                onServerConnect.Invoke();
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(NetworkManager))]
public class NetworkManagerEditor : Editor
{
    SerializedProperty _intialized;
    SerializedProperty _connected;
    SerializedProperty _ip;
    SerializedProperty _id;

    private void OnEnable()
    {
        _intialized = serializedObject.FindProperty("_initialized");
        _connected = serializedObject.FindProperty("_connected");
        _ip = serializedObject.FindProperty("_ip");
        _id = serializedObject.FindProperty("_id");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        serializedObject.Update();



        GUIContent label = new GUIContent();

        EditorGUILayout.PropertyField(_intialized);

        label.text = "Connected";
        EditorGUILayout.Toggle(label, _connected.boolValue);

        label.text = "IP Address";
        EditorGUILayout.PropertyField(_ip, label);

        label.text = "Network ID";
        EditorGUILayout.PropertyField(_id, label);



        serializedObject.ApplyModifiedProperties();
    }
}
#endif
