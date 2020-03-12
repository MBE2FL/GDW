using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UI;
using Unity.Collections.LowLevel.Unsafe;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.Jobs;


// This struct needs to be in the same order as in C++
public struct CS_to_Plugin_Functions
{
    public IntPtr MultiplyVectors;
    public IntPtr MultiplyInts;
    public IntPtr RandomFloat;

    public IntPtr ConnectedToServer;

    // The functions don't need to be the same though
    // Init isn't in C++
    public bool Init(IntPtr pluginHandle, NetworkManager networkManager)
    {
        MultiplyVectors = Marshal.GetFunctionPointerForDelegate(new Func<Vector3, Vector3, Vector3>(NetworkManager.multiplyVectors));
        MultiplyInts = Marshal.GetFunctionPointerForDelegate(new Func<int, int, int>(NetworkManager.MultiplyInts));
        //RandomFloat = Marshal.GetFunctionPointerForDelegate(new Func<float>(NetworkManager.GetFloat));
        RandomFloat = Marshal.GetFunctionPointerForDelegate(new Func<float>(NetworkManager.GetFloat));

        //ConnectedToServer = Marshal.GetFunctionPointerForDelegate(new Func<bool>(NetworkManager.connectedToServer));

        return true;
    }
}


public enum MessageTypes : byte
{
    ConnectionAttempt,
    ConnectionAccepted,
    ConnectionFailed,
    ServerFull,
    TransformMsg,
    Anim,
    EntitiesQuery,
    EntitiesStart,
    EntitiesNoStart,
    EntitiesRequired,
    EntitiesUpdate,
    EntityIDs,
    EmptyMsg,
    ErrorMsg
}


[StructLayout(LayoutKind.Sequential)]
public struct TransformData
{
    public byte objID;
    public Vector3 pos;
    public Quaternion rot;
}

[StructLayout(LayoutKind.Sequential)]
public struct AnimData
{
    public byte objID;
    public int state;
}

[StructLayout(LayoutKind.Sequential)]
public struct EntityData
{
    public byte entityID;
    public byte entityPrefabType;
    public byte ownership;
}




struct ConnectJob : IJob
{
    [NativeDisableUnsafePtrRestriction]
    public IntPtr _ip;



    public void Execute()
    {
        // Attempt to establish a connection to the server.
        NetworkManager.connectToServer(Marshal.PtrToStringAnsi(_ip));
    }
}




public class NetworkManager : MonoBehaviour
{
#region DLL_VARIABLES
    //const string DLL_NAME = "NETWORKINGDLL";
    // Path to the DLL
#if UNITY_EDITOR
    private const string PATH = "/Plugins/Release/NetworkingDLL.dll";
#else
    private const string PATH = "/Plugins/NetworkingDLL.dll";
#endif

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
    public delegate bool connectToServerDelegate(string ip);
    public static connectToServerDelegate connectToServer;

    public delegate MessageTypes queryEntityRequestDelegate();
    public static queryEntityRequestDelegate queryEntityRequest;

    public delegate bool sendStarterEntitiesDelegate(IntPtr entities, int numEntities);
    public static sendStarterEntitiesDelegate sendStarterEntities;

    public delegate bool sendRequiredEntitiesDelegate(IntPtr entities, int numEntities);
    public static sendRequiredEntitiesDelegate sendRequiredEntities;

    public delegate bool initNetworkDelegate(string ip);
    public initNetworkDelegate initNetwork;

    //[DllImport(DLL_NAME)]
    //public static extern bool connectToServer(string id);

    //[DllImport(DLL_NAME)]
    //public static extern bool initNetwork(string ip, string id);


    // Network object functions
    public delegate bool queryConnectAttemptDelegate(ref int id);
    public queryConnectAttemptDelegate queryConnectAttempt;

    //public delegate void sendDataDelegate(ref Vector3 position, ref Quaternion rotation);
    public delegate void sendDataDelegate(int msgType, int objID, IntPtr data);
    public sendDataDelegate sendData;

    //public delegate void receiveDataDelegate(ref Vector3 position, ref Quaternion rotation);
    public delegate void receiveDataDelegate(ref MessageTypes msgType, ref int objID, ref IntPtr data);
    public receiveDataDelegate receiveData;

    public delegate IntPtr getReceiveDataDelegate(ref int numElements);
    public getReceiveDataDelegate getReceiveData;



    public delegate void receiveUDPDataDelegate();
    public receiveUDPDataDelegate receiveUDPData;

    public delegate void getPacketHandleSizesDelegate(ref int transDataElements, ref int animDataElements);
    public getPacketHandleSizesDelegate getPacketHandleSizes;

    //public delegate void getPacketHandlesDelegate(ref int transDataElements, IntPtr transDataHandle, ref int animDataElements, IntPtr animDataHandle);
    //public getPacketHandlesDelegate getPacketHandles;

    public delegate void getPacketHandlesDelegate(IntPtr dataHandle);
    public getPacketHandlesDelegate getPacketHandles;

    public delegate void packetHandlesCleanUpDelegate();
    public packetHandlesCleanUpDelegate packetHandlesCleanUp;

    public delegate IntPtr getTransformHandleDelegate();
    public getTransformHandleDelegate getTransformHandle;
#endregion DLL_VARIABLES



    [SerializeField]
    bool _initialized = false;

    [SerializeField]
    bool _connected = false;
    [SerializeField]
    bool _connecting = false;

    [SerializeField]
    string _ip = "127.0.0.1";

    [SerializeField]
    int _id = 0;

    [SerializeField]
    GameObject _connectButton;

    [SerializeField]
    GameObject _testObj;


    public static event Action onServerConnect;
    public event Action onDataSend;
    public event Action onDataReceive;


    GameManager _gameManager;

    [SerializeField]
    float _timeSinceLastUpdate = 0.0f;
    float _time = 0.0f;
    [SerializeField]
    float _updateInterval = 0.066f; // 1s / 15ups = 0.066ms
    //float _updateInterval = 0.167f;

    [SerializeField]
    List<NetworkObject> _networkObjects;
    public static EntityData[] entityData;
    public static int numEntities = 0;


    //[SerializeField]
    //List<NetworkObject> _networkPrefabs;
    [SerializeField]
    Dictionary<PrefabTypes, NetworkObject> _networkPrefabs;
    public List<NetworkObject> NetworkObjects
    {
        get
        {
            return _networkObjects;
        }
        set
        {
            _networkObjects = value;
        }
    }



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
        queryEntityRequest = ManualPluginImporter.GetDelegate<queryEntityRequestDelegate>(_pluginHandle, "queryEntityRequest");
        sendStarterEntities = ManualPluginImporter.GetDelegate<sendStarterEntitiesDelegate>(_pluginHandle, "sendStarterEntities");
        sendRequiredEntities = ManualPluginImporter.GetDelegate<sendRequiredEntitiesDelegate>(_pluginHandle, "sendRequiredEntities");
        initNetwork = ManualPluginImporter.GetDelegate<initNetworkDelegate>(_pluginHandle, "initNetwork");

        // Network object functions
        queryConnectAttempt = ManualPluginImporter.GetDelegate<queryConnectAttemptDelegate>(_pluginHandle, "queryConnectAttempt");
        sendData = ManualPluginImporter.GetDelegate<sendDataDelegate>(_pluginHandle, "sendData");
        receiveData = ManualPluginImporter.GetDelegate<receiveDataDelegate>(_pluginHandle, "receiveData");
        getReceiveData = ManualPluginImporter.GetDelegate<getReceiveDataDelegate>(_pluginHandle, "getReceiveData");


        receiveUDPData = ManualPluginImporter.GetDelegate<receiveUDPDataDelegate>(_pluginHandle, "receiveUDPData");
        getPacketHandleSizes = ManualPluginImporter.GetDelegate<getPacketHandleSizesDelegate>(_pluginHandle, "getPacketHandleSizes");
        getPacketHandles = ManualPluginImporter.GetDelegate<getPacketHandlesDelegate>(_pluginHandle, "getPacketHandles");
        packetHandlesCleanUp = ManualPluginImporter.GetDelegate<packetHandlesCleanUpDelegate>(_pluginHandle, "packetHandlesCleanUp");
        getTransformHandle = ManualPluginImporter.GetDelegate<getTransformHandleDelegate>(_pluginHandle, "getTransformHandle");
    }

    private void Awake()
    {
        // Open the library
        _pluginHandle = ManualPluginImporter.OpenLibrary(Application.dataPath + PATH);

        // Init the C# functions
        _pluginFunctions.Init(_pluginHandle, this);

        // Init the plugin functions 
        // Always call this before calling any C++ functions
        InitPluginFunctions();

        // We're just calling InitPlugin (a C++ function)
        InitPlugin(_pluginFunctions);

        InitConsole();

        // Output a message to the console, using a C++ function
        //IntPtr result = OutputConsoleMessage("This is a test.");
        //Debug.Log(Marshal.PtrToStringAnsi(result));
        //OutputConsoleMessage("This is a test.");

        onServerConnect += connectServerSuccess;


        NetworkObject[] _tempPrefabs = Resources.LoadAll<NetworkObject>("Server Prefabs");
        _networkPrefabs = new Dictionary<PrefabTypes, NetworkObject>(_tempPrefabs.Length);
        foreach (NetworkObject netObj in _tempPrefabs)
        {
            _networkPrefabs.Add(netObj.PrefabType, netObj);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        initializeNetworkManager();

        _gameManager = GetComponent<GameManager>();

        GameManager.onPlay += play;
    }

    private void OnApplicationQuit()
    {
        // Not sure if this works
        FreeTheConsole();

        // close the plugin
        // This will allow you to rebuild the dll while unity is open (but not while playing)
        ManualPluginImporter.CloseLibrary(_pluginHandle);

        onServerConnect -= connectServerSuccess;

        _networkObjects.Clear();

        GameManager.onPlay -= play;
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
        // Connecting to server
        if (!_connected && _connecting)
        {
            // Connected to server
            if (queryConnectAttempt(ref _id))
            {
                // Notify all listeners.
                if (onServerConnect != null)
                    onServerConnect.Invoke();
            }
        }

        if (!_connected || !_gameManager.LevelInProgress)
            return;


        float deltaTime = Time.time - _timeSinceLastUpdate;
        _timeSinceLastUpdate = deltaTime;

        _time -= deltaTime;

        if (_time <= 0.0f)
        {

            //// Send this player's transform data to server.
            //if (onDataSend != null)
            //    onDataSend.Invoke();


            //// Retrieve server interpolated transform data of other player.
            ////if (onDataReceive != null)
            ////    onDataReceive.Invoke();
            //receivePackets();

            _time = _updateInterval;
        }
    }

    void initializeNetworkManager()
    {
        _initialized = initNetwork(_ip);
    }

    public void connect()
    {
        // Only attempt to establish a connection to the server iff, no connection has already been made.
        if (!_connected && !_connecting)
        {
            ConnectJob job = new ConnectJob();
            job._ip = Marshal.StringToHGlobalAnsi(_ip);


            numEntities = _networkObjects.Count;
            entityData = new EntityData[numEntities];

            int i = 0;
            foreach (NetworkObject obj in _networkObjects)
            {
                entityData[i].entityID = obj.ObjID;
                entityData[i].entityPrefabType = (byte)obj.PrefabType;
                entityData[i].ownership = (byte)obj.Ownership;

                ++i;
            }

            //job._entities = Marshal.GetIUnknownForObject(entities);
            //job._entities = IntPtr.Zero;


            //job._networkManager;
            //Marshal.StructureToPtr<NetworkManager>(this, job._networkManager, false);


            JobHandle jobHandle = job.Schedule();

            //jobHandle.Complete();


            _connecting = true;

            _testObj.SetActive(true);

            _connectButton.GetComponent<Button>().interactable = false;
            _connectButton.GetComponentInChildren<Text>().text = "Connecting...";
        }
    }

    public void connectServerSuccess()
    {
        _connected = true;
        _connecting = false;


        _connectButton.SetActive(false);
        // For some reason won't turn on/off game objects.
        //_connectingButton.SetActive(false);

        _testObj.SetActive(false);

        Debug.Log("Successfully connected to server.");
    }

    public void play()
    {
        MessageTypes msgType = queryEntityRequest();
        Debug.Log(msgType);

        if (msgType == MessageTypes.EntitiesStart)
        {
            unsafe
            {
                fixed (EntityData* tempPtr = entityData)
                {
                    IntPtr entitiesPtr = new IntPtr(tempPtr);
                    // Send starting entity list to the server.
                    sendStarterEntities(entitiesPtr, numEntities);
                }
            }

            NetworkObject netObj = null;
            foreach (EntityData entity in entityData)
            {
                netObj = Instantiate(_networkPrefabs[(PrefabTypes)entity.entityPrefabType], Vector3.zero, Quaternion.identity);
                netObj.ObjID = entity.entityID;

                Debug.Log("Entity with ID spawned: " + entity.entityID);
            }
        }
        else if (msgType == MessageTypes.EntitiesRequired)
        {

        }
        else if (msgType == MessageTypes.EmptyMsg)
        {

        }
        else if (msgType == MessageTypes.ErrorMsg)
        {

        }
        else
        {

        }
    }


    void receivePackets()
    {
        receiveUDPData();


        int transDataElements = -1;
        int animDataElements = -1;
        //IntPtr transDataHandle;
        //IntPtr animDataHandle;
        IntPtr dataHandle;


        getPacketHandleSizes(ref transDataElements, ref animDataElements);

        //transDataHandle = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>() * transDataElements);
        //animDataHandle = Marshal.AllocHGlobal(Marshal.SizeOf<AnimData>() * animDataElements);

        dataHandle = Marshal.AllocHGlobal((Marshal.SizeOf<TransformData>() * transDataElements) + (Marshal.SizeOf<AnimData>() * animDataElements));

        getPacketHandles(dataHandle);


        TransformData[] transData = new TransformData[transDataElements];
        AnimData[] animData = new AnimData[animDataElements];


        //transDataHandle = getTransformHandle();

        IntPtr tempDataHandle = dataHandle;
        for (int i = 0; i < transDataElements; ++i)
        {
            transData[i] = (TransformData)Marshal.PtrToStructure(dataHandle, typeof(TransformData));
            dataHandle += Marshal.SizeOf(typeof(TransformData));

            Debug.Log("objID: " + transData[i].objID);
            Debug.Log("Pos: " + transData[i].pos.ToString());
            Debug.Log("Rot: " + transData[i].rot.ToString());
        }

        // Clean up
        Array.Clear(transData, 0, transData.Length);
        Array.Clear(animData, 0, animData.Length);

        //Marshal.FreeHGlobal(tempTestFuck);
        //Marshal.FreeHGlobal(animDataHandle);
        Marshal.FreeHGlobal(tempDataHandle);



        //packetHandlesCleanUp();



        #region OLD_BYTE_TEST
        //Vector3 position = Vector3.zero;
        //Quaternion rotation = Quaternion.identity;

        ////_networkManager.receiveData(ref position, ref rotation);

        ////transform.position = position;
        ////transform.rotation = rotation;


        //MessageTypes msgType = MessageTypes.ConnectionAttempt;
        //int objID = -1;
        //IntPtr data = IntPtr.Zero;
        //int numElements = -1;
        //byte[] byteData;


        //_networkManager.receiveData(ref msgType, ref objID, ref data);


        //data = _networkManager.getReceiveData(ref numElements);

        //byteData = new byte[numElements * 512];

        //Marshal.Copy(data, byteData, 0, numElements * 512);



        //// Received some packets.
        //if (numElements > 0)
        //{
        //    int packetSize = 512;
        //    int packetOffset = 0;
        //    for (int i = 0; i < numElements; ++i)
        //    {
        //        packetOffset = i * packetSize;

        //        msgType = (MessageTypes)byteData[packetOffset];


        //        switch (msgType)
        //        {
        //            case MessageTypes.TransformMsg:
        //                {
        //                    position.x = BitConverter.ToSingle(byteData, 3 + packetOffset);
        //                    position.y = BitConverter.ToSingle(byteData, 7 + packetOffset);
        //                    position.z = BitConverter.ToSingle(byteData, 11 + packetOffset);
        //                    rotation.x = BitConverter.ToSingle(byteData, 15 + packetOffset);
        //                    rotation.y = BitConverter.ToSingle(byteData, 19 + packetOffset);
        //                    rotation.z = BitConverter.ToSingle(byteData, 23 + packetOffset);
        //                    rotation.w = BitConverter.ToSingle(byteData, 27 + packetOffset);


        //                    MemoryStream stream = new MemoryStream(byteData, 0, numElements * 512);
        //                    BinaryReader reader = new BinaryReader(stream);

        //                    position.x = reader.ReadSingle();
        //                    position.y = reader.ReadSingle();
        //                    position.z = reader.ReadSingle();
        //                    rotation.x = reader.ReadSingle();
        //                    rotation.y = reader.ReadSingle();
        //                    rotation.z = reader.ReadSingle();
        //                    rotation.w = reader.ReadSingle();



        //                    Debug.Log("Pos: " + position.ToString());
        //                    Debug.Log("Rot: " + rotation.ToString());
        //                }
        //                break;
        //            case MessageTypes.Anim:
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}
        #endregion OLD_BYTE_TEST
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
    SerializedProperty _connectButton;
    SerializedProperty _testObj;

    string _entListName = "Enter Save Name Here";

    private void OnEnable()
    {
        _intialized = serializedObject.FindProperty("_initialized");
        _connected = serializedObject.FindProperty("_connected");
        _ip = serializedObject.FindProperty("_ip");
        _id = serializedObject.FindProperty("_id");
        _connectButton = serializedObject.FindProperty("_connectButton");
        _testObj = serializedObject.FindProperty("_testObj");
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

        label.text = "Connect Button";
        EditorGUILayout.PropertyField(_connectButton, label);

        label.text = "_testObj";
        EditorGUILayout.PropertyField(_testObj, label);

        //EditorGUILayout.ObjectField(label, _shader.Settings, typeof(RayMarchShaderSettings), true) as RayMarchShaderSettings;

        EditorGUILayout.Space(20.0f);

        label.text = "Entity List Name";
        _entListName = EditorGUILayout.TextField(label, _entListName);

        if (GUILayout.Button("Save Entity List"))
        {
            NetworkEntityList asset = CreateInstance<NetworkEntityList>();

            NetworkObject[] netObjs = FindObjectsOfType<NetworkObject>();

            asset.NetObjs = new List<NetworkObject>(netObjs);

            AssetDatabase.CreateAsset(asset, "Assets/Scripts/Networking/" + _entListName + ".asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }


        serializedObject.ApplyModifiedProperties();
    }
}
#endif
