using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


// This struct needs to be in the same order as in C++
public struct CS_to_Plugin_Functions
{
    public IntPtr MultiplyVectors;
    public IntPtr MultiplyInts;
    public IntPtr RandomFloat;

    // The functions don't need to be the same though
    // Init isn't in C++
    public bool Init(IntPtr pluginHandle)
    {
        MultiplyVectors = Marshal.GetFunctionPointerForDelegate(new Func<Vector3, Vector3, Vector3>(NetworkManager.multiplyVectors));
        MultiplyInts = Marshal.GetFunctionPointerForDelegate(new Func<int, int, int>(NetworkManager.MultiplyInts));
        RandomFloat = Marshal.GetFunctionPointerForDelegate(new Func<float>(NetworkManager.GetFloat));

        return true;
    }
}



public class NetworkManager : MonoBehaviour
{
    //const string DLL_NAME = "NETWORKINGDLL";
    // Path to the DLL
    private const string PATH = "/Plugins/Release/NetworkingDLL.dll";

    // Handle for the DLL 
    private IntPtr _pluginHandle;

    // Container for all functions we're sending over to C++
    private CS_to_Plugin_Functions _pluginFunctions = new CS_to_Plugin_Functions();

    // When getting a function from C++ for use in C#, you need these lines

    // public delegate return_type FUNCTIONNAME_Delegate(parameters)
    public delegate void InitPluginDelegate(CS_to_Plugin_Functions functions);
    // public type_from_last_line function_name
    public InitPluginDelegate InitPlugin;

    public delegate void InitConsoleDelegate();
    public InitConsoleDelegate InitConsole;

    public delegate void FreeTheConsoleDelegate();
    public FreeTheConsoleDelegate FreeTheConsole;

    public delegate IntPtr OutputConsoleMessageDelegate(string msg);
    public OutputConsoleMessageDelegate OutputConsoleMessage;


    // Network manager functions
    public delegate bool connectToServerDelegate(string id);
    public connectToServerDelegate connectToServer;

    public delegate bool initNetworkDelegate(string ip, string id);
    public initNetworkDelegate initNetwork;

    //[DllImport(DLL_NAME)]
    //public static extern bool connectToServer(string id);

    //[DllImport(DLL_NAME)]
    //public static extern bool initNetwork(string ip, string id);


    // Network object functions
    public delegate void sendDataDelegate(ref Vector3 position, ref Quaternion rotation);
    public sendDataDelegate sendData;

    public delegate void receiveDataDelegate(ref Vector3 position, ref Quaternion rotation);
    public receiveDataDelegate receiveData;


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



    private void InitPluginFunctions()
    {
        // To get the function, add this line
        // FunctionName = ManualPluginImporter.GetDelegate<delegate_type>(Plugin_Handle, "NAME_OF_FUNCTION_IN_C++");
        // You can now call the function like any other. For example: FunctionName(int a, int b);
        InitPlugin = ManualPluginImporter.GetDelegate<InitPluginDelegate>(_pluginHandle, "InitPlugin");
        InitConsole = ManualPluginImporter.GetDelegate<InitConsoleDelegate>(_pluginHandle, "InitConsole");
        FreeTheConsole = ManualPluginImporter.GetDelegate<FreeTheConsoleDelegate>(_pluginHandle, "FreeTheConsole");
        OutputConsoleMessage = ManualPluginImporter.GetDelegate<OutputConsoleMessageDelegate>(_pluginHandle, "OutputMessageToConsole");

        // Network manager functions
        connectToServer = ManualPluginImporter.GetDelegate<connectToServerDelegate>(_pluginHandle, "connectToServer");
        initNetwork = ManualPluginImporter.GetDelegate<initNetworkDelegate>(_pluginHandle, "initNetwork");

        // Network object functions
        sendData = ManualPluginImporter.GetDelegate<sendDataDelegate>(_pluginHandle, "sendData");
        receiveData = ManualPluginImporter.GetDelegate<receiveDataDelegate>(_pluginHandle, "receiveData");
    }

    private void Awake()
    {
        // Open the library
        _pluginHandle = ManualPluginImporter.OpenLibrary(Application.dataPath + PATH);

        // Init the C# functions
        _pluginFunctions.Init(_pluginHandle);

        // Init the plugin functions 
        // Always call this before calling any C++ functions
        InitPluginFunctions();

        // We're just calling InitPlugin (a C++ function)
        InitPlugin(_pluginFunctions);

        InitConsole();

        // Output a message to the console, using a C++ function
        IntPtr result = OutputConsoleMessage("This is a test.");
        Debug.Log(Marshal.PtrToStringAnsi(result));
    }

    private void OnApplicationQuit()
    {
        // Not sure if this works
        FreeTheConsole();

        // close the plugin
        // This will allow you to rebuild the dll while unity is open (but not while playing)
        ManualPluginImporter.CloseLibrary(_pluginHandle);
    }

    // Start is called before the first frame update
    void Start()
    {
        initializeNetworkManager();

        _gameManager = GetComponent<GameManager>();
    }

    // Example functions
    public static Vector3 multiplyVectors(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public static int MultiplyInts(int i1, int i2)
    {
        return i1 * i2;
    }

    public static float GetFloat()
    {
        return UnityEngine.Random.Range(0.0f, 100.0f);
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
