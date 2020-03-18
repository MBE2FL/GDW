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
public enum PacketTypes
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
    byte _objID = 0;
    [SerializeField]
    PrefabTypes _prefabType = PrefabTypes.Sister;
    [SerializeField]
    Ownership _ownership = Ownership.ClientOwned;

    [SerializeField]
    PacketTypes _packetTypes;
    

    public byte ObjID
    {
        get
        {
            return _objID;
        }
        set
        {
            _objID = value;
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
                _networkManager.onDataReceive += sendData;
                break;
            default:
                _networkManager.onDataSend += sendData;
                _networkManager.onDataReceive += receiveData;
                break;
        }
        
        _networkManager.NetworkObjects.Add(this);
    }

    private void OnApplicationQuit()
    {
        //_networkManager.onServerConnect -= onServerConnect;
        NetworkManager.onServerConnect -= onServerConnect;

        switch (_ownership)
        {
            case Ownership.ClientOwned:
                _networkManager.onDataReceive += sendData;
                break;
            default:
                _networkManager.onDataSend += sendData;
                _networkManager.onDataReceive += receiveData;
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
        // WARNING WOULD ONLY WORK WITH BLITTABLE TYPES I BELIEVE!

        //Vector3 position = transform.position;
        //Quaternion rotation = transform.rotation;

        //float[] data = new float[7];

        //data[0] = position.x;
        //data[1] = position.y;
        //data[2] = position.z;
        //data[3] = rotation.x;
        //data[4] = rotation.y;
        //data[5] = rotation.z;
        //data[6] = rotation.w;


        //unsafe
        //{
        //    fixed(float* ptrData = data)
        //    {
        //        IntPtr dataPtr = new IntPtr(ptrData);
        //        _networkManager.sendData((int)MessageTypes.TransformMsg, 0, dataPtr);
        //    }
        //}


        if ((_packetTypes & PacketTypes.Transform) == PacketTypes.Transform)
        {
            TransformData transData = new TransformData() { objID = _objID, pos = transform.position, rot = transform.rotation };
            IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf<TransformData>());
            Marshal.StructureToPtr(transData, dataPtr, false);
            //Marshal.PtrToStructure(dataPtr, transData);
            _networkManager.sendData((int)MessageTypes.TransformMsg, _objID, dataPtr);
            Marshal.FreeHGlobal(dataPtr);
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
