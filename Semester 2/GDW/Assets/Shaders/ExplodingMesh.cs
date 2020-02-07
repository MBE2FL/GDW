using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingMesh : MonoBehaviour
{
    public ComputeShader shader;
    public float lerp = 0.0f;
    public float dropDistance = 0.0f;

    Mesh mesh;

    int[] triangles;
    Vector3[] vertices;
    Vector3[] normals;
    Vector2[] uvs;

    Vector3[] newNormals;
    Vector2[] newUvs;
    
    
    Matrix4x4[] data;
    Matrix4x4[] baseData;
    Matrix4x4[] output;
    void Start()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        triangles = mesh.triangles;
        vertices = mesh.vertices;
        normals = mesh.normals;
        uvs = mesh.uv;

        newNormals = new Vector3[triangles.Length];
        newUvs = new Vector2[triangles.Length];


        for(int i = 0; i < newNormals.Length; i++)
        {
            newNormals[i] = mesh.normals[mesh.triangles[i]];
            newUvs[i] = mesh.uv[mesh.triangles[i]];
        }
        
        

        data = new Matrix4x4[mesh.triangles.Length/3];
        output = new Matrix4x4[mesh.triangles.Length/3];
        
        baseData = new Matrix4x4[mesh.triangles.Length/3];

        int j = 0;
        for (int i = 0; i < data.Length; i++)
        {
            data[i].SetRow(0, new Vector4(mesh.vertices[mesh.triangles[j]].x,
                mesh.vertices[mesh.triangles[j]].y - dropDistance - i *0.1f,
                mesh.vertices[mesh.triangles[j]].z, 0));
            baseData[i].SetRow(0, new Vector4(mesh.vertices[mesh.triangles[j]].x,
                mesh.vertices[mesh.triangles[j]].y,
                mesh.vertices[mesh.triangles[j]].z, 0));
            j++;
            
            data[i].SetRow(1, new Vector4(mesh.vertices[mesh.triangles[j]].x,
                mesh.vertices[mesh.triangles[j]].y - dropDistance - i * 0.1f,
                mesh.vertices[mesh.triangles[j]].z, 0));
            baseData[i].SetRow(1, new Vector4(mesh.vertices[mesh.triangles[j]].x,
                mesh.vertices[mesh.triangles[j]].y,
                mesh.vertices[mesh.triangles[j]].z, 0));
            j++;

            data[i].SetRow(2, new Vector4(mesh.vertices[mesh.triangles[j]].x,
                mesh.vertices[mesh.triangles[j]].y - dropDistance - i * 0.1f,
                mesh.vertices[mesh.triangles[j]].z, 0));
            baseData[i].SetRow(2, new Vector4(mesh.vertices[mesh.triangles[j]].x,
                mesh.vertices[mesh.triangles[j]].y,
                mesh.vertices[mesh.triangles[j]].z, 0));
            j++;

            data[i].SetRow(3, new Vector4(0, 0, 0, 0));
            baseData[i].SetRow(3, new Vector4(0, 0, 0, 0)); 

        }
        RunShader();
    }

    private void RunShader()
    {
        if (lerp > 0.999999)
        {
            mesh.triangles = triangles;
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
        }
        else
        {
            ComputeBuffer buffer = new ComputeBuffer(data.Length, 16 * 4);
            ComputeBuffer baseBuffer = new ComputeBuffer(baseData.Length, 16 * 4);

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

            int[] tri = new int[output.Length * 3];
            for (int i = 0; i < tri.Length; i++)
            {
                tri[i] = i;
            }
            Vector3[] verts = new Vector3[mesh.triangles.Length];
            int j = 0;
            for (int i = 0; i < data.Length; i++)
            {
                verts[j] = new Vector3(output[i].GetRow(0).x, output[i].GetRow(0).y, output[i].GetRow(0).z);
                j++;

                verts[j] = new Vector3(output[i].GetRow(1).x, output[i].GetRow(1).y, output[i].GetRow(1).z);
                j++;

                verts[j] = new Vector3(output[i].GetRow(2).x, output[i].GetRow(2).y, output[i].GetRow(2).z);
                j++;
            }



            mesh.vertices = verts;
            mesh.triangles = tri;
            mesh.normals = newNormals;
            mesh.uv = newUvs;

        }
        //mesh.vertices = vertices;
        //mesh.triangles = triangles;
    }
    private void Update()
    {
        RunShader();
        lerp += Time.deltaTime * 0.5f;
    }

    private void OnApplicationQuit()
    {
        mesh.triangles = triangles;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
    }
}
