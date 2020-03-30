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


public enum PacketTypes : byte
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
    public byte _EID;
    public Vector3 _pos;
    public Quaternion _rot;
    public Vector3 _vel;
}

[StructLayout(LayoutKind.Sequential)]
public struct AnimData
{
    public byte _EID;
    public int _state;
}

[StructLayout(LayoutKind.Sequential)]
public struct EntityData
{
    public byte _EID;
    public PrefabTypes _entityPrefabType;
    public Ownership _ownership;
    public Vector3 _position;
    public Quaternion _rotation;
    public byte _parent;
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

struct ReceiveTCPJob : IJob
{
    public void Execute()
    {
        while (!NetworkManager.stopJobs)
        {
            NetworkManager.receiveTCPData();
        }

        Debug.Log("ReceiveTCPJob Stopped");
    }
}

struct ReceiveUDPJob : IJob
{
    public void Execute()
    {
        while (!NetworkManager.stopJobs)
        {
            NetworkManager.receiveUDPData();
        }

        Debug.Log("ReceiveUDPJob Stopped");
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



    public delegate bool initNetworkDelegate(string ip);
    public initNetworkDelegate initNetwork;

    public delegate void networkCleanupDelegate();
    public networkCleanupDelegate networkCleanup;

    public delegate bool connectToServerDelegate(string ip);
    public static connectToServerDelegate connectToServer;

    public delegate bool queryConnectAttemptDelegate(ref int id);
    public queryConnectAttemptDelegate queryConnectAttempt;

    public delegate PacketTypes queryEntityRequestDelegate();
    public static queryEntityRequestDelegate queryEntityRequest;

    public delegate bool sendStarterEntitiesDelegate(IntPtr entities, int numEntities);
    public static sendStarterEntitiesDelegate sendStarterEntities;

    public delegate bool sendRequiredEntitiesDelegate(IntPtr entities, ref int numEntities, ref int numServerEntities);
    public static sendRequiredEntitiesDelegate sendRequiredEntities;

    public delegate void getServerEntitiesDelegate(IntPtr serverEntities);
    public getServerEntitiesDelegate getServerEntities;


    public delegate void sendDataDelegate(PacketTypes pckType, IntPtr data);
    public sendDataDelegate sendData;


    public delegate void receiveUDPDataDelegate();
    public static receiveUDPDataDelegate receiveUDPData;

    public delegate void receiveTCPDataDelegate();
    public static receiveTCPDataDelegate receiveTCPData;

    public delegate void getPacketHandleSizesDelegate(ref int transDataElements, ref int animDataElements, ref int entityDataElements);
    public getPacketHandleSizesDelegate getPacketHandleSizes;

    public delegate void getPacketHandlesDelegate(IntPtr dataHandle);
    public getPacketHandlesDelegate getPacketHandles;
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
    Dictionary<byte, NetworkObject> _networkObjects = new Dictionary<byte, NetworkObject>();
    [SerializeField]
    Dictionary<PrefabTypes, NetworkObject> _networkPrefabs;

    public Dictionary<byte, NetworkObject> NetworkObjects
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

    public float UpdateInterval
    {
        get
        {
            return _updateInterval;
        }
    }


    JobHandle connectJobHandle;
    JobHandle receiveTCPJobHandle;
    JobHandle receiveUDPJobHandle;

    public static bool stopJobs = false;





    private void InitPluginFunctions()
    {
        // To get the function, add this line
        // FunctionName = ManualPluginImporter.GetDelegate<delegate_type>(Plugin_Handle, "NAME_OF_FUNCTION_IN_C++");
        // You can now call the function like any other. For example: FunctionName(int a, int b);
        InitPlugin = ManualPluginImporter.GetDelegate<InitPluginDelegate>(_pluginHandle, "InitPlugin");
        InitConsole = ManualPluginImporter.GetDelegate<InitConsoleDelegate>(_pluginHandle, "InitConsole");
        FreeTheConsole = ManualPluginImporter.GetDelegate<FreeTheConsoleDelegate>(_pluginHandle, "FreeTheConsole");
        OutputConsoleMessage = ManualPluginImporter.GetDelegate<OutputConsoleMessageDelegate>(_pluginHandle, "OutputMessageToConsole");


        initNetwork = ManualPluginImporter.GetDelegate<initNetworkDelegate>(_pluginHandle, "initNetwork");
        networkCleanup = ManualPluginImporter.GetDelegate<networkCleanupDelegate>(_pluginHandle, "networkCleanup");
        connectToServer = ManualPluginImporter.GetDelegate<connectToServerDelegate>(_pluginHandle, "connectToServer");
        queryConnectAttempt = ManualPluginImporter.GetDelegate<queryConnectAttemptDelegate>(_pluginHandle, "queryConnectAttempt");
        queryEntityRequest = ManualPluginImporter.GetDelegate<queryEntityRequestDelegate>(_pluginHandle, "queryEntityRequest");
        sendStarterEntities = ManualPluginImporter.GetDelegate<sendStarterEntitiesDelegate>(_pluginHandle, "sendStarterEntities");
        sendRequiredEntities = ManualPluginImporter.GetDelegate<sendRequiredEntitiesDelegate>(_pluginHandle, "sendRequiredEntities");
        getServerEntities = ManualPluginImporter.GetDelegate<getServerEntitiesDelegate>(_pluginHandle, "getServerEntities");


        sendData = ManualPluginImporter.GetDelegate<sendDataDelegate>(_pluginHandle, "sendData");

        receiveUDPData = ManualPluginImporter.GetDelegate<receiveUDPDataDelegate>(_pluginHandle, "receiveUDPData");
        receiveTCPData = ManualPluginImporter.GetDelegate<receiveTCPDataDelegate>(_pluginHandle, "receiveTCPData");
        getPacketHandleSizes = ManualPluginImporter.GetDelegate<getPacketHandleSizesDelegate>(_pluginHandle, "getPacketHandleSizes");
        getPacketHandles = ManualPluginImporter.GetDelegate<getPacketHandlesDelegate>(_pluginHandle, "getPacketHandles");
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


        DontDestroyOnLoad(this.gameObject);
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
        networkCleanup();

        stopJobs = true;

        connectJobHandle.Complete();
        receiveTCPJobHandle.Complete();
        receiveUDPJobHandle.Complete();




        onServerConnect -= connectServerSuccess;

        _networkObjects.Clear();

        GameManager.onPlay -= play;


        // Not sure if this works
        FreeTheConsole();

        // close the plugin
        // This will allow you to rebuild the dll while unity is open (but not while playing)
        ManualPluginImporter.CloseLibrary(_pluginHandle);
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
        if (Input.GetKeyDown(KeyCode.Greater))
        {
            _updateInterval *= 2.0f;
            Debug.Log("Update Interval: " + _updateInterval);
        }
        else if (Input.GetKeyDown(KeyCode.Less))
        {
            _updateInterval *= 0.5f;
            Debug.Log("Update Interval: " + _updateInterval);
        }

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

            // Send this player's transform data to server.
            if (onDataSend != null)
                onDataSend.Invoke();


            //// Retrieve server interpolated transform data of other player.
            ////if (onDataReceive != null)
            ////    onDataReceive.Invoke();
            receivePackets();

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




            //job._entities = Marshal.GetIUnknownForObject(entities);
            //job._entities = IntPtr.Zero;


            //job._networkManager;
            //Marshal.StructureToPtr<NetworkManager>(this, job._networkManager, false);


            connectJobHandle = job.Schedule();

            //jobHandle.Complete();


            _connecting = true;

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


        Debug.Log("Successfully connected to server.");
    }

    public void play()
    {
        PacketTypes pckType = queryEntityRequest();
        Debug.Log(pckType);

        List<NetworkObject> netObjsList = new List<NetworkObject>();
        EntityData[] entityData;
        int numEntities = 0;

        // Server is requesting the starting entities list.
        if (pckType == PacketTypes.EntitiesStart)
        {
            // Retrieve all networked objects in the scene.
            NetworkObject[] netObjs = FindObjectsOfType<NetworkObject>();
            netObjsList.AddRange(netObjs);

            // Add any objects not in the scene.
            netObjsList.Add(Instantiate(_networkPrefabs[PrefabTypes.Sister], Vector3.zero, Quaternion.identity));


            numEntities = netObjsList.Count;
            entityData = new EntityData[numEntities];


            int i = 0;
            foreach (NetworkObject netObj in netObjsList)
            {
                entityData[i]._EID = netObj.EID;
                entityData[i]._entityPrefabType = netObj.PrefabType;
                entityData[i]._ownership = netObj.Ownership;
                entityData[i]._position = netObj.transform.position;
                entityData[i]._rotation = netObj.transform.rotation;
                entityData[i]._parent = 0;

                ++i;
            }


            unsafe
            {
                fixed (EntityData* tempPtr = entityData)
                {
                    IntPtr entitiesPtr = new IntPtr(tempPtr);
                    // Send starting entity list to the server.
                    sendStarterEntities(entitiesPtr, numEntities);
                }
            }

            // Assign networked objects their server generated entity IDs.
            _networkObjects.Clear();
            NetworkObject networkObj = null;
            EntityData entity;
            for (int index = 0; index < numEntities; ++index)
            {
                entity = entityData[index];
                networkObj = netObjsList[index];

                networkObj.EID = entity._EID;

                // Map network object to it's entity ID.
                _networkObjects.Add(networkObj.EID, networkObj);
            }
        }
        // Server is requesting the required entities list.
        else if (pckType == PacketTypes.EntitiesRequired)
        {
            Debug.Log("Entities Required message type received.");

            NetworkObject[] netObjs = FindObjectsOfType<NetworkObject>();
            foreach (NetworkObject netObj in netObjs)
            {
                Destroy(netObj.gameObject);
            }

            netObjsList.Add(Instantiate(_networkPrefabs[PrefabTypes.Brother], new Vector3(3.0f, 0.0f, 0.0f), Quaternion.identity));


            numEntities = netObjsList.Count;
            entityData = new EntityData[numEntities];
            int numServerEntities = 0;

            int i = 0;
            foreach (NetworkObject netObj in netObjsList)
            {
                entityData[i]._EID = netObj.EID;
                entityData[i]._entityPrefabType = netObj.PrefabType;
                entityData[i]._ownership = netObj.Ownership;
                entityData[i]._position = netObj.transform.position;
                entityData[i]._rotation = netObj.transform.rotation;
                entityData[i]._parent = 0;

                ++i;
            }

            unsafe
            {
                fixed (EntityData* tempPtr = entityData)
                {
                    IntPtr entitiesPtr = new IntPtr(tempPtr);

                    // Send required entity list to the server.
                    sendRequiredEntities(entitiesPtr, ref numEntities, ref numServerEntities);
                }
            }

            // Assign networked objects their server generated entity IDs.
            _networkObjects.Clear();
            NetworkObject networkObj = null;
            EntityData entity;
            for (int index = 0; index < numEntities; ++index)
            {
                entity = entityData[index];
                networkObj = netObjsList[index];

                networkObj.EID = entity._EID;

                // Map network object to it's entity ID.
                _networkObjects.Add(networkObj.EID, networkObj);
            }



            // Process server entities
            //EntityData[] serverEntityData = new EntityData[numServerEntities];
            int entityDataSize = Marshal.SizeOf<EntityData>();

            IntPtr serverEntityPtr = Marshal.AllocHGlobal(entityDataSize * numServerEntities);

            PrefabTypes prefabType;
            Ownership ownership;

            // Get entities from the server.
            getServerEntities(serverEntityPtr);

            // Generate server entities for this client.
            IntPtr tempserverEntityPtr = serverEntityPtr;
            i = 0;
            for (; i < numServerEntities; ++i)
            {
                entity = (EntityData)Marshal.PtrToStructure(serverEntityPtr, typeof(EntityData));
                serverEntityPtr += entityDataSize;

                //Debug.Log("Server Entity: " + entity._entityID);
                //Debug.Log("Pos: " + entity._position);

                prefabType = entity._entityPrefabType;
                ownership = entity._ownership;


                if (prefabType == PrefabTypes.Sister)
                {
                    prefabType = PrefabTypes.SisterPawn;
                    ownership = Ownership.OtherClientOwned;
                }
                else if (prefabType == PrefabTypes.Brother)
                {
                    prefabType = PrefabTypes.BrotherPawn;
                    ownership = Ownership.OtherClientOwned;
                }


                networkObj = Instantiate(_networkPrefabs[prefabType], entity._position, entity._rotation);

                networkObj.EID = entity._EID;
                networkObj.PrefabType = prefabType;
                networkObj.Ownership = ownership;

                // Map network object to it's entity ID.
                _networkObjects.Add(networkObj.EID, networkObj);
            }

            //Array.Clear(serverEntityData, 0, numServerEntities);

            Marshal.FreeHGlobal(tempserverEntityPtr);
        }
        else if (pckType == PacketTypes.EmptyMsg)
        {
            Debug.Log("Empty message type received.");
        }
        else if (pckType == PacketTypes.ErrorMsg)
        {
            Debug.Log("Error message type received.");
        }
        else
        {
            Debug.Log("Wrong message type received: " + pckType);
        }


        ReceiveTCPJob receiveTCPJob = new ReceiveTCPJob();
        receiveTCPJobHandle = receiveTCPJob.Schedule();

        ReceiveUDPJob receiveUDPJob = new ReceiveUDPJob();
        receiveUDPJobHandle = receiveUDPJob.Schedule();
    }


    void receivePackets()
    {
        int transDataElements = 0;
        int animDataElements = 0;
        int entityDataElements = 0;
        IntPtr dataHandle;
        int transDataSize = Marshal.SizeOf<TransformData>();
        int animDataSize = Marshal.SizeOf<AnimData>();
        int entityDataSize = Marshal.SizeOf<EntityData>();


        getPacketHandleSizes(ref transDataElements, ref animDataElements, ref entityDataElements);

        dataHandle = Marshal.AllocHGlobal((transDataSize * transDataElements) + (animDataSize * animDataElements) + (entityDataSize * entityDataElements));

        getPacketHandles(dataHandle);


        //TransformData[] transData = new TransformData[transDataElements];
        //AnimData[] animData = new AnimData[animDataElements];
        //EntityData[] entityData = new EntityData[entityDataElements];

        IntPtr tempDataHandle = dataHandle;


        TransformData transData;
        //byte EID; // -1
        NetworkObject netObj;
        for (int i = 0; i < transDataElements; ++i)
        {
            transData = (TransformData)Marshal.PtrToStructure(dataHandle, typeof(TransformData));
            dataHandle += transDataSize;

            //netObj = _networkObjects[transData._EID];

            if (_networkObjects.TryGetValue(transData._EID, out netObj))
            {
                netObj.deadReckon(transData);
            }
        }

        AnimData animData;
        for (int i = 0; i < animDataElements; ++i)
        {
            animData = (AnimData)Marshal.PtrToStructure(dataHandle, typeof(AnimData));
            dataHandle += animDataSize;


            if (_networkObjects.TryGetValue(animData._EID, out netObj))
            {
                //netObj.GetComponent<Animator>().Play(animData._state, 0);
                netObj.Animator.Play(animData._state, 0);
                //if (Animator.StringToHash("Walking") == animData._state)
                //    netObj.GetComponent<Animator>().Play(animData._state);
                //else if (Animator.StringToHash("Idle") == animData._state)
                //    netObj.GetComponent<Animator>().Play(animData._state);
                //else
                //    Debug.LogError("State hashes does not match: Walking " + animData._state + " != " + Animator.StringToHash("Walking")
                //        + "/n or Idle " + animData._state + " != " + Animator.StringToHash("Idle"));
            }
        }

        PrefabTypes prefabType;
        Ownership ownership;
        EntityData entity;
        NetworkObject networkObj;
        for (int i = 0; i < entityDataElements; ++i)
        {
            entity = (EntityData)Marshal.PtrToStructure(dataHandle, typeof(EntityData));
            dataHandle += entityDataSize;


            prefabType = entity._entityPrefabType;
            ownership = entity._ownership;


            if (prefabType == PrefabTypes.Sister)
            {
                prefabType = PrefabTypes.SisterPawn;
                ownership = Ownership.OtherClientOwned;
            }
            else if (prefabType == PrefabTypes.Brother)
            {
                prefabType = PrefabTypes.BrotherPawn;
                ownership = Ownership.OtherClientOwned;
            }


            networkObj = Instantiate(_networkPrefabs[prefabType], entity._position, entity._rotation);

            networkObj.EID = entity._EID;
            networkObj.PrefabType = prefabType;
            networkObj.Ownership = ownership;

            Debug.Log("Entity Received: ");
            Debug.Log("EID: " + entity._EID);
            Debug.Log("Prefab Type: " + entity._entityPrefabType);
            Debug.Log("Ownership: " + entity._ownership);

            // Map network object to it's entity ID.
            _networkObjects.Add(networkObj.EID, networkObj);
        }




        // Clean up
        //Array.Clear(transData, 0, transData.Length);
        //Array.Clear(animData, 0, animData.Length);
        //Array.Clear(entityData, 0, entityData.Length);

        Marshal.FreeHGlobal(tempDataHandle);



        #region OLD_BYTE_TEST
        //Vector3 position = Vector3.zero;
        //Quaternion rotation = Quaternion.identity;

        ////_networkManager.receiveData(ref position, ref rotation);

        ////transform.position = position;
        ////transform.rotation = rotation;


        //PacketTypes pckType = PacketTypes.ConnectionAttempt;
        //int objID = -1;
        //IntPtr data = IntPtr.Zero;
        //int numElements = -1;
        //byte[] byteData;


        //_networkManager.receiveData(ref pckType, ref objID, ref data);


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

        //        pckType = (PacketTypes)byteData[packetOffset];


        //        switch (pckType)
        //        {
        //            case PacketTypes.TransformMsg:
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
        //            case PacketTypes.Anim:
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

    string _entListName = "Enter Save Name Here";

    private void OnEnable()
    {
        _intialized = serializedObject.FindProperty("_initialized");
        _connected = serializedObject.FindProperty("_connected");
        _ip = serializedObject.FindProperty("_ip");
        _id = serializedObject.FindProperty("_id");
        _connectButton = serializedObject.FindProperty("_connectButton");
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
