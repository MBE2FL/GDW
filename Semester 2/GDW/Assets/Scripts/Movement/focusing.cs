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

    private void Awake()
    {
        _camTrans = _camera.transform;

        _focusOnTarget = Focus();
        StartCoroutine(_focusOnTarget);
    }

    IEnumerator Focus()
    {
        while (true)
        {
            // Smoothly move this camera to focus on the target.
            while (_camera.transform.position != _character.transform.position &&
                _camera.transform.rotation != _character.transform.rotation)
            {
                _camera.GetComponent<cameraMovement>().sister.GetComponent<Movement>().enabled = false;
                _camera.GetComponent<cameraMovement>().brother.GetComponent<Movement>().enabled = false;
                _camera.GetComponent<cameraMovement>().enabled = false;
                _camera.transform.position = Vector3.SmoothDamp(_camTrans.position, _character.transform.position + new Vector3(0,2,-4), ref velocity, 3.0f);
                _camera.transform.rotation = Quaternion.Slerp(_camTrans.rotation, _character.transform.rotation, 0.015f);

                yield return null;
            }

            _camera.GetComponent<cameraMovement>().enabled = true;
            _camera.GetComponent<cameraMovement>().sister.GetComponent<Movement>().enabled = true;
            _camera.GetComponent<cameraMovement>().brother.GetComponent<Movement>().enabled = true;

            StopCoroutine(_focusOnTarget);
            yield return null;
        }
    }
}
