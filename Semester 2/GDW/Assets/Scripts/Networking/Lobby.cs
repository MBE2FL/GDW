using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System.Runtime.InteropServices;
using System;

struct ReceiveMsgJob : IJob
{
    public void Execute()
    {
        while (Lobby._stopJob || NetworkManager.stopJobs)
        {
            NetworkManager.receiveLobbyData();
        }

        Debug.Log("ReceiveMsgJob Stopped");
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ChatData
{
    public byte _EID;
    public byte _msgSize;
    public string _msg;
}

public class Lobby : MonoBehaviour
{
    NetworkManager _networkManager;
    JobHandle _receiveMsgJobHandle;
    public static bool _stopJob = false;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<NetworkManager>();

        NetworkManager.onServerConnect += onServerConnect;
    }

    private void OnApplicationQuit()
    {
        NetworkManager.onServerConnect -= onServerConnect;

        _stopJob = true;
        _receiveMsgJobHandle.Complete();
    }

    private void OnDestroy()
    {
        NetworkManager.onServerConnect -= onServerConnect;

        _stopJob = true;
        _receiveMsgJobHandle.Complete();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onServerConnect()
    {
        ReceiveMsgJob receiveMsgJob = new ReceiveMsgJob();
        _receiveMsgJobHandle = receiveMsgJob.Schedule();
    }

    public void sendMsg(string msg)
    {

    }

    void receiveMsgs()
    {
        int numMsgs = 0;
        int numChars = 0;
        int chatDataSize = Marshal.SizeOf<ChatData>();
        ChatData chatData;
        _networkManager.getNumLobbyPackets(ref numMsgs, ref numChars);

        IntPtr dataHandle = Marshal.AllocHGlobal(chatDataSize * numMsgs);
        IntPtr tempDataHandle = dataHandle;


        
        for (int i = 0; i < numMsgs; ++i)
        {
            chatData = Marshal.PtrToStructure<ChatData>(dataHandle);
            dataHandle += chatDataSize;
        }

        Marshal.FreeHGlobal(tempDataHandle);
    }
}
