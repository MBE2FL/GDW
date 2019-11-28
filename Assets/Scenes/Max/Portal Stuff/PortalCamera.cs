using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    private Transform _playerCamera;
    public Transform _portal;
    public Transform _otherPortal;
    private Matrix4x4 _originalPerspective;
    private Camera _portalCam;

    // Start is called before the first frame update
    void Start()
    {
        _playerCamera = Camera.main.GetComponent<Transform>();
        _portalCam = GetComponent<Camera>();
        _originalPerspective = _portalCam.projectionMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = _playerCamera.position - _otherPortal.position;
        transform.position = _portal.position + offset;

        //transform.rotation = _playerCamera.rotation;

        // TO-DO Modify to work on all 3 axis. Only works for portals rotated along the y-axis.
        float angularDiffBetweenPortalRots = Quaternion.Angle(_portal.rotation, _otherPortal.rotation);

        // Found bug. Had to negate the angular difference. (FIXED)
        // Found another bug. Orientation is correct, but screen cutout is not lined up with the portal.
        // Found another bug. When a portal is rotated, not seeing exactly what you should. Other camera should be fixed in front of portal.
        Quaternion portalRotDiff = Quaternion.AngleAxis(angularDiffBetweenPortalRots, Vector3.up);
        Vector3 newCamDir = portalRotDiff * _playerCamera.forward;
        transform.rotation = Quaternion.LookRotation(newCamDir, Vector3.up);



        Matrix4x4 p = _originalPerspective;
        p.m01 += Mathf.Sin(Time.time * 1.2F) * 0.1F;
        p.m10 += Mathf.Sin(Time.time * 1.5F) * 0.1F;
        _portalCam.projectionMatrix = p;
    }
}
