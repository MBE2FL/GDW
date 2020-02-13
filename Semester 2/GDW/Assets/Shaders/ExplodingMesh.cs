using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingMesh : MonoBehaviour
{
    private ComputeShader shader;
    private ComputeShader displacementShader;
    private ComputeShader memoryShader;
    public float lerp = 0.0f;
    public float dropDistance = 0.0f;

    Mesh mesh;

    int[] triangles;
    Vector3[] vertices;
    Vector3[] normals;
    Vector2[] uvs;

    Vector3[] newNormals;
    Vector2[] newUvs;
    Vector3[] newVerts;
    int[] newTris;
    
    
    public Vector3[] data;
    public Vector3[] baseData;
    Vector3[] output;
    bool reset = true;
    bool re = true;
    void Start()
    {
        shader = (ComputeShader)Resources.Load("GeoCompute Shader");
        displacementShader = (ComputeShader)Resources.Load("Displacement Shader");
        memoryShader = (ComputeShader)Resources.Load("Memory Allocation Shader");

        mesh = GetComponent<MeshFilter>().sharedMesh;
        triangles = mesh.triangles;
        vertices = mesh.vertices;
        normals = mesh.normals;
        uvs = mesh.uv;
        


        newNormals = new Vector3[triangles.Length];
        newUvs = new Vector2[triangles.Length];
        newVerts = new Vector3[triangles.Length];
        newTris = new int[triangles.Length];
        baseData = new Vector3[triangles.Length];
        data = new Vector3[triangles.Length];
        output = new Vector3[triangles.Length];


        for(int i = 0; i < triangles.Length; i++)
        {
            //newNormals[i] = mesh.normals[mesh.triangles[i]];
            //newUvs[i] = mesh.uv[mesh.triangles[i]];
            //baseData[i] = newVerts[i] = mesh.vertices[mesh.triangles[i]];
            //baseData[i] = mesh.vertices[mesh.triangles[i]];
            newTris[i] = i;

        }

        RunMemShader();

        RunDisShader();

        mesh.vertices = newVerts;
        mesh.uv2 = new Vector2[triangles.Length];
        mesh.uv =  newUvs;
        mesh.triangles = newTris;
        mesh.normals = newNormals;

        Vector2[] temp = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            temp[i].x = baseData[i].y;
            temp[i].y = data[i].y;

        }
        mesh.uv2 = temp;


            //data = new Matrix4x4[mesh.triangles.Length/3];
            //output = new Matrix4x4[mesh.triangles.Length/3];
            //
            //baseData = new Matrix4x4[mesh.triangles.Length/3];
            //
            //int j = 0;
            //for (int i = 0; i < data.Length; i++)
            //{
            //    data[i].SetRow(0, new Vector4(mesh.vertices[mesh.triangles[j]].x,
            //        mesh.vertices[mesh.triangles[j]].y - dropDistance - i *0.1f,
            //        mesh.vertices[mesh.triangles[j]].z, 0));
            //    baseData[i].SetRow(0, new Vector4(mesh.vertices[mesh.triangles[j]].x,
            //        mesh.vertices[mesh.triangles[j]].y,
            //        mesh.vertices[mesh.triangles[j]].z, 0));
            //    j++;
            //    
            //    data[i].SetRow(1, new Vector4(mesh.vertices[mesh.triangles[j]].x,
            //        mesh.vertices[mesh.triangles[j]].y - dropDistance - i * 0.1f,
            //        mesh.vertices[mesh.triangles[j]].z, 0));
            //    baseData[i].SetRow(1, new Vector4(mesh.vertices[mesh.triangles[j]].x,
            //        mesh.vertices[mesh.triangles[j]].y,
            //        mesh.vertices[mesh.triangles[j]].z, 0));
            //    j++;
            //
            //    data[i].SetRow(2, new Vector4(mesh.vertices[mesh.triangles[j]].x,
            //        mesh.vertices[mesh.triangles[j]].y - dropDistance - i * 0.1f,
            //        mesh.vertices[mesh.triangles[j]].z, 0));
            //    baseData[i].SetRow(2, new Vector4(mesh.vertices[mesh.triangles[j]].x,
            //        mesh.vertices[mesh.triangles[j]].y,
            //        mesh.vertices[mesh.triangles[j]].z, 0));
            //    j++;
            //
            //    data[i].SetRow(3, new Vector4(0, 0, 0, 0));
            //    baseData[i].SetRow(3, new Vector4(0, 0, 0, 0)); 
            //
            //}
            //RunShader();
        }

    private void RunShader()
    {
        if (lerp > 0.99)
        {
            if (reset)
            {
                mesh.triangles = triangles;
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.uv = uvs;
                reset = false;
                re = true;
            }
        }
        else if (data.Length > 0)
        {
            ComputeBuffer buffer = new ComputeBuffer(data.Length, 3 * 4);
            ComputeBuffer baseBuffer = new ComputeBuffer(baseData.Length, 3 * 4);

            buffer.SetData(data);
            baseBuffer.SetData(baseData);

            int kernelHandle = shader.FindKernel("CSMain");
            shader.SetBuffer(kernelHandle, "dataBuffer", buffer);
            shader.SetFloat("t", lerp);

            shader.Dispatch(kernelHandle, data.Length, 1, 1);
            shader.SetBuffer(kernelHandle, "baseBuffer", baseBuffer);
            shader.Dispatch(kernelHandle, baseData.Length, 1, 1);

            //output = data;
            buffer.GetData(output); ;
            buffer.Dispose();
            baseBuffer.Dispose();

            mesh.vertices = output;
            if (re)
            {
                mesh.triangles = newTris;
                mesh.normals = newNormals;
                mesh.uv = newUvs;
                reset = true;
                re = false;
            }
        }
        //mesh.vertices = vertices;
        //mesh.triangles = triangles;
    }

    private void RunDisShader()
    {
        if (newVerts.Length > 0)
        {
            ComputeBuffer buffer = new ComputeBuffer(newVerts.Length, 3 * 4);
            buffer.SetData(newVerts);

            int kernelHandle = displacementShader.FindKernel("Main");
            displacementShader.SetBuffer(kernelHandle, "dataBuffer", buffer);

            displacementShader.Dispatch(kernelHandle, newVerts.Length, 1, 1);

            buffer.GetData(data);

            buffer.Dispose();
        }

    }

    private void RunMemShader()
    {

        for (int i = 0; i < newNormals.Length; i++)
        {
            newNormals[i] = mesh.normals[mesh.triangles[i]];
            newUvs[i] = mesh.uv[mesh.triangles[i]];
            baseData[i] = newVerts[i] = mesh.vertices[mesh.triangles[i]];
            //baseData[i] = mesh.vertices[mesh.triangles[i]];
            newTris[i] = i;
        }
        if (newVerts.Length > 0)
        {
            ComputeBuffer triBuffer = new ComputeBuffer(mesh.triangles.Length, sizeof(int));
            triBuffer.SetData(mesh.triangles);

            ComputeBuffer normBuffer = new ComputeBuffer(mesh.normals.Length, 3 * 4);
            normBuffer.SetData(mesh.normals);
            ComputeBuffer newNormBuffer = new ComputeBuffer(newNormals.Length, 3 * 4);
            newNormBuffer.SetData(newNormals);

            ComputeBuffer uvBuffer = new ComputeBuffer(mesh.uv.Length, 2 * 4);
            uvBuffer.SetData(mesh.uv);
            ComputeBuffer newUVBuffer = new ComputeBuffer(newUvs.Length, 2 * 4);
            newUVBuffer.SetData(newUvs);

            ComputeBuffer vertBuffer = new ComputeBuffer(mesh.vertices.Length, 3 * 4);
            vertBuffer.SetData(mesh.vertices);
            ComputeBuffer baseBuffer = new ComputeBuffer(baseData.Length, 3 * 4);
            baseBuffer.SetData(baseData);
            ComputeBuffer newVertBuffer = new ComputeBuffer(newVerts.Length, 3 * 4);
            newVertBuffer.SetData(newVerts);

            ComputeBuffer newTriBuffer = new ComputeBuffer(newTris.Length, sizeof(int));
            newTriBuffer.SetData(newTris);


            int kernelHandle = memoryShader.FindKernel("Main");

            memoryShader.SetBuffer(kernelHandle, "tris", triBuffer);
            memoryShader.Dispatch(kernelHandle, mesh.triangles.Length, 1, 1);

            memoryShader.SetBuffer(kernelHandle, "normals", normBuffer);
            memoryShader.Dispatch(kernelHandle, mesh.normals.Length, 1, 1);
            memoryShader.SetBuffer(kernelHandle, "newNormals", newNormBuffer);
            memoryShader.Dispatch(kernelHandle, newNormals.Length, 1, 1);

            memoryShader.SetBuffer(kernelHandle, "uvs", uvBuffer);
            memoryShader.Dispatch(kernelHandle, mesh.uv.Length, 1, 1);
            memoryShader.SetBuffer(kernelHandle, "newUvs", newUVBuffer);
            memoryShader.Dispatch(kernelHandle, newUvs.Length, 1, 1);

            memoryShader.SetBuffer(kernelHandle, "vertices", vertBuffer);
            memoryShader.Dispatch(kernelHandle, mesh.vertices.Length, 1, 1);
            memoryShader.SetBuffer(kernelHandle, "baseData", baseBuffer);
            memoryShader.Dispatch(kernelHandle, baseData.Length, 1, 1);
            memoryShader.SetBuffer(kernelHandle, "newVerts", newVertBuffer);
            memoryShader.Dispatch(kernelHandle, newVerts.Length, 1, 1);


            newNormBuffer.GetData(newNormals);
            newNormBuffer.Dispose();

            newUVBuffer.GetData(newUvs);
            newUVBuffer.Dispose();

            baseBuffer.GetData(baseData);
            baseBuffer.Dispose();
            newVertBuffer.GetData(newVerts);
            newVertBuffer.Dispose();

        }

    }


    private void Update()
    {
        //RunShader();
        lerp += Time.deltaTime * 0.2f;
    }

    private void OnApplicationQuit()
    {
        mesh.triangles = triangles;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
    }
}
