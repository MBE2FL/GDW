using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;
using UnityEngine.SceneManagement;



//[StructLayout(LayoutKind.Sequential)]
//[Serializable]
//public struct PlayTime
//{
//    public int min;
//    public float sec;
//}

//[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
//[Serializable]
//public struct PlayerTime
//{
//    public string name;
//    public PlayTime playTime;
//}


public class Leaderboard : MonoBehaviour
{
    public Text line1;
    public Text line2;
    public Text line3;
    public Text line4;
    public Text line5;

    public Text time1;
    public Text time2;
    public Text time3;
    public Text time4;
    public Text time5;


    public List<ScoreData> playerTimes;

    NetworkManager _networkManager;
    Lobby _lobby;

    private void sort()
    {
        playerTimes.Sort((t1, t2) => (t1.min * 60 + t1.sec).CompareTo(t2.min * 60 + t2.sec));
    }

    void Start()
    {
        _networkManager = GetComponent<NetworkManager>();
        NetworkManager.onServerConnect += onServerConnect;

        SceneManager.sceneLoaded += onSceneLoaded;

        _lobby = GetComponent<Lobby>();
    }

    private void OnApplicationQuit()
    {
        NetworkManager.onServerConnect -= onServerConnect;

        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    private void OnDestroy()
    {
        NetworkManager.onServerConnect -= onServerConnect;

        SceneManager.sceneLoaded -= onSceneLoaded;
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if ((scene.name == "Leaderboard") && (_lobby.CharChoice == CharacterChoices.SisterChoice))
        {
            SceneManager.SetActiveScene(scene);

            sendScore();

            SceneManager.sceneLoaded -= onSceneLoaded;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (line1)
        {
            sort();
            if (playerTimes.Count > 0)
            {
                line1.text = "1. " + playerTimes[0].name;
                time1.text = playerTimes[0].min.ToString() + ":" + playerTimes[0].sec.ToString();
            }
            if (playerTimes.Count > 1)
            {
                line2.text = "2. " + playerTimes[1].name;
                time2.text = playerTimes[1].min.ToString() + ":" + playerTimes[1].sec.ToString();
            }
            if (playerTimes.Count > 2)
            {
                line3.text = "3. " + playerTimes[2].name;
                time3.text = playerTimes[2].min.ToString() + ":" + playerTimes[2].sec.ToString();
            }
            if (playerTimes.Count > 3)
            {
                line4.text = "4. " + playerTimes[3].name;
                time4.text = playerTimes[3].min.ToString() + ":" + playerTimes[3].sec.ToString();
            }
            if (playerTimes.Count > 4)
            {
                line5.text = "5. " + playerTimes[4].name;
                time5.text = playerTimes[4].min.ToString() + ":" + playerTimes[4].sec.ToString();
            }
        }
    }


    public void onServerConnect()
    {
        // Get all the scores currently stored on the server.
        int numScores = 0;
        _networkManager.getNumScores(ref numScores);

        ScoreData scoreData;
        IntPtr scoreHandle;
        //IntPtr tempScoreHandle;
        int scoreDataSize = Marshal.SizeOf<ScoreData>();

        //scoreHandle = Marshal.AllocHGlobal(scoreDataSize * numScores);
        //tempScoreHandle = scoreHandle;

        scoreHandle = _networkManager.getScoresHandle();

        for (int i = 0; i < numScores; ++i)
        {
            scoreData = Marshal.PtrToStructure<ScoreData>(scoreHandle);
            scoreHandle += scoreDataSize;

            playerTimes.Add(scoreData);
        }

        _networkManager.cleanupScoresHandle();
    }

    void sendScore()
    {
        // Record the new score.
        PlayTime playTime = new PlayTime() { min = (int)(Time.time / 60.0f), sec = Time.time % 60 };
        PlayerTime playerTime = new PlayerTime() { name = _networkManager.TeamName, playTime = playTime };

        // Send the new score to the server.
        ScoreData scoreData = new ScoreData() { _nameSize = (byte)playerTime.name.Length, _time = playerTime };
        IntPtr dataHandle = Marshal.AllocHGlobal(Marshal.SizeOf<ScoreData>());
        Marshal.StructureToPtr(scoreData, dataHandle, false);
        _networkManager.sendData(PacketTypes.Score, dataHandle);

        // Add and sort the new score into our current leaderboard.
        playerTimes.Add(playerTime);
        sort();
    }
}
