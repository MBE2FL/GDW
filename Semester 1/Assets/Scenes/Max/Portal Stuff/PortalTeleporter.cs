using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    [SerializeField]
    private Transform _player;
    [SerializeField]
    private Transform _receiver;
    private bool _playerInPortal = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<Renderer>().material.SetFloat("_totalTime", Time.time);

        if (_playerInPortal)
        {
            //Vector3 toPlayer = (_player.position - transform.position).normalized;
            // Don't need to normalize, since only care if dot product is less than 0. (i.e. Don't care about clamping it to [-1, 1])
            Vector3 toPlayer = _player.position - transform.position;
            float PortaldotPlayer = Vector3.Dot(transform.up, toPlayer);


            // Player is behind the portal
            //if (PortaldotPlayer < 0.0f)
            //{
            //    float rotDiff = -Quaternion.Angle(transform.rotation, _receiver.rotation);
            //    rotDiff += 180.0f;
            //    _player.Rotate(Vector3.up, rotDiff); // TO-DO Also update to 3 axis.

            //    Vector3 posOffset = Quaternion.Euler(0.0f, rotDiff, 0.0f) * toPlayer;
            //    _player.position = _receiver.position + posOffset;

            //    _playerInPortal = false;

            //    Debug.Log("Teleported");
            //}


            // Player is behind the portal
            if (PortaldotPlayer < 0.0f)
            {
                float rotDiff = -Quaternion.Angle(transform.rotation, _receiver.rotation);
                rotDiff += 180.0f;
                _player.Rotate(Vector3.up, rotDiff); // TO-DO Also update to 3 axis.

                Vector3 posOffset = Quaternion.Euler(0.0f, rotDiff, 0.0f) * toPlayer;
                _player.position = _receiver.position + posOffset;

                _playerInPortal = false;

                Debug.Log("Teleported");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Sister")
        {
            _playerInPortal = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Sister")
        {
            _playerInPortal = false;
        }
    }
}
