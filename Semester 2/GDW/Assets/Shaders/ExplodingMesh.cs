using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingMesh : MonoBehaviour
{
    private ComputeShader shader;
    private ComputeShader displacementShader;
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
    
    
    Vector3[] data;
    Vector3[] baseData;
    Vector3[] output;
    void Start()
    {
        shader = (ComputeShader)Resources.Load("GeoCompute Shader");
        displacementShader = (ComputeShader)Resources.Load("Displacement Shader");

        mesh = GetComponent<MeshFilter>().sharedMesh;
        triangles = mesh.triangles;
        vertices = mesh.vertices;
        normals = mesh.normals;
        uvs = mesh.uv;

        newNormals = new Vector3[triangles.Length];
        newUvs = new Vector2[triangles.Length];
        newVerts = new Vector3[triangles.Length];


        for(int i = 0; i < newNormals.Length; i++)
        {
            newNormals[i] = mesh.normals[mesh.triangles[i]];
            newUvs[i] = mesh.uv[mesh.triangles[i]];
            newVerts[i] = mesh.vertices[mesh.triangles[i]];
        }

        baseData = newVerts;

        RunDisShader();

        data = newVerts;

        output = new Vector3[triangles.Length];


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
        RunShader();
    }

    private void RunShader()
    {
        if (lerp > 0.99)
        {
            mesh.triangles = triangles;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
        }
        else
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
            //buffer.Dispose();
            //baseBuffer.Dispose();

            int[] tri = new int[triangles.Length];
            for (int i = 0; i < tri.Length; i++)
            {
                tri[i] = i;
            }
            



            mesh.vertices = output;
            mesh.triangles = tri;
            mesh.normals = newNormals;
            mesh.uv = newUvs;

        }
        //mesh.vertices = vertices;
        //mesh.triangles = triangles;
    }

    private void RunDisShader()
    {
        ComputeBuffer buffer = new ComputeBuffer(newVerts.Length, 3 * 4);
        buffer.SetData(newVerts);

        int kernelHandle = displacementShader.FindKernel("CSMain");
        displacementShader.SetBuffer(kernelHandle, "dataBuffer", buffer);

        displacementShader.Dispatch(kernelHandle, newVerts.Length, 1, 1);
        
        buffer.GetData(newVerts); ;
        
    }


    private void Update()
    {
        RunShader();
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
