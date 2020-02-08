using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class PlayerNetwork : MonoBehaviour
{
    const string DLL_NAME = "NETWORKINGDLL";

    [DllImport(DLL_NAME)]
    public static extern void sendData(ref Vector3 position, ref Quaternion rotation);

    [DllImport(DLL_NAME)]
    public static extern void receiveData(ref Vector3 position, ref Quaternion rotation);

    [DllImport(DLL_NAME)]
    public static extern bool connectToServer(string id);

    [DllImport(DLL_NAME)]
    public static extern bool initNetwork(ref string ip);


    [SerializeField]
    bool _connected = false;

    [SerializeField]
    bool _connect = false;

    [SerializeField]
    string _ip = "127.0.0.1";

    [SerializeField]
    string _id = "1";

    // Start is called before the first frame update
    void Start()
    {
        bool _initialized = initNetwork(ref _ip);
        _initialized = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Establish connection to server.
        if (_connect)
        {
            if (!_connected)
            {
                _connected = connectToServer(_id);
                Debug.Log(_connected);
            }

            _connect = false;
        }



        // Send this player's transform data to server.
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        sendData(ref position, ref rotation);


        //// Retrieve server interpolated transform data of other player.
        //receiveData(ref position, ref rotation);

        //Debug.Log("Position: " + position);
        //Debug.Log("Rotation: " + rotation);
    }


}
