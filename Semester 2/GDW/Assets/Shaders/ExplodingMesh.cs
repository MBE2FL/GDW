using UnityEngine;

public class ExplodingMesh : MonoBehaviour
{
    public bool useNormals = true;

    private ComputeShader explosiveShader;
    private ComputeShader shader;
    private ComputeShader displacementShader;
    private ComputeShader memoryShader;
    public float lerp = -3.0f;
    public float dropDistance = 0.0f;

    public Material oldMaterial;
    Material newMaterial;

    bool matChange = false;

    Mesh mesh;

    struct Tri
    {
        public Vector3 p1;
        public Vector3 p3;
        public Vector3 p2;
    }

    int[] triangles;
    Vector3[] vertices;
    Vector3[] normals;
    Vector2[] uvs;

    Vector3[] newNormals;
    Vector2[] newUvs;
    Vector3[] newVerts;
    int[] newTris;
    
    
    //public Vector3[] data;
    Vector3[] baseData;
    Vector3[] output;
    bool reset = true;
    bool re = true;
    void Start()
    {
        shader = (ComputeShader)Resources.Load("GeoCompute Shader");
        explosiveShader = (ComputeShader)Resources.Load("Explosive Shader");
        displacementShader = (ComputeShader)Resources.Load("Displacement Shader");
        memoryShader = (ComputeShader)Resources.Load("Memory Allocation Shader");

        mesh = GetComponent<MeshFilter>().sharedMesh;
        triangles = (int[])mesh.triangles.Clone();
        vertices = (Vector3[])mesh.vertices.Clone();
        normals = (Vector3[])mesh.normals.Clone();
        uvs = (Vector2[])mesh.uv.Clone();

        newMaterial = gameObject.GetComponent<MeshRenderer>().material;

        newNormals = new Vector3[triangles.Length];
        newUvs = new Vector2[triangles.Length];
        newVerts = new Vector3[triangles.Length];
        newTris = new int[triangles.Length];
        baseData = new Vector3[triangles.Length];
        //data = new Vector3[triangles.Length];
        output = new Vector3[triangles.Length];


        for (int i = 0; i < triangles.Length; i++)
        {
            newTris[i] = i;
        }

        RunMemShader();
        // mesh.vertices = newVerts;
        // mesh.uv2 = new Vector4[triangles.Length];
        // mesh.uv3 = new Vector2[triangles.Length];
        // mesh.uv4 = new Vector2[triangles.Length];
        // mesh.uv = newUvs;
        // mesh.triangles = newTris;
        // mesh.normals = newNormals;
        //mesh = new Mesh();
        mesh.SetVertices(newVerts);
        mesh.SetTriangles(newTris, 0);
        mesh.SetUVs(1, new Vector2[triangles.Length]);
        mesh.SetUVs(2, new Vector2[triangles.Length]);
        mesh.SetUVs(3, new Vector2[triangles.Length]);
        mesh.SetUVs(0, newUvs);
        mesh.SetNormals(newNormals);

        if (useNormals)
        {
            RunExpShader();
        }
        else
        {
            RunDisShader();
        }



        Vector2[] temp1 = new Vector2[triangles.Length];
        Vector2[] temp2 = new Vector2[triangles.Length];
        Vector2[] temp3 = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            //temp1[i].x = baseData[i].x;
            //temp1[i].y = newVerts[i].x;
            //
            //temp2[i].x = baseData[i].y;
            //temp2[i].y = newVerts[i].y;
            //
            //temp3[i].x = baseData[i].z;
            //temp3[i].y = newVerts[i].z;

            temp1[i].x = baseData[i].x;
            temp1[i].y = baseData[i].y;

            temp2[i].x = baseData[i].z;
            temp2[i].y = newVerts[i].x;

            temp3[i].x = newVerts[i].y;
            temp3[i].y = newVerts[i].z;

        }
        mesh.SetUVs(1, temp1);
        mesh.SetUVs(2, temp2);
        mesh.SetUVs(3, temp3);

        {
            /*data = new Matrix4x4[mesh.triangles.Length/3];
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
            RunShader();*/
        }
    }

    //private void RunShader()
    //{
    //    if (lerp > 0.99)
    //    {
    //        if (reset)
    //        {
    //            mesh.triangles = triangles;
    //            mesh.vertices = vertices;
    //            mesh.normals = normals;
    //            mesh.uv = uvs;
    //            reset = false;
    //            re = true;
    //        }
    //    }
    //    else if (data.Length > 0)
    //    {
    //        ComputeBuffer buffer = new ComputeBuffer(data.Length, 3 * 4);
    //        ComputeBuffer baseBuffer = new ComputeBuffer(baseData.Length, 3 * 4);

    //        buffer.SetData(data);
    //        baseBuffer.SetData(baseData);

    //        int kernelHandle = shader.FindKernel("CSMain");
    //        shader.SetBuffer(kernelHandle, "dataBuffer", buffer);
    //        shader.SetFloat("t", lerp);

    //        //shader.Dispatch(kernelHandle, data.Length, 1, 1);
    //        shader.SetBuffer(kernelHandle, "baseBuffer", baseBuffer);
    //        shader.Dispatch(kernelHandle, baseData.Length, 1, 1);

    //        //output = data;
    //        buffer.GetData(output); ;
    //        buffer.Dispose();
    //        baseBuffer.Dispose();

    //        mesh.vertices = output;
    //        if (re)
    //        {
    //            mesh.triangles = newTris;
    //            mesh.normals = newNormals;
    //            mesh.uv = newUvs;
    //            reset = true;
    //            re = false;
    //        }
    //    }
    //    //mesh.vertices = vertices;
    //    //mesh.triangles = triangles;
    //}

    private void RunDisShader()
    {
        if (newVerts.Length > 0)
        {
            ComputeBuffer buffer = new ComputeBuffer(newVerts.Length, 3 * 4);
            buffer.SetData(newVerts);

            int kernelHandle = displacementShader.FindKernel("Main");
            displacementShader.SetBuffer(kernelHandle, "dataBuffer", buffer);

            displacementShader.Dispatch(kernelHandle, newVerts.Length, 1, 1);

            buffer.GetData(newVerts);

            buffer.Dispose();
        }

    }

    private void RunExpShader()
    {
        if (newVerts.Length > 0)
        {

            Tri[] triangle = new Tri[newVerts.Length / 3];
            Tri[] norm = new Tri[newVerts.Length / 3];
            int j = 0;
            for (int i = 0; i < triangle.Length; i++)
            {
                triangle[i].p1 = newVerts[j];
                norm[i].p1 = newNormals[j];
                j++;

                triangle[i].p2 = newVerts[j];
                norm[i].p2 = newNormals[j];
                j++;

                triangle[i].p3 = newVerts[j];
                norm[i].p3 = newNormals[j];
                j++;
            }

            ComputeBuffer buffer = new ComputeBuffer(triangle.Length, 9 * 4);
            buffer.SetData(triangle);

            ComputeBuffer normBuffer = new ComputeBuffer(norm.Length, 9 * 4);
            normBuffer.SetData(norm);

            int kernelHandle = explosiveShader.FindKernel("Main");
            explosiveShader.SetBuffer(kernelHandle, "dataBuffer", buffer);
            
            explosiveShader.SetBuffer(kernelHandle, "normBuffer", normBuffer);
            explosiveShader.Dispatch(kernelHandle, normals.Length, 1, 1);


            buffer.GetData(triangle);

            j = 0;
            for (int i = 0; i < triangle.Length; i++)
            {

                newVerts[j] = triangle[i].p1;
                j++;

                newVerts[j] = triangle[i].p2;
                j++;

                newVerts[j] = triangle[i].p3;
                j++;
            }

                buffer.Dispose();
        }

    }

    private void RunMemShader()
    {

        //for (int i = 0; i < newNormals.Length; i++)
        //{
        //    newNormals[i] = mesh.normals[mesh.triangles[i]];
        //    newUvs[i] = mesh.uv[mesh.triangles[i]];
        //    baseData[i] = newVerts[i] = mesh.vertices[mesh.triangles[i]];
        //    //baseData[i] = mesh.vertices[mesh.triangles[i]];
        //    newTris[i] = i;
        //}
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
            //memoryShader.Dispatch(kernelHandle, mesh.triangles.Length, 1, 1);

            memoryShader.SetBuffer(kernelHandle, "normals", normBuffer);
            //memoryShader.Dispatch(kernelHandle, mesh.normals.Length, 1, 1);
            memoryShader.SetBuffer(kernelHandle, "newNormals", newNormBuffer);
            //memoryShader.Dispatch(kernelHandle, newNormals.Length, 1, 1);

            memoryShader.SetBuffer(kernelHandle, "uvs", uvBuffer);
            //memoryShader.Dispatch(kernelHandle, mesh.uv.Length, 1, 1);
            memoryShader.SetBuffer(kernelHandle, "newUvs", newUVBuffer);
            //memoryShader.Dispatch(kernelHandle, newUvs.Length, 1, 1);

            memoryShader.SetBuffer(kernelHandle, "Vertices", vertBuffer);
            //memoryShader.Dispatch(kernelHandle, mesh.vertices.Length, 1, 1);
            memoryShader.SetBuffer(kernelHandle, "baseData", baseBuffer);
            //memoryShader.Dispatch(kernelHandle, baseData.Length, 1, 1);
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
        lerp += Time.deltaTime * 0.5f;
        //Vector4[] temp3 = new Vector4[mesh.triangles.Length];
        if (lerp > 0.999f)
        {
            lerp = 1.0f;
            if (oldMaterial && matChange)
            {
                gameObject.GetComponent<MeshRenderer>().material = oldMaterial;
                matChange = false;
            }
        }
        else
        {
            if (!matChange)
            {
                gameObject.GetComponent<MeshRenderer>().material = newMaterial;
                matChange = true;
            }

            gameObject.GetComponent<MeshRenderer>().material.SetFloat("t", lerp);
        }


        //for (int i = 0; i < mesh.triangles.Length; i++)
        //{
        //    temp3[i].x = lerp;
        //}
        //mesh.SetUVs(3, temp3);
    }

    private void OnApplicationQuit()
    {
        if (mesh != null)
        {
            // mesh.vertices = vertices;
            // mesh.triangles = triangles;
            // mesh.normals = normals;
            // mesh.uv = uvs;
            mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles,0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.uv2 = null;
            mesh.uv3 = null;
            mesh.uv4 = null; 
        }
    }
}
