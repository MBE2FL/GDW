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
    RopeCoil,
    SisterV2,
    SisterV2Pawn,
    BrotherV2,
    BrotherV2Pawn
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
    private Vector3 _lastKnownPos;
    [SerializeField]
    private Quaternion _lastKnownRot;
    [SerializeField]
    private Vector3 _lastKnownVel;
    [SerializeField]
    float _lastKnowTime = 0.0f;

    [SerializeField]
    Vector3 _playerPrediction;

    [SerializeField]
    Vector3 _prevPos;
    [SerializeField]
    Quaternion _prevRot;
    [SerializeField]
    Vector3 _prevVel;

    [SerializeField]
    float _time = 0.0f;

    [SerializeField]
    bool _thresholdPassed = false;
    [SerializeField]
    float _threshold = 1.5f;


    [SerializeField]
    float _timeToConverge = 1.0f;
    [SerializeField]
    float _convergeTimer = 0.0f;


    Animator _animator;
    Rigidbody _rigidBody;
    float _prevAnimState = -1.0f;
    

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

        _lastKnownPos = transform.position;
    }

    private void OnApplicationQuit()
    {
        if (!_networkManager)
            return;

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
        if (!_networkManager)
            return;

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

    public void setOwnership(Ownership ownership)
    {
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

        _ownership = ownership;

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
    }

    void sendData()
    {
        // Send all packet types.
        if (_packetOptions == PacketOptions.All)
        {
            IntPtr dataPtr;

            //if ((transform.position - _oldPosition).sqrMagnitude >= 0.25f || Quaternion.Dot(transform.rotation, _oldRotation) <= 0.95f)
            //{
            //    TransformData transData = new TransformData()
            //    { _EID = _EID, _pos = transform.position, _rot = transform.rotation, _vel = _rigidBody.velocity };
            //    dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());

            //    Marshal.StructureToPtr(transData, dataPtr, false);
            //    //Marshal.PtrToStructure(dataPtr, transData);
            //    _networkManager.sendData(PacketTypes.TransformMsg, dataPtr);

            //    Marshal.FreeHGlobal(dataPtr);
            //    _oldPosition = transform.position;
            //    _oldRotation = transform.rotation;
            //}

            //int state = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            float state = _animator.GetFloat("speed");

            if (!Mathf.Approximately(_prevAnimState, state))
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
                //if ((transform.position - _oldPosition).sqrMagnitude >= 0.25f || Quaternion.Dot(transform.rotation, _oldRotation) <= 0.95f)
                //{
                //    TransformData transData = new TransformData()
                //    { _EID = _EID, _pos = transform.position, _rot = transform.rotation, _vel = _rigidBody.velocity };
                //    IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());

                //    Marshal.StructureToPtr(transData, dataPtr, false);
                //    //Marshal.PtrToStructure(dataPtr, transData);
                //    _networkManager.sendData(PacketTypes.TransformMsg, dataPtr);

                //    Marshal.FreeHGlobal(dataPtr);
                //    _oldPosition = transform.position;
                //    _oldRotation = transform.rotation;
                //}
            }
            // Send animation packets.
            else if ((_packetOptions & PacketOptions.Anim) == PacketOptions.Anim)
            {
                float state = _animator.GetFloat("speed");

                if (!Mathf.Approximately(_prevAnimState, state))
                {
                    //Debug.Log("EID " + _EID + " Anim State: " + state);
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
        // Store current position, rotation and velocity of pawn.
        _prevPos = transform.position;
        _prevRot = transform.rotation;
        _prevVel = _lastKnownVel;


        // Update rotation to last known packet data.
        //transform.rotation = _transData._rot;

        // Last known packet data.
        _lastKnownPos = _transData._pos;
        _lastKnownVel = _transData._vel;
        _lastKnownRot = _transData._rot;
        _lastKnowTime = Time.time;

        // Reset convergence timer.
        _convergeTimer = 0.0f;
    }

    private void Update()
    {
        if (_ownership == Ownership.OtherClientOwned)
        {
            _convergeTimer += Time.deltaTime;
            float interT = _convergeTimer / _timeToConverge;

            if (interT < 1.0f)
            {
                Vector3 pos = Vector3.Lerp(_prevPos, _lastKnownPos, interT);
                Vector3 vel = Vector3.Lerp(_prevVel, _lastKnownVel, interT);
                Quaternion rot = Quaternion.Slerp(_prevRot, _lastKnownRot, interT);


                transform.position = pos + vel * (Time.time - _lastKnowTime);
                transform.rotation = rot;
            }
            else
            {
                transform.position = _lastKnownPos + _lastKnownVel * (Time.time - _lastKnowTime);
                transform.rotation = _lastKnownRot;
            }


            Debug.Log("Dead Reckoning");
        }
        else if (_ownership == Ownership.ClientOwned)
        {
            _playerPrediction = _lastKnownPos + _lastKnownVel * (Time.time - _lastKnowTime);

            if ((transform.position - _playerPrediction).sqrMagnitude >= _threshold * _threshold)
            {
                _thresholdPassed = true;
                _time = 0.0f;

                _lastKnownPos = transform.position;
                //_oldRotation = transform.rotation;
                _lastKnownVel = _rigidBody.velocity;
                //_otherClientFutureTime = Time.time + _networkManager.UpdateInterval;
                _lastKnowTime = Time.time;
                Debug.Log("Threshold breached");
            }

            if (_thresholdPassed)
                _time += Time.deltaTime;

            if (_time >= _networkManager.LagTime && _thresholdPassed)
            {
                TransformData transData = new TransformData()
                { _EID = _EID, _pos = transform.position, _rot = transform.rotation, _vel = _rigidBody.velocity };
                IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());

                Marshal.StructureToPtr(transData, dataPtr, false);
                //Marshal.PtrToStructure(dataPtr, transData);
                _networkManager.sendData(PacketTypes.TransformMsg, dataPtr);

                Marshal.FreeHGlobal(dataPtr);

                _time = 0.0f;
                _thresholdPassed = false;
            }
        }
        // Server Owned
        else
        {
            if ((_packetOptions == PacketOptions.Transform) || (_packetOptions == PacketOptions.All))
            {
                TransformData transData = new TransformData()
                { _EID = _EID, _pos = transform.position, _rot = transform.rotation, _vel = _rigidBody.velocity };
                IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());

                Marshal.StructureToPtr(transData, dataPtr, false);
                _networkManager.sendData(PacketTypes.TransformMsg, dataPtr);

                Marshal.FreeHGlobal(dataPtr);
            }
        }
    }
}
