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
    Image _sisterButtonImage;
    Image _brotherButtonImage;
    Button _sisterButton;
    Button _brotherButton;
    bool _inLobby = true;
    CharacterChoices _charChoice = CharacterChoices.NoChoice;

    public string IPAddr
    {
        get
        {
            return _ipInput.text;
        }
    }

    public bool InLobby
    {
        get
        {
            return _inLobby;
        }
        set
        {
            _inLobby = value;
        }
    }

    public CharacterChoices CharChoice
    {
        get
        {
            return _charChoice;
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

        _sisterButtonImage = canvas.transform.Find("Play as sister").GetComponent<Image>();
        _brotherButtonImage = canvas.transform.Find("Play as brother").GetComponent<Image>();
        _sisterButton = canvas.transform.Find("Play as sister").GetComponent<Button>();
        _brotherButton = canvas.transform.Find("Play as brother").GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _networkManager = GetComponent<NetworkManager>();

        NetworkManager.onServerConnect += onServerConnect;
        NetworkManager.onServerConnectFail += onServerConnectFail;

        ReceiveMsgJob receiveMsgJob = new ReceiveMsgJob();
        _receiveMsgJobHandle = receiveMsgJob.Schedule();
    }

    private void OnApplicationQuit()
    {
        NetworkManager.onServerConnect -= onServerConnect;
        NetworkManager.onServerConnectFail -= onServerConnectFail;

        _stopJob = true;
        _receiveMsgJobHandle.Complete();
    }

    private void OnDestroy()
    {
        NetworkManager.onServerConnect -= onServerConnect;
        NetworkManager.onServerConnectFail -= onServerConnectFail;

        _stopJob = true;
        _receiveMsgJobHandle.Complete();
    }

    // Update is called once per frame
    void Update()
    {
        if (_networkManager.Connected && _inLobby)
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
        _connectText.text = "Connected";
    }

    void onServerConnectFail()
    {
        _connectButton.interactable = true;
        _connectText.text = "Connect";
    }

    public void sendMsg()
    {
        if (!_networkManager.Connected)
        {
            _chatLogText.text += "\nNot connected to the server!";
            return;
        }

        string msg = _chatInput.text;

        if (msg.Length > 256)
        {
            _chatLogText.text += "\nChat exceeded 256 character limit!";
            return;
        }
        else if (msg.Length <= 0)
        {
            _chatLogText.text += "\nChat cannot be empty!";
            return;
        }

        // Send chat message to the server.
        ChatData chatData = new ChatData() { _EID = (byte)_networkManager.ID, _msg = msg, _msgSize = (byte)msg.Length };

        IntPtr dataHandle = Marshal.AllocHGlobal(Marshal.SizeOf(chatData));
        Marshal.StructureToPtr(chatData, dataHandle, false);

        _networkManager.sendData(PacketTypes.LobbyChat, dataHandle);

        Marshal.FreeHGlobal(dataHandle);

        // Add this client's message to the chat log.
        _chatLogText.text += "\nPlayer " + _networkManager.ID + ": " + chatData._msg;

        // Reset chat input text.
        _chatInput.text = "";
    }

    public void sendTeamName()
    {
        if (!_networkManager.Connected)
        {
            _chatLogText.text += "\nNot connected to the server!";
            return;
        }

        string msg = _teamNameInput.text;

        if (msg.Length > 256)
        {
            _chatLogText.text += "\nTeam name exceeded 256 character limit!";
            return;
        }
        else if (msg.Length <= 0)
        {
            _chatLogText.text += "\nTeam name cannot be empty!";
            return;
        }

        // Send team name to the server.
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
        int newTeamNameMsg = 0;
        int newCharChoice = 0;
        int chatDataSize = Marshal.SizeOf<ChatData>();
        int charChoiceDataSize = Marshal.SizeOf<CharChoiceData>();
        ChatData chatData;
        CharChoiceData charChoiceData;
        _networkManager.getNumLobbyPackets(ref numMsgs, ref newTeamNameMsg, ref newCharChoice);

        int totalSize;
        //if (newTeamNameMsg)
        //    totalSize = (chatDataSize * numMsgs) + chatDataSize;
        //else
        //    totalSize = chatDataSize * numMsgs;

        totalSize = (chatDataSize * numMsgs) + (chatDataSize * newTeamNameMsg) + (charChoiceDataSize * newCharChoice);

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
        if (newTeamNameMsg == 1)
        {
            chatData = Marshal.PtrToStructure<ChatData>(dataHandle);
            dataHandle += chatDataSize;

            _teamNameInput.text = chatData._msg;

            // Change colour of the text to indicate the team name is up to date.
            _teamNameImage.color = new Color32(25, 255, 108, 255);
        }

        // Receive character choice.
        if (newCharChoice == 1)
        {
            charChoiceData = Marshal.PtrToStructure<CharChoiceData>(dataHandle);
            dataHandle += chatDataSize;

            CharacterChoices charChoice = charChoiceData._charChoice;

            if (charChoice == CharacterChoices.SisterChoice)
            {
                // Change colour of the button to indicate the current player choice is up to date.
                _sisterButtonImage.color = new Color32(255, 255, 255, 255);
                _sisterButton.interactable = false;
                _brotherButton.interactable = true;
            }
            else if (charChoice == CharacterChoices.BrotherChoice)
            {
                // Change colour of the button to indicate the current player choice is up to date.
                _brotherButtonImage.color = new Color32(255, 255, 255, 255);
                _brotherButton.interactable = false;
                _sisterButton.interactable = true;
            }
            else
            {
                if (_charChoice == CharacterChoices.NoChoice)
                {
                    _sisterButtonImage.color = new Color32(255, 255, 255, 255);
                    _brotherButtonImage.color = new Color32(255, 255, 255, 255);
                    _brotherButton.interactable = true;
                    _sisterButton.interactable = true;
                }
                else if (_charChoice == CharacterChoices.SisterChoice)
                {
                    _brotherButtonImage.color = new Color32(255, 255, 255, 255);
                    _brotherButton.interactable = true;
                }
                else
                {
                    _sisterButtonImage.color = new Color32(255, 255, 255, 255);
                    _sisterButton.interactable = true;
                }
            }
        }

        Marshal.FreeHGlobal(tempDataHandle);
    }

    public void onTeamNameChange()
    {
        // Change colour of the text to indicate the team name is not up to date.
        _teamNameImage.color = new Color32(255, 71, 78, 255);
    }

    public void pickSister()
    {
        if (!_networkManager.Connected)
        {
            _chatLogText.text += "\nCan't pick character, not connected to the server!";
            return;
        }

        // Change colour of the button to indicate the current player's choice is up to date.
        if (_charChoice == CharacterChoices.NoChoice)
        {
            _charChoice = CharacterChoices.SisterChoice;
            _sisterButtonImage.color = new Color32(25, 255, 108, 255);
        }
        else if (_charChoice == CharacterChoices.SisterChoice)
        {
            _charChoice = CharacterChoices.NoChoice;
            _sisterButtonImage.color = new Color32(255, 255, 255, 255);
        }
        else if (_charChoice == CharacterChoices.BrotherChoice)
        {
            _charChoice = CharacterChoices.SisterChoice;
            _sisterButtonImage.color = new Color32(25, 255, 108, 255);
            _brotherButtonImage.color = new Color32(255, 255, 255, 255);
        }

        // Send character choice to the server.
        CharChoiceData charChoiceData = new CharChoiceData() { _EID = (byte)_networkManager.ID, _charChoice = _charChoice };

        IntPtr dataHandle = Marshal.AllocHGlobal(Marshal.SizeOf<CharChoiceData>());
        Marshal.StructureToPtr(charChoiceData, dataHandle, false);

        _networkManager.sendData(PacketTypes.LobbyCharChoice, dataHandle);

        Marshal.FreeHGlobal(dataHandle);


        // Change colour of the button to indicate the current player choice is up to date.
        //_charChoice = CharacterChoices.SisterChoice;
        //_sisterButtonImage.color = new Color32(25, 255, 108, 255);

        //if (_brotherButton.interactable == true)
        //{
        //    _brotherButtonImage.color = new Color32(255, 255, 255, 255);
        //}
    }

    public void pickBrother()
    {
        if (!_networkManager.Connected)
        {
            _chatLogText.text += "\nCan't pick character, not connected to the server!";
            return;
        }

        // Change colour of the button to indicate the current player's choice is up to date.
        if (_charChoice == CharacterChoices.NoChoice)
        {
            _charChoice = CharacterChoices.BrotherChoice;
            _brotherButtonImage.color = new Color32(25, 255, 108, 255);
        }
        else if (_charChoice == CharacterChoices.BrotherChoice)
        {
            _charChoice = CharacterChoices.NoChoice;
            _brotherButtonImage.color = new Color32(255, 255, 255, 255);
        }
        else if (_charChoice == CharacterChoices.SisterChoice)
        {
            _charChoice = CharacterChoices.BrotherChoice;
            _brotherButtonImage.color = new Color32(25, 255, 108, 255);
            _sisterButtonImage.color = new Color32(255, 255, 255, 255);
        }

        // Send character choice to the server.
        CharChoiceData charChoiceData = new CharChoiceData() { _EID = (byte)_networkManager.ID, _charChoice = _charChoice };

        IntPtr dataHandle = Marshal.AllocHGlobal(Marshal.SizeOf<CharChoiceData>());
        Marshal.StructureToPtr(charChoiceData, dataHandle, false);

        _networkManager.sendData(PacketTypes.LobbyCharChoice, dataHandle);

        Marshal.FreeHGlobal(dataHandle);


        // Change colour of the button to indicate the current player choice is up to date.
        //_charChoice = CharacterChoices.BrotherChoice;
        //_brotherButtonImage.color = new Color32(25, 255, 108, 255);

        //if (_sisterButton.interactable == true)
        //{
        //    _sisterButtonImage.color = new Color32(255, 255, 255, 255);
        //}
    }
}
