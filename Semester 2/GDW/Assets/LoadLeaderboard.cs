using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLeaderboard : MonoBehaviour
{
    NetworkManager _networkManager;


    private void Awake()
    {
        _networkManager = FindObjectOfType<GameManager>().GetComponent<NetworkManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Sister" || other.transform.tag == "Brother")
        {
            _networkManager.NetworkObjects.Clear();

            SceneManager.LoadScene("Leaderboard");
        }
    }
}
