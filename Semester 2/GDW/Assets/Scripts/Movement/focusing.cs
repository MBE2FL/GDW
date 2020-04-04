using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class focusing : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private GameObject _character;
    private Vector3 velocity = Vector3.zero;

    private Transform _camTrans;
    private IEnumerator _focusOnTarget;
    cameraMovement _camMove;
    Movement _movement;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camTrans = _camera.transform;
        _camMove = _camera.GetComponent<cameraMovement>();

        _focusOnTarget = Focus();
        StartCoroutine(_focusOnTarget);
    }

    IEnumerator Focus()
    {
        while (true)
        {
            if (!_camMove.Player)
                yield return null;

            if (!_movement)
                _movement = _camMove.Player.GetComponent<Movement>();

            // Smoothly move this camera to focus on the target.
            while (_camera.transform.position != _character.transform.position &&
                _camera.transform.rotation != _character.transform.rotation)
            {
                _movement.enabled = false;
                _camMove.enabled = false;
                _camera.transform.position = Vector3.SmoothDamp(_camTrans.position, _character.transform.position + new Vector3(0,2,-4), ref velocity, 3.0f);
                _camera.transform.rotation = Quaternion.Slerp(_camTrans.rotation, _character.transform.rotation, 0.015f);

                yield return null;
            }

            _camMove.enabled = true;
            _movement.enabled = true;

            StopCoroutine(_focusOnTarget);
            yield return null;
        }
    }
}
