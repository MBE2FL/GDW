using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UI;

struct ReceiveMsgJob : IJob
{
    public void Execute()
    {
        while (!Lobby._stopJob && !NetworkManager.stopJobs)
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
    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string _msg;
    //public IntPtr _msg;
}

public class Lobby : MonoBehaviour
{
    NetworkManager _networkManager;
    JobHandle _receiveMsgJobHandle;
    public static bool _stopJob = false;
    string _msg = "";
    InputField _chatInput;
    InputField _ipInput;
    Button _connectButton;
    Text _connectText;
    Text _chatLogText;
    InputField _teamNameInput;
    Image _teamNameImage;

    public string IPAddr
    {
        get
        {
            return _ipInput.text;
        }
    }


    private void Awake()
    {
        GameObject canvas = GameObject.Find("UI");
        _chatInput = canvas.transform.Find("Chat Input").GetComponent<InputField>();
        _ipInput = canvas.transform.Find("IP Input").GetComponent<InputField>();

        Transform connectObj = canvas.transform.Find("Connect");
        _connectButton = connectObj.GetComponent<Button>();
        _connectText = connectObj.GetComponentInChildren<Text>();

        _chatLogText = canvas.transform.Find("Chat log display").GetComponent<Text>();

        _teamNameInput = canvas.transform.Find("Team name").GetComponent<InputField>();
        _teamNameImage = canvas.transform.Find("Team name").GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _networkManager = GetComponent<NetworkManager>();

        NetworkManager.onServerConnect += onServerConnect;

        ReceiveMsgJob receiveMsgJob = new ReceiveMsgJob();
        _receiveMsgJobHandle = receiveMsgJob.Schedule();
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
        if (_networkManager.Connected)
            receiveMsgs();
    }

    public void connect()
    {
        _networkManager.connect(_ipInput.text);

        _connectButton.interactable = false;
        _connectText.text = "Connecting...";
    }

    void onServerConnect()
    {
        _connectButton.gameObject.SetActive(false);
    }

    public void sendMsg()
    {
        string msg = _chatInput.text;

        if (msg.Length > 256)
        {
            Debug.LogWarning("Chat exceeded 256 character limit!");
            return;
        }
        else if (msg.Length <= 0)
        {
            Debug.LogWarning("Chat cannot be empty!");
            return;
        }

        ChatData chatData = new ChatData() { _EID = (byte)_networkManager.ID, _msg = msg, _msgSize = (byte)msg.Length };

        IntPtr dataHandle = Marshal.AllocHGlobal(Marshal.SizeOf(chatData));
        Marshal.StructureToPtr(chatData, dataHandle, false);

        _networkManager.sendData(PacketTypes.LobbyChat, dataHandle);

        Marshal.FreeHGlobal(dataHandle);

        _chatLogText.text += "\nPlayer " + _networkManager.ID + ": " + chatData._msg;
    }

    public void sendTeamName()
    {
        string msg = _teamNameInput.text;

        if (msg.Length > 256)
        {
            Debug.LogWarning("Team name exceeded 256 character limit!");
            return;
        }
        else if (msg.Length <= 0)
        {
            Debug.LogWarning("Team name cannot be empty!");
            return;
        }

        ChatData chatData = new ChatData() { _EID = (byte)_networkManager.ID, _msg = msg, _msgSize = (byte)msg.Length };

        IntPtr dataHandle = Marshal.AllocHGlobal(Marshal.SizeOf(chatData));
        Marshal.StructureToPtr(chatData, dataHandle, false);

        _networkManager.sendData(PacketTypes.LobbyTeamName, dataHandle);

        Marshal.FreeHGlobal(dataHandle);


        // Change colour of the text to indicate the team name is up to date.
        _teamNameImage.color = new Color32(25, 255, 108, 255);
    }

    void receiveMsgs()
    {
        int numMsgs = 0;
        bool newTeamNameMsg = false;
        int chatDataSize = Marshal.SizeOf<ChatData>();
        ChatData chatData;
        _networkManager.getNumLobbyPackets(ref numMsgs, ref newTeamNameMsg);

        int totalSize;
        if (newTeamNameMsg)
            totalSize = (chatDataSize * numMsgs) + chatDataSize;
        else
            totalSize = chatDataSize * numMsgs;

        IntPtr dataHandle = Marshal.AllocHGlobal(totalSize);
        IntPtr tempDataHandle = dataHandle;

        _networkManager.getLobbyPacketHandles(dataHandle);

        // Receive chat messages.
        for (int i = 0; i < numMsgs; ++i)
        {
            chatData = Marshal.PtrToStructure<ChatData>(dataHandle);
            dataHandle += chatDataSize;

            _chatLogText.text += "\nPlayer " + chatData._EID + ": " + chatData._msg;
        }

        // Receive team name message.
        if (newTeamNameMsg)
        {
            chatData = Marshal.PtrToStructure<ChatData>(dataHandle);
            dataHandle += chatDataSize;

            _teamNameInput.text = chatData._msg;

            // Change colour of the text to indicate the team name is up to date.
            _teamNameImage.color = new Color32(25, 255, 108, 255);
        }

        Marshal.FreeHGlobal(tempDataHandle);
    }

    public void onTeamNameChange()
    {
        // Change colour of the text to indicate the team name is not up to date.
        _teamNameImage.color = new Color32(255, 71, 78, 255);
    }
}
