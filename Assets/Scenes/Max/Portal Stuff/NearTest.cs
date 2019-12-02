using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearTest : MonoBehaviour
{
    public Camera _portalCam;
    public Transform _portalRenderPlane;
    public Vector3 _front;
    public Vector4 _clipPlaneWorldSpace;
    public Vector4 _clipPlaneCameraSpace;
    //public Plane _plane;

    public Vector3 _nearClipOffset;

    public Matrix4x4 _originalProjection;

    //public float left = -0.2F;
    //public float right = 0.2F;
    //public float top = 0.2F;
    //public float bottom = -0.2F;


    // Start is called before the first frame update
    void Start()
    {
        _originalProjection = _portalCam.projectionMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        _portalCam.projectionMatrix = _originalProjection;

        _front = _portalRenderPlane.up;
        _clipPlaneWorldSpace = new Vector4(_front.x, _front.y, _front.z, Vector3.Dot(_portalRenderPlane.position + _nearClipOffset, -_front));
        _clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(_portalCam.worldToCameraMatrix)) * _clipPlaneWorldSpace;

        //_plane = new Plane(_clipPlaneWorldSpace, 0.0f);
        Debug.DrawRay(_portalRenderPlane.position, _front, Color.white);
        //Debug.DrawRay(_portalCam.transform.position - _plane.normal * _plane.GetDistanceToPoint(_portalCam.transform.position), _plane.normal * _plane.GetDistanceToPoint(_portalCam.transform.position), Color.red);

        //Matrix4x4 p = _originalProjection;
        //p.m01 += Mathf.Sin(Time.time * 1.2F) * 0.1F;
        //p.m10 += Mathf.Sin(Time.time * 1.5F) * 0.1F;
        //_portalCam.projectionMatrix = p;

        //Matrix4x4 m = PerspectiveOffCenter(left, right, bottom, top, _portalCam.nearClipPlane, _portalCam.farClipPlane);
        //_portalCam.projectionMatrix = m;

        _portalCam.projectionMatrix = _portalCam.CalculateObliqueMatrix(_clipPlaneCameraSpace);
    }

    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}
