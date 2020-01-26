using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPortal : MonoBehaviour
{
    private Transform _playerCamera;
    public Camera portalCamera;
    public Transform pairPortal;

    // Start is called before the first frame update
    void Start()
    {
        _playerCamera = Camera.main.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        var relativePosition = transform.InverseTransformPoint(_playerCamera.transform.position);
        //relativePosition = Vector3.Scale(relativePosition, new Vector3(-1, 1, -1));
        portalCamera.transform.position = pairPortal.TransformPoint(relativePosition);

        var relativeRotation = transform.InverseTransformDirection(_playerCamera.transform.forward);
        //relativeRotation = Vector3.Scale(relativeRotation, new Vector3(-1, 1, -1));
        portalCamera.transform.forward = pairPortal.TransformDirection(relativeRotation);
    }
}
