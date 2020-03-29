using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;


public enum PrefabTypes : byte
{
    Brother,
    BrotherPawn,
    Sister,
    SisterPawn,
    Rock,
    Tire,
    Rope
}

public enum Ownership : byte
{
    ClientOwned,
    ServerOwned,
    OtherClientOwned
}

[Flags]
public enum PacketOptions
{
    None = 0, // Custom name for "Nothing" option
    Transform = 1 << 0,
    Anim = 1 << 1,
    //AB = A | B, // Combination of two flags
    All = ~0, // Custom name for "Everything" option
}

public class NetworkObject : MonoBehaviour
{
    //[SerializeField]
    //NetworkOp _networkOp;

    NetworkManager _networkManager;

    [SerializeField]
    byte _EID = 0;
    [SerializeField]
    PrefabTypes _prefabType = PrefabTypes.Sister;
    [SerializeField]
    Ownership _ownership = Ownership.ClientOwned;

    [SerializeField]
    PacketOptions _packetOptions;

    [SerializeField]
    private Vector3 _oldPosition;
    [SerializeField]
    private Quaternion _oldRotation;
    [SerializeField]
    private Vector3 _futurePosition;
    [SerializeField]
    private Quaternion _futureRotation;
    [SerializeField]
    private float _futureTime;

    Animator _animator;
    Rigidbody _rigidBody;
    int _prevAnimState = -1;
    

    public byte EID
    {
        get
        {
            return _EID;
        }
        set
        {
            _EID = value;
        }
    }

    public PrefabTypes PrefabType
    {
        get
        {
            return _prefabType;
        }
        set
        {
            _prefabType = value;
        }
    }

    public Ownership Ownership
    {
        get
        {
            return _ownership;
        }
        set
        {
            _ownership = value;
        }
    }

    public Animator Animator
    {
        get
        {
            return _animator;
        }
    }


    //const string DLL_NAME = "NETWORKINGDLL";

    //[DllImport(DLL_NAME)]
    //public static extern void sendData(ref Vector3 position, ref Quaternion rotation);

    //[DllImport(DLL_NAME)]
    //public static extern void receiveData(ref Vector3 position, ref Quaternion rotation);

    // Start is called before the first frame update
    void Start()
    {
        _networkManager = GameObject.Find("Game Manager").GetComponent<NetworkManager>();

        //_networkManager.onServerConnect += onServerConnect;
        NetworkManager.onServerConnect += onServerConnect;

        switch (_ownership)
        {
            case Ownership.ClientOwned:
                _networkManager.onDataSend += sendData;
                break;
            case Ownership.OtherClientOwned:
                _networkManager.onDataReceive += receiveData;
                break;
            default:
                _networkManager.onDataSend += sendData;
                _networkManager.onDataReceive += receiveData;
                break;
        }

        //_networkManager.NetworkObjects.Add(this);

        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void OnApplicationQuit()
    {
        //_networkManager.onServerConnect -= onServerConnect;
        NetworkManager.onServerConnect -= onServerConnect;

        switch (_ownership)
        {
            case Ownership.ClientOwned:
                _networkManager.onDataSend -= sendData;
                break;
            case Ownership.OtherClientOwned:
                _networkManager.onDataReceive -= receiveData;
                break;
            default:
                _networkManager.onDataSend -= sendData;
                _networkManager.onDataReceive -= receiveData;
                break;
        }
    }

    private void OnDestroy()
    {
        NetworkManager.onServerConnect -= onServerConnect;

        switch (_ownership)
        {
            case Ownership.ClientOwned:
                _networkManager.onDataSend -= sendData;
                break;
            case Ownership.OtherClientOwned:
                _networkManager.onDataReceive -= receiveData;
                break;
            default:
                _networkManager.onDataSend -= sendData;
                _networkManager.onDataReceive -= receiveData;
                break;
        }
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    void onServerConnect()
    {
        Debug.Log("Network Object Ready: " + _ownership);
    }

    void sendData()
    {
        // Send all packet types.
        if (_packetOptions == PacketOptions.All)
        {
            IntPtr dataPtr;

            if ((transform.position - _oldPosition).sqrMagnitude >= 0.25f || Quaternion.Dot(transform.rotation, _oldRotation) <= 0.95f)
            {
                TransformData transData = new TransformData()
                { _EID = _EID, _pos = transform.position, _rot = transform.rotation, _vel = _rigidBody.velocity };
                dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());

                Marshal.StructureToPtr(transData, dataPtr, false);
                //Marshal.PtrToStructure(dataPtr, transData);
                _networkManager.sendData(PacketTypes.TransformMsg, dataPtr);

                Marshal.FreeHGlobal(dataPtr);
                _oldPosition = transform.position;
                _oldRotation = transform.rotation;
                Debug.Log("Threshold breched");
            }

            int state = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

            if (_prevAnimState != state)
            {
                //Debug.Log("EID " + _EID + " Anim State: " + state);
                _prevAnimState = state;

                AnimData animData = new AnimData() { _EID = _EID, _state = state };
                dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());

                Marshal.StructureToPtr(animData, dataPtr, false);
                //Marshal.PtrToStructure(dataPtr, transData);
                _networkManager.sendData(PacketTypes.Anim, dataPtr);

                Marshal.FreeHGlobal(dataPtr);
            }
        }
        else
        {
            // Send transform packets.
            if ((_packetOptions & PacketOptions.Transform) == PacketOptions.Transform)
            {
                if ((transform.position - _oldPosition).sqrMagnitude >= 0.25f || Quaternion.Dot(transform.rotation, _oldRotation) <= 0.95f)
                {
                    TransformData transData = new TransformData()
                    { _EID = _EID, _pos = transform.position, _rot = transform.rotation, _vel = _rigidBody.velocity };
                    IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());

                    Marshal.StructureToPtr(transData, dataPtr, false);
                    //Marshal.PtrToStructure(dataPtr, transData);
                    _networkManager.sendData(PacketTypes.TransformMsg, dataPtr);

                    Marshal.FreeHGlobal(dataPtr);
                    _oldPosition = transform.position;
                    _oldRotation = transform.rotation;
                    Debug.Log("Threshold breched");
                }
            }
            // Send animation packets.
            else if ((_packetOptions & PacketOptions.Anim) == PacketOptions.Anim)
            {
                int state = _animator.GetCurrentAnimatorStateInfo(0).tagHash;
                
                if (_prevAnimState != state)
                {
                    Debug.Log("EID " + _EID + " Anim State: " + state);
                    _prevAnimState = state;

                    AnimData animData = new AnimData() { _EID = _EID, _state = state };
                    IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());

                    Marshal.StructureToPtr(animData, dataPtr, false);
                    //Marshal.PtrToStructure(dataPtr, transData);
                    _networkManager.sendData(PacketTypes.Anim, dataPtr);

                    Marshal.FreeHGlobal(dataPtr);
                }
            }
        }
    }


    void receiveData()
    {
        //_networkManager.receiveUDPData();


        //int transDataElements = -1;
        //int animDataElements = -1;
        //IntPtr transDataHandle;
        //IntPtr animDataHandle;

        //_networkManager.getPacketHandles(ref transDataElements, transDataHandle, ref animDataElements, animDataHandle);


        //TransformData[] transData = new TransformData[transDataElements];
        //AnimData[] animData = new AnimData[animDataElements];


        //transDataHandle = _networkManager.getTransformHandle();


        //if (transDataElements > 0)
        //{
        //    transData[0] = (TransformData)Marshal.PtrToStructure(transDataHandle, typeof(TransformData));
        //    transDataHandle += Marshal.SizeOf(typeof(TransformData));
        //    Debug.Log("objID: " + transData[0].objID);
        //    Debug.Log("Pos: " + transData[0].pos.ToString());
        //    Debug.Log("Rot: " + transData[0].rot.ToString());
        //}

        

        //_networkManager.packetHandlesCleanUp();



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

    public void deadReckon(TransformData _transData)
    {
        transform.position = _transData._pos;
        transform.rotation = _transData._rot;

        _futurePosition = transform.position + _transData._vel * _networkManager.UpdateInterval;
        _futureTime = Time.time + _networkManager.UpdateInterval;
    }

    private void Update()
    {
        if (_ownership != Ownership.ClientOwned)
        {
            if (Time.time > _futureTime)
            {
                if ((transform.position - _futurePosition).sqrMagnitude >= 0.25f)
                {
                    transform.position = _futurePosition;
                }
                return;
            }
            transform.position = Vector3.Lerp(_oldPosition, _futurePosition, (_futureTime - Time.time) / _futureTime);
        }
        else
        {
            Vector3 _playerPrediction = Vector3.Lerp(_oldPosition, _futurePosition, (_futureTime - Time.time) / _futureTime);
            //if ((transform.position - _playerPrediction).sqrMagnitude >= ())
        }
    }
}
