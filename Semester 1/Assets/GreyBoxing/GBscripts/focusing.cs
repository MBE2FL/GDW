using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class focusing : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Vector3 _targetPos;
    [SerializeField]
    private Vector3 _targetAngle;
    private Vector3 velocity = Vector3.zero;
    private bool _focusAvaliable = false;

    private Transform _camTrans;
    private IEnumerator _focusOnTarget;
    private bool _focusing = false;
    private Vector3 _savedCamPos;
    private Quaternion _savedCamRot;

    private void Awake()
    {
        _camTrans = _camera.transform;

        _focusOnTarget = Focus();
        StartCoroutine(_focusOnTarget);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Sister" || collision.gameObject.tag == "Brother")
            _focusAvaliable = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Sister" || collision.gameObject.tag == "Brother")
            _focusAvaliable = false;
    }

    IEnumerator Focus()
    {
        while (true)
        {
            // Store the unfocused position and rotation of this camera.
            _savedCamPos = _camTrans.position;
            _savedCamRot = _camTrans.rotation;

            // Smoothly move this camera to focus on the target.
            while (_focusAvaliable && Input.GetKey(KeyCode.E))
            {
                _focusing = true;
                _camera.GetComponent<cameraMovement>().sister.GetComponent<Movement>().enabled = false;
                _camera.GetComponent<cameraMovement>().brother.GetComponent<Movement>().enabled = false;
                _camera.GetComponent<cameraMovement>().enabled = false;
                _camera.transform.position = Vector3.SmoothDamp(_camTrans.position, _targetPos, ref velocity, 0.75f);
                _camera.transform.rotation = Quaternion.Slerp(_camTrans.rotation, Quaternion.Euler(_targetAngle), 0.05f);

                yield return null;
            }
            
            // Smoothly move this camera to focusing on the player.
            while (_focusing)
            {
                _camera.transform.position = Vector3.SmoothDamp(_camTrans.position, _savedCamPos, ref velocity, 0.25f);
                _camera.transform.rotation = Quaternion.Slerp(_camTrans.rotation, _savedCamRot, 0.2f);

                if (((_savedCamPos - _camTrans.position).sqrMagnitude < 0.0001f) &&
                    (Quaternion.Dot(_savedCamRot, _camTrans.rotation) >= 1.0f))
                {
                    _camera.transform.position = _savedCamPos;
                    _camera.transform.rotation = _savedCamRot;
                    _focusing = false;
                    _camera.GetComponent<cameraMovement>().enabled = true;
                    _camera.GetComponent<cameraMovement>().sister.GetComponent<Movement>().enabled = true;
                    _camera.GetComponent<cameraMovement>().brother.GetComponent<Movement>().enabled = true;
                }

                yield return null;
            }

            yield return null;
        }
    }
}
