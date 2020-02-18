using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

enum NetworkOp
{
    Receiever,
    Transmitter
}

public class NetworkObject : MonoBehaviour
{
    [SerializeField]
    NetworkOp _networkOp;

    NetworkManager _networkManager;

    //const string DLL_NAME = "NETWORKINGDLL";

    //[DllImport(DLL_NAME)]
    //public static extern void sendData(ref Vector3 position, ref Quaternion rotation);

    //[DllImport(DLL_NAME)]
    //public static extern void receiveData(ref Vector3 position, ref Quaternion rotation);

    // Start is called before the first frame update
    void Start()
    {
        _networkManager = GameObject.Find("Game Manager").GetComponent<NetworkManager>();

        _networkManager.onServerConnect += onServerConnect;

        switch (_networkOp)
        {
            case NetworkOp.Receiever:
                _networkManager.onDataReceive += receiveData;
                break;
            case NetworkOp.Transmitter:
                _networkManager.onDataSend += sendData;
                break;
            default:
                break;
        }
    }

    private void OnApplicationQuit()
    {
        _networkManager.onServerConnect -= onServerConnect;

        switch (_networkOp)
        {
            case NetworkOp.Receiever:
                _networkManager.onDataReceive -= receiveData;
                break;
            case NetworkOp.Transmitter:
                _networkManager.onDataSend -= sendData;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    //void Update()
    //{

    //}

    void onServerConnect()
    {
        Debug.Log("Network Object Ready: " + _networkOp);
    }

    void sendData()
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        _networkManager.sendData(ref position, ref rotation);
    }

    void receiveData()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        _networkManager.receiveData(ref position, ref rotation);

        transform.position = position;
        transform.rotation = rotation;
    }

}
