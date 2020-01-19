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


    void Start()
    {
        // Get all ray march objects.
        RMObj[] rmObjs = (RMObj[])FindObjectsOfType(typeof(RMObj));
        GameObject[] objs = new GameObject[rmObjs.Length];

        for (int i = 0; i < rmObjs.Length; ++i)
        {
            objs[i] = rmObjs[i].gameObject;
        }


        int width = 32;
        int height = 32;
        int length = 32;

        float[] voxels = new float[width * height * length];

        // Extract all neccessary info, and pack into float array.
        Matrix4x4[] invModelMats = new Matrix4x4[objs.Length];
        int[] primitiveTypes = new int[objs.Length];
        Vector4[] combineOps = new Vector4[objs.Length];
        Vector4[] primitiveGeoInfo = new Vector4[objs.Length];


        GameObject obj;
        for (int i = 0; i < objs.Length; ++i)
        {
            obj = objs[i].gameObject;


            invModelMats[i] = obj.transform.localToWorldMatrix.inverse;
            primitiveTypes[i] = (int)obj.GetComponent<RMPrimitive>().PrimitiveType;
            combineOps[i] = obj.GetComponent<RMPrimitive>().CombineOp;
            primitiveGeoInfo[i] = obj.GetComponent<RMPrimitive>().GeoInfo;


        }


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

        ////The size of voxel array.
        //int width = 32;
        //int height = 32;
        //int length = 32;

        //float[] voxels = new float[width * height * length];

        //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
        //for (int x = 0; x < width; x++)
        //{
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int z = 0; z < length; z++)
        //        {
        //            float fx = x / (width - 1.0f);
        //            float fy = y / (height - 1.0f);
        //            float fz = z / (length - 1.0f);

        //            int idx = x + y * width + z * width * height;

        //            voxels[idx] = fractal.Sample3D(fx, fy, fz);


        //        }
        //    }
        //}
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
            go.transform.localPosition = new Vector3(-width / 2, -height / 2, -length / 2);

            meshes.Add(go);



            buffer.Release();
        }
    }

        // Update is called once per frame
        void Update()
    {
        
    }
}
