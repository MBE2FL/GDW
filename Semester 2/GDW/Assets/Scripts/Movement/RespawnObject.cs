using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnObject : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 respawnPos;
    private Transform _transform;
    void Start()
    {
        _transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_transform.position.y < -4.0f)
        {
            if (this._transform.transform.parent)
            {
                _transform.transform.SetParent(null);
            }
            _transform.position = respawnPos;

        }
    }
}
