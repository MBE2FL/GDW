using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public struct PlayTime
    {
        public int min;
        public float sec;
    }
    public struct PlayerTime
    {
        public string name;
        public PlayTime playTime;

    }
    public List<PlayerTime> playerTimes;

    private void sort()
    {
        playerTimes.Sort((t1, t2) => (t1.playTime.min * 60 + t1.playTime.sec).CompareTo(t2.playTime.min * 60 + t2.playTime.sec));
    }

    void Start()
    {

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
                time1.text = playerTimes[0].playTime.min.ToString() + ":" + playerTimes[0].playTime.sec.ToString();
            }
            if (playerTimes.Count > 1)
            {
                line2.text = "2. " + playerTimes[1].name;
                time2.text = playerTimes[1].playTime.min.ToString() + ":" + playerTimes[1].playTime.sec.ToString();
            }
            if (playerTimes.Count > 2)
            {
                line3.text = "3. " + playerTimes[2].name;
                time3.text = playerTimes[2].playTime.min.ToString() + ":" + playerTimes[2].playTime.sec.ToString();
            }
            if (playerTimes.Count > 3)
            {
                line4.text = "4. " + playerTimes[3].name;
                time4.text = playerTimes[3].playTime.min.ToString() + ":" + playerTimes[3].playTime.sec.ToString();
            }
            if (playerTimes.Count > 4)
            {
                line5.text = "5. " + playerTimes[4].name;
                time5.text = playerTimes[4].playTime.min.ToString() + ":" + playerTimes[4].playTime.sec.ToString();
            }
        }
    }
}
