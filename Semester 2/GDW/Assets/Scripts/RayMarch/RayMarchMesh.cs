using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;



public class RayMarchMesh : MonoBehaviour
{
    public Material m_material;

    List<GameObject> meshes = new List<GameObject>();

    public MARCHING_MODE mode = MARCHING_MODE.CUBES;

    public ComputeShader _sdfCompute;

    public bool meshify = false;

    [SerializeField]
    RMMarchingCubeShader _rmShader;

    [SerializeField]
    Vector3Int _numThreads = new Vector3Int(32, 1, 1);
    [SerializeField]
    Vector3Int _resolution;
    [SerializeField]
    BoundsInt _volumeBounds;

    void Start()
    {
        //// Get all ray march objects.
        //RMObj[] rmObjs = (RMObj[])FindObjectsOfType(typeof(RMObj));
        //GameObject[] objs = new GameObject[rmObjs.Length];

        //for (int i = 0; i < rmObjs.Length; ++i)
        //{
        //    objs[i] = rmObjs[i].gameObject;
        //}


        //int width = 32;
        //int height = 32;
        //int length = 32;

        //float[] voxels = new float[width * height * length];

        //// Extract all neccessary info, and pack into float array.
        //Matrix4x4[] invModelMats = new Matrix4x4[objs.Length];
        //int[] primitiveTypes = new int[objs.Length];
        //Vector4[] combineOps = new Vector4[objs.Length];
        //Vector4[] primitiveGeoInfo = new Vector4[objs.Length];


        //GameObject obj;
        //for (int i = 0; i < objs.Length; ++i)
        //{
        //    obj = objs[i].gameObject;


        //    invModelMats[i] = obj.transform.localToWorldMatrix.inverse;
        //    primitiveTypes[i] = (int)obj.GetComponent<RMPrimitive>().PrimitiveType;
        //    combineOps[i] = obj.GetComponent<RMPrimitive>().CombineOp;
        //    primitiveGeoInfo[i] = obj.GetComponent<RMPrimitive>().GeoInfo;


        //}


        //int kernel = _sdfCompute.FindKernel("CSMain");

        //// Create a compute buffer.
        //ComputeBuffer buffer = new ComputeBuffer(voxels.Length, sizeof(float));
        //buffer.SetData(voxels);
        //_sdfCompute.SetBuffer(kernel, "_voxels", buffer);
        ////_collisionCompute.SetTexture(0, "Result", tex);

        //_sdfCompute.SetMatrixArray("_invModelMats", invModelMats);
        //_sdfCompute.SetInts("_primitiveTypes", primitiveTypes);
        //_sdfCompute.SetVectorArray("_combineOps", combineOps);
        //_sdfCompute.SetVectorArray("_primitiveGeoInfo", primitiveGeoInfo);

        ////int numThreadGroups = objs.Length;
        //_sdfCompute.Dispatch(kernel, 1, 1, 1);



        ////Set the mode used to create the mesh.
        ////Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
        //Marching marching = null;
        //if (mode == MARCHING_MODE.TETRAHEDRON)
        //    marching = new MarchingTertrahedron();
        //else
        //    marching = new MarchingCubes();

        ////Surface is the value that represents the surface of mesh
        ////For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
        ////The target value does not have to be the mid point it can be any value with in the range.
        //marching.Surface = 0.0f;

        //////The size of voxel array.
        ////int width = 32;
        ////int height = 32;
        ////int length = 32;

        ////float[] voxels = new float[width * height * length];

        ////Fill voxels with values. Im using perlin noise but any method to create voxels will work.
        ////for (int x = 0; x < width; x++)
        ////{
        ////    for (int y = 0; y < height; y++)
        ////    {
        ////        for (int z = 0; z < length; z++)
        ////        {
        ////            float fx = x / (width - 1.0f);
        ////            float fy = y / (height - 1.0f);
        ////            float fz = z / (length - 1.0f);

        ////            int idx = x + y * width + z * width * height;

        ////            voxels[idx] = fractal.Sample3D(fx, fy, fz);


        ////        }
        ////    }
        ////}
        //buffer.GetData(voxels);

        //List<Vector3> verts = new List<Vector3>();
        //List<int> indices = new List<int>();

        ////The mesh produced is not optimal. There is one vert for each index.
        ////Would need to weld vertices for better quality mesh.
        //marching.Generate(voxels, width, height, length, verts, indices);

        ////A mesh in unity can only be made up of 65000 verts.
        ////Need to split the verts between multiple meshes.

        //int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        //int numMeshes = verts.Count / maxVertsPerMesh + 1;

        //for (int i = 0; i < numMeshes; i++)
        //{

        //    List<Vector3> splitVerts = new List<Vector3>();
        //    List<int> splitIndices = new List<int>();

        //    for (int j = 0; j < maxVertsPerMesh; j++)
        //    {
        //        int idx = i * maxVertsPerMesh + j;

        //        if (idx < verts.Count)
        //        {
        //            splitVerts.Add(verts[idx]);
        //            splitIndices.Add(j);
        //        }
        //    }

        //    if (splitVerts.Count == 0) continue;

        //    Mesh mesh = new Mesh();
        //    mesh.SetVertices(splitVerts);
        //    mesh.SetTriangles(splitIndices, 0);
        //    mesh.RecalculateBounds();
        //    mesh.RecalculateNormals();

        //    GameObject go = new GameObject("Mesh");
        //    go.transform.parent = transform;
        //    go.AddComponent<MeshFilter>();
        //    go.AddComponent<MeshRenderer>();
        //    go.GetComponent<Renderer>().material = m_material;
        //    go.GetComponent<MeshFilter>().mesh = mesh;
        //    //go.transform.localPosition = new Vector3(-width / 2, -height / 2, -length / 2);
        //    //go.transform.localPosition = new Vector3(0.0f, 4.0f, 0.0f);

        //    meshes.Add(go);
        //}


        //buffer.Release();
    }

        // Update is called once per frame
    void Update()
    {
        if (meshify)
        {
            meshifySingleCube();

            meshify = false;
        }
    }

    void meshifyMultipleCubes()
    {
        foreach (GameObject gameObject in meshes)
        {
            Destroy(gameObject);
        }

        // Get all ray march objects.
        RMObj[] rmObjs = (RMObj[])FindObjectsOfType(typeof(RMObj));
        GameObject[] objs = new GameObject[rmObjs.Length];

        // Extract all neccessary info, and pack into float array.
        Matrix4x4[] invModelMats = new Matrix4x4[objs.Length];
        int[] primitiveTypes = new int[objs.Length];
        Vector4[] combineOps = new Vector4[objs.Length];
        Vector4[] primitiveGeoInfo = new Vector4[objs.Length];

        for (int i = 0; i < rmObjs.Length; ++i)
        {
            objs[i] = rmObjs[i].gameObject;
        }

        GameObject obj;
        for (int i = 0; i < objs.Length; ++i)
        {
            obj = objs[i].gameObject;


            invModelMats[i] = obj.transform.localToWorldMatrix.inverse;
            primitiveTypes[i] = (int)obj.GetComponent<RMPrimitive>().PrimitiveType;
            combineOps[i] = obj.GetComponent<RMPrimitive>().CombineOp;
            primitiveGeoInfo[i] = obj.GetComponent<RMPrimitive>().GeoInfo;
        }




        for (int x = 0; x < _volumeBounds.x; ++x)
        {
            for (int y = 0; y < _volumeBounds.y; ++y)
            {
                for (int z = 0; z < _volumeBounds.z; ++z)
                {
                    int width = _resolution.x;
                    int height = _resolution.y;
                    int length = _resolution.z;

                    float[] voxels = new float[width * height * length];


                    int kernel = _sdfCompute.FindKernel("CSMain");

                    // Create a compute buffer.
                    ComputeBuffer buffer = new ComputeBuffer(voxels.Length, sizeof(float));
                    buffer.SetData(voxels);
                    _sdfCompute.SetBuffer(kernel, "_voxels", buffer);
                    //_collisionCompute.SetTexture(0, "Result", tex);

                    _sdfCompute.SetMatrixArray("_invModelMats", invModelMats);
                    _sdfCompute.SetInts("_primitiveTypes", primitiveTypes);
                    _sdfCompute.SetVectorArray("_combineOps", combineOps);
                    _sdfCompute.SetVectorArray("_primitiveGeoInfo", primitiveGeoInfo);
                    _sdfCompute.SetVector("_volumeArea", new Vector3(x, y, z));

                    //int numThreadGroups = objs.Length;
                    _sdfCompute.Dispatch(kernel, 1, 1, 1);



                    //Set the mode used to create the mesh.
                    //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
                    Marching marching = null;
                    if (mode == MARCHING_MODE.TETRAHEDRON)
                        marching = new MarchingTertrahedron();
                    else
                        marching = new MarchingCubes();

                    //Surface is the value that represents the surface of mesh
                    //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
                    //The target value does not have to be the mid point it can be any value with in the range.
                    marching.Surface = 0.0f;


                    buffer.GetData(voxels);

                    List<Vector3> verts = new List<Vector3>();
                    List<int> indices = new List<int>();

                    //The mesh produced is not optimal. There is one vert for each index.
                    //Would need to weld vertices for better quality mesh.
                    marching.Generate(voxels, width, height, length, verts, indices);

                    //A mesh in unity can only be made up of 65000 verts.
                    //Need to split the verts between multiple meshes.

                    int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
                    int numMeshes = verts.Count / maxVertsPerMesh + 1;

                    for (int i = 0; i < numMeshes; i++)
                    {

                        List<Vector3> splitVerts = new List<Vector3>();
                        List<int> splitIndices = new List<int>();

                        for (int j = 0; j < maxVertsPerMesh; j++)
                        {
                            int idx = i * maxVertsPerMesh + j;

                            if (idx < verts.Count)
                            {
                                splitVerts.Add(verts[idx]);
                                splitIndices.Add(j);
                            }
                        }

                        if (splitVerts.Count == 0) continue;

                        Mesh mesh = new Mesh();
                        mesh.SetVertices(splitVerts);
                        mesh.SetTriangles(splitIndices, 0);
                        mesh.RecalculateBounds();
                        mesh.RecalculateNormals();

                        GameObject go = new GameObject("Mesh");
                        go.transform.parent = transform;
                        go.AddComponent<MeshFilter>();
                        go.AddComponent<MeshRenderer>();
                        go.GetComponent<Renderer>().material = m_material;
                        go.GetComponent<MeshFilter>().mesh = mesh;
                        //go.transform.localPosition = new Vector3(-width / 2, -height / 2, -length / 2);
                        //go.transform.localPosition = new Vector3(0.0f, 4.0f, 0.0f);
                        go.transform.localPosition = new Vector3(x * 1.0f, y + 10.0f, z * 1.0f);
                        go.transform.localScale = new Vector3(.0323f, .0323f, .0323f);

                        meshes.Add(go);
                    }

                    buffer.Release();
                }
            }
        }
    }

    void meshifySingleCube()
    {
        foreach (GameObject gameObject in meshes)
        {
            Destroy(gameObject);
        }



        if (!_rmShader)
            return;

        RMObj[] rmObjs = _rmShader.RenderList.ToArray();
        // Get all ray march objects.
        //RMObj[] rmObjs = (RMObj[])FindObjectsOfType(typeof(RMObj));
        GameObject[] objs = new GameObject[rmObjs.Length];

        // Extract all neccessary info, and pack into float array.
        Matrix4x4[] invModelMats = new Matrix4x4[objs.Length];
        int[] primitiveTypes = new int[objs.Length];
        Vector4[] combineOps = new Vector4[objs.Length];
        Vector4[] primitiveGeoInfo = new Vector4[objs.Length];
        Vector4[] boundGeoInfo = new Vector4[objs.Length];

        for (int i = 0; i < rmObjs.Length; ++i)
        {
            objs[i] = rmObjs[i].gameObject;
        }

        GameObject obj;
        for (int i = 0; i < objs.Length; ++i)
        {
            obj = objs[i].gameObject;


            invModelMats[i] = obj.transform.localToWorldMatrix.inverse;
            primitiveTypes[i] = (int)obj.GetComponent<RMPrimitive>().PrimitiveType;
            combineOps[i] = obj.GetComponent<RMPrimitive>().CombineOp;
            primitiveGeoInfo[i] = obj.GetComponent<RMPrimitive>().GeoInfo;
            boundGeoInfo[i] = obj.GetComponent<RMPrimitive>().BoundGeoInfo;
        }


        const int RESOLUTION = 10;
        Vector3 volumeArea = new Vector3(5, 5, 5);
        int width = RESOLUTION * (int)volumeArea.x;
        int height = RESOLUTION * (int)volumeArea.y;
        int length = RESOLUTION * (int)volumeArea.z;

        width = _resolution.x * _volumeBounds.size.x;
        height = _resolution.y * _volumeBounds.size.y;
        length = _resolution.z * _volumeBounds.size.z;



        float[] voxels = new float[width * height * length];


        int kernel = _sdfCompute.FindKernel("CSMain");

        // Create a compute buffer.
        ComputeBuffer buffer = new ComputeBuffer(voxels.Length, sizeof(float));
        buffer.SetData(voxels);
        _sdfCompute.SetBuffer(kernel, "_voxels", buffer);
        //_collisionCompute.SetTexture(0, "Result", tex);

        _sdfCompute.SetFloat("_maxDrawDist", 300.0f);
        _sdfCompute.SetMatrixArray("_invModelMats", invModelMats);
        _sdfCompute.SetInts("_primitiveTypes", primitiveTypes);
        _sdfCompute.SetVectorArray("_combineOps", combineOps);
        _sdfCompute.SetVectorArray("_primitiveGeoInfo", primitiveGeoInfo);
        _sdfCompute.SetVectorArray("_boundGeoInfo", primitiveGeoInfo);
        //_sdfCompute.SetVector("_volumeArea", volumeArea);
        _sdfCompute.SetVector("_volumeArea", new Vector4(_volumeBounds.size.x, _volumeBounds.size.y, _volumeBounds.size.z));

        Matrix4x4 localToWorld = new Matrix4x4();
        localToWorld.SetTRS(_volumeBounds.position, Quaternion.identity, Vector3.one);
        _sdfCompute.SetMatrix("_volumeLocalToWorld", localToWorld);

        //int numThreadGroups = objs.Length;
        //_sdfCompute.Dispatch(kernel, (int)volumeArea.x, 1, 1);
        _sdfCompute.Dispatch(kernel, _volumeBounds.size.x, 1, 1);



        //Set the mode used to create the mesh.
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
        Marching marching = null;
        if (mode == MARCHING_MODE.TETRAHEDRON)
            marching = new MarchingTertrahedron();
        else
            marching = new MarchingCubes();

        //Surface is the value that represents the surface of mesh
        //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
        //The target value does not have to be the mid point it can be any value with in the range.
        marching.Surface = 0.0f;


        buffer.GetData(voxels);

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels, width, height, length, verts, indices);

        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.

        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        for (int i = 0; i < numMeshes; i++)
        {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for (int j = 0; j < maxVertsPerMesh; j++)
            {
                int idx = i * maxVertsPerMesh + j;

                if (idx < verts.Count)
                {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if (splitVerts.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = m_material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.transform.localPosition = new Vector3(0, 0, 0);
            //go.transform.localPosition = new Vector3(0.0f, 4.0f, 0.0f);
            //go.transform.localPosition = new Vector3(x * 1.0f, y + 10.0f, z * 1.0f);
            //go.transform.localScale = new Vector3(.0323f, .0323f, .0323f);
            go.transform.localScale = new Vector3(.102f, .102f, .102f);

            meshes.Add(go);
        }

        buffer.Release();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.7f);
        //Gizmos.DrawCube(_volumeBounds.position, _volumeBounds.size);
        Gizmos.DrawWireCube(_volumeBounds.position, _volumeBounds.size);
    }
}
