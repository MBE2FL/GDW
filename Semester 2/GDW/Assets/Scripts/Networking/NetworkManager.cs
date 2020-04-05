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
using UnityEngine.SceneManagement;



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
    ErrorMsg,
    Score,
    ClientScoresRequest,
    LobbyChat,
    LobbyTeamName,
    LobbyCharChoice
}

public enum ConnectionStatus : byte
{
    Connected,
    Connecting,
    Disconected,
    ConnectionFailedStatus,
    ServerFullStatus,
};

public enum CharacterChoices : byte
{
    NoChoice,
    SisterChoice,
    BrotherChoice
};


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

[StructLayout(LayoutKind.Sequential)]
public struct ScoreData
{
    public byte _EID;
    public PlayerTime _time;
}

[StructLayout(LayoutKind.Sequential)]
public struct CharChoiceData
{
    public byte _EID;
    public CharacterChoices _charChoice;
};



struct ConnectJob : IJob
{
    //[NativeDisableUnsafePtrRestriction]
    //public IntPtr _ip;



    public void Execute()
    {
        // Attempt to establish a connection to the server.
        NetworkManager.Connecting = true;
        NetworkManager.connectToServer(NetworkManager.IP);

        Debug.Log("ConnectJob Stopped");
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



    public delegate bool initNetworkDelegate();
    public initNetworkDelegate initNetwork;

    public delegate void networkCleanupDelegate();
    public networkCleanupDelegate networkCleanup;

    public delegate void connectToServerDelegate(string ip);
    public static connectToServerDelegate connectToServer;

    public delegate void queryConnectAttemptDelegate(ref int id, ref ConnectionStatus status);
    public queryConnectAttemptDelegate queryConnectAttempt;

    public delegate void queryEntityRequestDelegate(ref PacketTypes query);
    public static queryEntityRequestDelegate queryEntityRequest;

    //public delegate PacketTypes sendStarterEntitiesDelegate(IntPtr entities, int numEntities);
    //public static sendStarterEntitiesDelegate sendStarterEntities;

    //public delegate PacketTypes sendRequiredEntitiesDelegate(IntPtr entities, ref int numEntities, ref int numServerEntities);
    //public static sendRequiredEntitiesDelegate sendRequiredEntities;

    public delegate PacketTypes sendEntitiesDelegate(IntPtr entities, ref int numEntities);
    public static sendEntitiesDelegate sendEntities;

    public delegate void getServerEntitiesDelegate(IntPtr serverEntities, ref int numServerEntities);
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


    public delegate void requestScoresDelegate();
    public requestScoresDelegate requestScores;

    public delegate void getNumScoresDelegate(ref int numScores);
    public getNumScoresDelegate getNumScores;

    public delegate IntPtr getScoresHandleDelegate();
    public getScoresHandleDelegate getScoresHandle;

    public delegate void cleanupScoresHandleDelegate();
    public cleanupScoresHandleDelegate cleanupScoresHandle;


    public delegate void receiveLobbyDataDelegate();
    public static receiveLobbyDataDelegate receiveLobbyData;

    public delegate void stopLobbyReceiveDelegate();
    public static stopLobbyReceiveDelegate stopLobbyReceive;

    public delegate void getNumLobbyPacketsDelegate(ref int numMsgs, ref int newTeamNameMsg, ref int newCharChoice, ref int numNewPlayers);
    public getNumLobbyPacketsDelegate getNumLobbyPackets;

    public delegate void getLobbyPacketHandlesDelegate(IntPtr dataHandle);
    public getLobbyPacketHandlesDelegate getLobbyPacketHandles;
    #endregion DLL_VARIABLES



    [SerializeField]
    bool _initialized = false;

    [SerializeField]
    bool _connected = false;

    static bool _connecting = false;

    static string _ip = "127.0.0.1";

    [SerializeField]
    int _id = 0;

    [SerializeField]
    ConnectionStatus _status = ConnectionStatus.Disconected;


    public static event Action onServerConnect;
    public static event Action onServerConnectFail;
    public event Action onDataSend;
    public event Action onDataReceive;


    GameManager _gameManager;
    Lobby _lobby;


    [SerializeField]
    float _timeSinceLastUpdate = 0.0f;
    float _time = 0.0f;
    [SerializeField]
    float _updateInterval = 0.066f; // 1s / 15ups = 0.066ms
    //float _updateInterval = 0.167f;
    float _lagTime = 0.0f;


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

    public float LagTime
    {
        get
        {
            return _lagTime;
        }
    }

    public int ID
    {
        get
        {
            return _id;
        }
    }

    public static bool Connecting
    {
        get
        {
            return _connecting;
        }
        set
        {
            _connecting = value;
        }
    }

    public static string IP
    {
        get
        {
            return _ip;
        }
        set
        {
            _ip = value;
        }
    }


    JobHandle connectJobHandle;
    JobHandle receiveTCPJobHandle;
    JobHandle receiveUDPJobHandle;

    public static bool stopJobs = false;

    public bool Connected
    {
        get
        {
            return _connected;
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


        initNetwork = ManualPluginImporter.GetDelegate<initNetworkDelegate>(_pluginHandle, "initNetwork");
        networkCleanup = ManualPluginImporter.GetDelegate<networkCleanupDelegate>(_pluginHandle, "networkCleanup");
        connectToServer = ManualPluginImporter.GetDelegate<connectToServerDelegate>(_pluginHandle, "connectToServer");
        queryConnectAttempt = ManualPluginImporter.GetDelegate<queryConnectAttemptDelegate>(_pluginHandle, "queryConnectAttempt");
        queryEntityRequest = ManualPluginImporter.GetDelegate<queryEntityRequestDelegate>(_pluginHandle, "queryEntityRequest");
        //sendStarterEntities = ManualPluginImporter.GetDelegate<sendStarterEntitiesDelegate>(_pluginHandle, "sendStarterEntities");
        //sendRequiredEntities = ManualPluginImporter.GetDelegate<sendRequiredEntitiesDelegate>(_pluginHandle, "sendRequiredEntities");
        sendEntities = ManualPluginImporter.GetDelegate<sendEntitiesDelegate>(_pluginHandle, "sendEntities");
        getServerEntities = ManualPluginImporter.GetDelegate<getServerEntitiesDelegate>(_pluginHandle, "getServerEntities");


        sendData = ManualPluginImporter.GetDelegate<sendDataDelegate>(_pluginHandle, "sendData");

        receiveUDPData = ManualPluginImporter.GetDelegate<receiveUDPDataDelegate>(_pluginHandle, "receiveUDPData");
        receiveTCPData = ManualPluginImporter.GetDelegate<receiveTCPDataDelegate>(_pluginHandle, "receiveTCPData");
        getPacketHandleSizes = ManualPluginImporter.GetDelegate<getPacketHandleSizesDelegate>(_pluginHandle, "getPacketHandleSizes");
        getPacketHandles = ManualPluginImporter.GetDelegate<getPacketHandlesDelegate>(_pluginHandle, "getPacketHandles");


        requestScores = ManualPluginImporter.GetDelegate<requestScoresDelegate>(_pluginHandle, "requestScores");
        getNumScores = ManualPluginImporter.GetDelegate<getNumScoresDelegate>(_pluginHandle, "getNumScores");
        getScoresHandle = ManualPluginImporter.GetDelegate<getScoresHandleDelegate>(_pluginHandle, "getScoresHandle");
        cleanupScoresHandle = ManualPluginImporter.GetDelegate<cleanupScoresHandleDelegate>(_pluginHandle, "cleanupScoresHandle");


        receiveLobbyData = ManualPluginImporter.GetDelegate<receiveLobbyDataDelegate>(_pluginHandle, "receiveLobbyData");
        stopLobbyReceive = ManualPluginImporter.GetDelegate<stopLobbyReceiveDelegate>(_pluginHandle, "stopLobbyReceive");
        getNumLobbyPackets = ManualPluginImporter.GetDelegate<getNumLobbyPacketsDelegate>(_pluginHandle, "getNumLobbyPackets");
        getLobbyPacketHandles = ManualPluginImporter.GetDelegate<getLobbyPacketHandlesDelegate>(_pluginHandle, "getLobbyPacketHandles");
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


        //DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        initializeNetworkManager();

        _gameManager = GetComponent<GameManager>();

        GameManager.onPlay += play;


        _lobby = GetComponent<Lobby>();


        SceneManager.sceneLoaded += onSceneLoaded;
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


        SceneManager.sceneLoaded -= onSceneLoaded;


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
        if (Input.GetKeyDown(KeyCode.R))
        {
            _lagTime += 1.0f;
            Debug.Log("Update Interval: " + (_updateInterval + _lagTime));
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            _lagTime -= 1.0f;
            Debug.Log("Update Interval: " + (_updateInterval + _lagTime));
        }

        // Connecting to server
        if (!_connected && _connecting)
        {
            // Connected to server
            //if (queryConnectAttempt(ref _id))
            //{
            //    // Notify all listeners.
            //    if (onServerConnect != null)
            //        onServerConnect.Invoke();
            //}
            queryConnectAttempt(ref _id, ref _status);

            switch (_status)
            {
                case ConnectionStatus.Connected:
                    // Notify all listeners.
                    if (onServerConnect != null)
                        onServerConnect.Invoke();
                    break;
                case ConnectionStatus.Connecting:
                    Debug.Log("Connecting...");
                    break;
                case ConnectionStatus.Disconected:
                    _connecting = false;
                    Debug.Log("Disconnected");

                    if (onServerConnectFail != null)
                        onServerConnectFail.Invoke();
                    break;
                case ConnectionStatus.ConnectionFailedStatus:
                    _connecting = false;
                    _status = ConnectionStatus.Disconected;
                    Debug.Log("Failed to connect to the server!");

                    if (onServerConnectFail != null)
                        onServerConnectFail.Invoke();
                    break;
                case ConnectionStatus.ServerFullStatus:
                    _connecting = false;
                    _status = ConnectionStatus.Disconected;
                    Debug.Log("The server is full!");

                    if (onServerConnectFail != null)
                        onServerConnectFail.Invoke();
                    break;
                default:
                    break;
            }
        }


        // Don't perform any game update loops, unless the client is connected and in a level.
        if (!_connected && !_gameManager.LevelInProgress)
            return;


        _time -= Time.deltaTime;

        if (_time <= 0.0f)
        {

            // Send this player's transform data to server.
            if (onDataSend != null)
                onDataSend.Invoke();


            //// Retrieve server interpolated transform data of other player.
            ////if (onDataReceive != null)
            ////    onDataReceive.Invoke();
            receivePackets();

            _time = _updateInterval + _lagTime;
        }
    }

    void initializeNetworkManager()
    {
        _initialized = initNetwork();
    }

    public void connect(string ip)
    {
        // Only attempt to establish a connection to the server iff, no connection has already been made.
        if (!_connected && !_connecting)
        {
            _ip = ip;

            ConnectJob job = new ConnectJob();
            //job._ip = Marshal.StringToHGlobalAnsi(_ip);
            connectJobHandle = job.Schedule();

            //_connecting = true;
        }
    }

    public void connectServerSuccess()
    {
        _connected = true;
        _connecting = false;


        //_connectButton.SetActive(false);
        // For some reason won't turn on/off game objects.
        //_connectingButton.SetActive(false);


        Debug.Log("Successfully connected to server.");
    }

    public void play()
    {
        // Load the first level.
        SceneManager.LoadScene("regan's test scene");
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.SetActiveScene(scene);

        loadEntities();

        Camera.main.GetComponent<cameraMovement>().setPlayer(_lobby.CharChoice);
    }

    public void loadEntities()
    {
        // Receive server's entity request.
        PacketTypes pckType = PacketTypes.EmptyMsg;
        queryEntityRequest(ref pckType);

        if (pckType != PacketTypes.ErrorMsg)
        {
            float timer = 0.0f;
            while ((pckType == PacketTypes.EntitiesQuery) && (timer <= 6.0f))
            {
                if (timer >= 3.0f)
                {
                     queryEntityRequest(ref pckType);

                    if (pckType == PacketTypes.ErrorMsg)
                        break;

                    timer = 0.0f;
                }

                timer += Time.deltaTime;
            }
        }

        Debug.Log(pckType);

        //List<NetworkObject> netObjsList = new List<NetworkObject>();
        //EntityData[] entityData;
        List<EntityData> entityData = new List<EntityData>();
        int numEntities = 0;

        // Server is requesting the starting entities list.
        if (pckType == PacketTypes.EntitiesStart)
        {
            Debug.Log("Starter entities message type received.");


            // Retrieve all networked objects in the scene.
            NetworkObject[] netObjs = FindObjectsOfType<NetworkObject>();
            //netObjsList.AddRange(netObjs);

            // Add any objects not in the scene.
            EntityData entity;
            if (_lobby.CharChoice == CharacterChoices.SisterChoice)
                entity = new EntityData()
                {
                    _EID = 0,
                    _entityPrefabType = PrefabTypes.SisterV2,
                    _ownership = Ownership.ClientOwned,
                    _parent = 0,
                    _position = new Vector3(37.0f, 0.0f, -21.0f),
                    _rotation = Quaternion.Euler(0.0f, -52.0f, 0.0f)
                };
            else
                entity = new EntityData()
                {
                    _EID = 0,
                    _entityPrefabType = PrefabTypes.BrotherV2,
                    _ownership = Ownership.ClientOwned,
                    _parent = 0,
                    _position = new Vector3(38.0f, 0.0f, -17.0f),
                    _rotation = Quaternion.Euler(0.0f, -115.0f, 0.0f)
                };

            entityData.Add(entity);
            //numEntities = netObjsList.Count;
            //entityData = new EntityData[numEntities];


            foreach (NetworkObject netObj in netObjs)
            {
                entity = new EntityData()
                {
                    _EID = netObj.EID,
                    _entityPrefabType = netObj.PrefabType,
                    _ownership = netObj.Ownership,
                    _parent = 0,
                    _position = netObj.transform.position,
                    _rotation = netObj.transform.rotation
                };

                entityData.Add(entity);
            }


            numEntities = entityData.Count;

            sendEntitiesToServer(entityData, numEntities);

            // Destroy all placeholder network objects.
            foreach (NetworkObject netObj in netObjs)
            {
                Destroy(netObj.gameObject);
            }

            receiveEntitiesFromServer();
        }
        // Server is requesting the required entities list.
        else if (pckType == PacketTypes.EntitiesRequired)
        {
            Debug.Log("Required entities message type received.");

            // Destroy all placeholder network objects.
            NetworkObject[] netObjs = FindObjectsOfType<NetworkObject>();
            foreach (NetworkObject netObj in netObjs)
            {
                Destroy(netObj.gameObject);
            }


            //numEntities = netObjsList.Count;
            //entityData = new EntityData[numEntities];
            //int numServerEntities = 0;


            EntityData entity;
            if (_lobby.CharChoice == CharacterChoices.SisterChoice)
                entity = new EntityData()
                {
                    _EID = 0,
                    _entityPrefabType = PrefabTypes.SisterV2,
                    _ownership = Ownership.ClientOwned,
                    _parent = 0,
                    _position = new Vector3(37.0f, 0.0f, -21.0f),
                    _rotation = Quaternion.Euler(0.0f, -52.0f, 0.0f)
                };
                //netObjsList.Add(Instantiate(_networkPrefabs[PrefabTypes.SisterV2], new Vector3(37.0f, 0.0f, -21.0f), Quaternion.Euler(0.0f, -52.0f, 0.0f)));
            else
                entity = new EntityData()
                {
                    _EID = 0,
                    _entityPrefabType = PrefabTypes.BrotherV2,
                    _ownership = Ownership.ClientOwned,
                    _parent = 0,
                    _position = new Vector3(38.0f, 0.0f, -17.0f),
                    _rotation = Quaternion.Euler(0.0f, -115.0f, 0.0f)
                };
            //netObjsList.Add(Instantiate(_networkPrefabs[PrefabTypes.BrotherV2], new Vector3(38.0f, 0.0f, -17.0f), Quaternion.Euler(0.0f, -115.0f, 0.0f)));


            entityData.Add(entity);

            sendEntitiesToServer(entityData, entityData.Count);

            receiveEntitiesFromServer();
        }
        else if (pckType == PacketTypes.EmptyMsg)
        {
            Debug.Log("Empty message type received.");
            return;
        }
        else if (pckType == PacketTypes.ErrorMsg)
        {
            Debug.Log("Error message type received.");
            return;
        }
        else
        {
            Debug.Log("Wrong message type received: " + pckType);
            return;
        }



        _lobby.stopLobby();

        ReceiveTCPJob receiveTCPJob = new ReceiveTCPJob();
        receiveTCPJobHandle = receiveTCPJob.Schedule();

        ReceiveUDPJob receiveUDPJob = new ReceiveUDPJob();
        receiveUDPJobHandle = receiveUDPJob.Schedule();
    }

    void sendEntitiesToServer(List<EntityData> entityData, int numEntities)
    {
        EntityData[] entityDataArr = entityData.ToArray();
        //unsafe
        //{
        //    fixed (EntityData* tempPtr = entityDataArr)
        //    {
        //        IntPtr entitiesPtr = new IntPtr(tempPtr);
        //        // Send starting entity list to the server.
        //        //sendStarterEntities(entitiesPtr, numEntities);
        //        sendEntities(entitiesPtr, ref numEntities);
        //    }
        //}

        // Send entities to the server.
        GCHandle gcHandle = GCHandle.Alloc(entityDataArr, GCHandleType.Pinned);

        sendEntities(gcHandle.AddrOfPinnedObject(), ref numEntities);

        gcHandle.Free();
    }

    void receiveEntitiesFromServer()
    {
        // Assign networked objects their server generated entity IDs.
        int numEntities = 0;
        int entityDataSize = Marshal.SizeOf<EntityData>();
        getServerEntities(IntPtr.Zero, ref numEntities);

        //float timer = 0.0f;
        //while ((numEntities <= 0) && (timer <= 4.0f))
        //{
        //    if (timer >= 2.0f)
        //    {
        //        getServerEntities(IntPtr.Zero, ref numEntities);

        //        if (numEntities >= 0)
        //            break;

        //        timer = 0.0f;
        //    }

        //    timer += 2.0f;
        //}


        if (numEntities <= 0)
        {
            Debug.Log("No Server Entities Received.");
            return;
        }


        IntPtr dataHandle = Marshal.AllocHGlobal(entityDataSize * numEntities);
        IntPtr tempDataHandle = dataHandle;
        getServerEntities(dataHandle, ref numEntities);



        _networkObjects.Clear();
        NetworkObject networkObj = null;
        EntityData entity;
        PrefabTypes prefabType;
        Ownership ownership;

        for (int i = 0; i < numEntities; ++i)
        {
            entity = (EntityData)Marshal.PtrToStructure(dataHandle, typeof(EntityData));
            dataHandle += entityDataSize;

            //Debug.Log("Server Entity: " + entity._entityID);
            //Debug.Log("Pos: " + entity._position);

            prefabType = entity._entityPrefabType;
            ownership = entity._ownership;


            if ((prefabType == PrefabTypes.SisterV2) && (_lobby.CharChoice == CharacterChoices.BrotherChoice))
            {
                prefabType = PrefabTypes.SisterV2Pawn;
                ownership = Ownership.OtherClientOwned;
            }
            else if ((prefabType == PrefabTypes.BrotherV2) && (_lobby.CharChoice == CharacterChoices.SisterChoice))
            {
                prefabType = PrefabTypes.BrotherV2Pawn;
                ownership = Ownership.OtherClientOwned;
            }


            networkObj = Instantiate(_networkPrefabs[prefabType], entity._position, entity._rotation);

            networkObj.EID = entity._EID;
            networkObj.PrefabType = prefabType;
            networkObj.Ownership = ownership;

            // Map network object to it's entity ID.
            _networkObjects.Add(networkObj.EID, networkObj);
        }

        Marshal.FreeHGlobal(tempDataHandle);
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


            if (prefabType == PrefabTypes.SisterV2)
            {
                prefabType = PrefabTypes.SisterV2Pawn;
                ownership = Ownership.OtherClientOwned;
            }
            else if (prefabType == PrefabTypes.BrotherV2)
            {
                prefabType = PrefabTypes.BrotherV2Pawn;
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
    SerializedProperty _status;
    //SerializedProperty _ip;
    SerializedProperty _id;

    string _entListName = "Enter Save Name Here";

    private void OnEnable()
    {
        _intialized = serializedObject.FindProperty("_initialized");
        _connected = serializedObject.FindProperty("_connected");
        _status = serializedObject.FindProperty("_status");
        //_ip = serializedObject.FindProperty("_ip");
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

        EditorGUI.BeginDisabledGroup(true);
        label.text = "Status";
        EditorGUILayout.PropertyField(_status, label);
        EditorGUI.EndDisabledGroup();

        label.text = "IP Address " + NetworkManager.IP;
        //EditorGUILayout.PropertyField(_ip, label);
        EditorGUILayout.LabelField(label);

        label.text = "Network ID";
        EditorGUILayout.PropertyField(_id, label);

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
