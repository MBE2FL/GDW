using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif
using System;
using UnityEngine.Events;
using System.Linq;


[Serializable]
public struct NodePoint
{
    public float _dist;
    public Vector3 _pos;
    public Vector3 _colour;
    public Vector3 _normal;
}

[Serializable]
public struct Node
{
    public NodePoint _botFrontLeft;
    public NodePoint _topFrontLeft;
    public NodePoint _topFrontRight;
    public NodePoint _botFrontRight;
    public NodePoint _botBackLeft;
    public NodePoint _topBackLeft;
    public NodePoint _topBackRight;
    public NodePoint _botBackRight;
    public uint _mortonCode;

    public string toString()
    {
        string info = "BFL: " + _botFrontLeft._pos.ToString() + "\n";
        info += "TFL: " + _topFrontLeft._pos.ToString() + "\n";
        info += "TFR: " + _topFrontRight._pos.ToString() + "\n";
        info += "BFR: " + _botFrontRight._pos.ToString() + "\n";
        info += "BBL: " + _botBackLeft._pos.ToString() + "\n";
        info += "TBL: " + _topBackLeft._pos.ToString() + "\n";
        info += "BBR: " + _botBackRight._pos.ToString() + "\n";
        info += "Code: " + _mortonCode;

        return info;
    }
}

[Serializable]
public struct DebugNodeInfo
{
    public Bounds _bounds;
    public int _depth;
    public Node _node;
    public bool _anyNegatives;
    public float _minDist;
    public float _maxDist;
}

[Serializable]
public struct GPUDebugNodeInfo
{
    public Vector3 _centre;
    public Vector3 _min;
    public Vector3 _max;
    public Vector3Int _id;
    public uint _linearID;
    public Vector3 _size;
    public uint _nodesPerAxis;
    public uint _maxDepth;
    public int _anyNegatives;
    public float _minDist;
    public float _maxDist;
}

public enum OctreeMethod
{
    CPU,
    GPU_Full_Octree,
    GPU_Big_Brain
}


[ExecuteInEditMode]
[AddComponentMenu("Ray Marching/RayMarcher")]
[DisallowMultipleComponent]
public class RayMarcher : MonoBehaviour
{
    [SerializeField]
    private List<RayMarchShader> _shaders = new List<RayMarchShader>();

    private static RayMarcher _instance;

    [SerializeField]
    Transform _sunlight;


    [SerializeField]
    RenderTexture _renderTex;
    [SerializeField]
    RenderTexture _renderDepthTex;

    //[SerializeField]
    //RMComputeRender _mortonTestShader;

    //[SerializeField]
    //public Bounds _testBounds;
    //public List<DebugNodeInfo> _interiorBounds;
    //public float _totalNodes;



    public static RayMarcher Instance
    {
        get
        {

            if (!_instance)
            {
                _instance = GameObject.Find("RayMarch Manager").GetComponent<RayMarcher>();
            }

            return _instance;
        }
    }

    public Transform Sunlight
    {
        get
        {
            return _sunlight;
        }
        set
        {
            _sunlight = value;
        }
    }

    public RenderTexture RenderTex
    {
        get
        {
            return _renderTex;
        }
        set
        {
            _renderTex = value;
        }
    }

    public RenderTexture RenderDepthTex
    {
        get
        {
            return _renderDepthTex;
        }
        set
        {
            _renderDepthTex = value;
        }
    }



    public List<RayMarchShader> Shaders
    {
        get
        {
            return _shaders;
        }
    }

    private void Start()
    {
        _shaders = new List<RayMarchShader>(GetComponents<RayMarchShader>());
    }


    private void OnDrawGizmos()
    {
        //Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
        //Gizmos.DrawWireCube(_testBounds.center, _testBounds.size);

        //Bounds bounds;
        //Node node;
        //Color colour;
        //foreach (DebugNodeInfo debugNodeInfo in _interiorBounds)
        //{
        //    bounds = debugNodeInfo._bounds;

        //    if (bounds.center.z < _testBounds.center.z)
        //        continue;

        //    node = debugNodeInfo._node;

        //    if (debugNodeInfo._anyNegatives)
        //        colour = Color.Lerp(Color.white, Color.red, debugNodeInfo._minDist / -7.0f);
        //    else
        //        colour = Color.Lerp(Color.green, Color.white, debugNodeInfo._maxDist / 3.0f);


        //    //colour = Color.Lerp(Color.white, Color.red, debugNodeInfo._depth / 5.0f);
        //    //colour.a = debugNodeInfo._depth / 5.0f;
        //    //Vector3 colour = new Vector3(debugNodeInfo._depth / 5.0f, debugNodeInfo._depth / 5.0f, debugNodeInfo._depth / 5.0f);
        //    //Gizmos.color = new Color(colour.x, colour.y, colour.z, debugNodeInfo._depth / 5.0f);
        //    Gizmos.color = colour;
        //    Gizmos.DrawWireCube(bounds.center, bounds.size);
        //    //Gizmos.DrawCube(bounds.center, bounds.size);
        //}
    }

    public void generateOctrees(RMComputeRender shader)
    {
        List<RMObj> validObjs = shader.RenderList.FindAll(obj =>
        {
            // Primitive: Valid iff it is not a CSG node.
            if (obj.IsPrim)
            {
                return !(obj as RMPrimitive).CSGNode;
            }
            // CSG: Valid iff it is a root CSG.
            else
            {
                return (obj as CSG).IsRoot;
            }
        });


        switch (shader.OctreeMethod)
        {
            case OctreeMethod.CPU:
                Bounds bounds;
                List<Node> octree;
                List<DebugNodeInfo> octreeDebugInfo;
                foreach (RMObj obj in validObjs)
                {
                    bounds = obj.OctreeBounds;
                    octree = obj.Octree;
                    octreeDebugInfo = obj.OctreeDebugInfo;

                    octree.Clear();
                    octreeDebugInfo.Clear();

                    createOctreeCPU(ref bounds, 0, obj.MaxDepth, ref octree, ref octreeDebugInfo, ref shader);
                }
                break;
            case OctreeMethod.GPU_Full_Octree:
                Node[] octreeArr;
                GPUDebugNodeInfo[] octreeDebugInfoArr;
                foreach (RMObj obj in validObjs)
                {
                    bounds = obj.OctreeBounds;
                    //octree = obj.Octree;
                    //octreeDebugInfo = obj.OctreeGPUDebugInfo;

                    //octree.Clear();
                    //octreeDebugInfo.Clear();

                    createOctreeGPUFullOctree(ref bounds, obj.MaxDepth, out octreeArr, out octreeDebugInfoArr, ref shader);

                    octree = new List<Node>(octreeArr);
                    //obj.Octree = new List<Node>(octreeArr);
                    radixSort(ref octree, 10);
                    obj.Octree = octree;
                    obj.OctreeGPUDebugInfo = new List<GPUDebugNodeInfo>(octreeDebugInfoArr);
                }
                break;
            case OctreeMethod.GPU_Big_Brain:
                Debug.LogWarning("GPU Big Brain Method Not Implemented Yet!");
                break;
            default:
                Debug.LogError("Unkown Octree Method Selected!");
                break;
        }
    }

    public void createOctreeCPU(ref Bounds bounds, int depth, uint maxDepth, ref List<Node> octree, ref List<DebugNodeInfo> octreeDebugInfo, ref RMComputeRender shader)
    {
        // Create a new node.
        Vector3 botBackLeft = bounds.min;
        Vector3 topFrontRight = bounds.max;
        Vector3 botFrontLeft = new Vector3(botBackLeft.x, botBackLeft.y, topFrontRight.z);
        Vector3 topFrontLeft = new Vector3(botBackLeft.x, topFrontRight.y, topFrontRight.z);
        Vector3 botFrontRight = new Vector3(topFrontRight.x, botBackLeft.y, topFrontRight.z);
        Vector3 topBackLeft = new Vector3(botBackLeft.x, topFrontRight.y, botBackLeft.z);
        Vector3 topBackRight = new Vector3(topFrontRight.x, topFrontRight.y, botBackLeft.z);
        Vector3 botBackRight = new Vector3(topFrontRight.x, botBackLeft.y, botBackLeft.z);
        createNode(out Node node, ref bounds, ref botFrontLeft, ref topFrontLeft, ref topFrontRight, ref botFrontRight, ref botBackLeft, 
                    ref topBackLeft, ref topBackRight, ref botBackRight);


        // Sample the distance field for every points position.
        node._botFrontLeft._dist = map(ref botFrontLeft, ref shader);
        node._topFrontLeft._dist = map(ref topFrontLeft, ref shader);
        node._topFrontRight._dist = map(ref topFrontRight, ref shader);
        node._botFrontRight._dist = map(ref botFrontRight, ref shader);
        node._botBackLeft._dist = map(ref botBackLeft, ref shader);
        node._topBackLeft._dist = map(ref topBackLeft, ref shader);
        node._topBackRight._dist = map(ref topBackRight, ref shader);
        node._botBackRight._dist = map(ref botBackRight, ref shader);

        float[] distValues = new float[8];
        distValues[0] = node._botFrontLeft._dist;
        distValues[1] = node._topFrontLeft._dist;
        distValues[2] = node._topFrontRight._dist;
        distValues[3] = node._botFrontRight._dist;
        distValues[4] = node._botBackLeft._dist;
        distValues[5] = node._topBackLeft._dist;
        distValues[6] = node._topBackRight._dist;
        distValues[7] = node._botBackRight._dist;
        bool subdividePredicate = distValues.Any(dist => dist < 0.0f) && distValues.Any(dist => dist > 0.0f);

        // DEBUG ONLY
        DebugNodeInfo debugNodeInfo = new DebugNodeInfo()
        {
            _bounds = bounds,
            _depth = depth,
            _node = node,
            _anyNegatives = distValues.Any(dist => dist < 0.0f),
            _minDist = distValues.Min(),
            _maxDist = distValues.Max()
        };
        //_interiorBounds.Add(debugNodeInfo);
        octreeDebugInfo.Add(debugNodeInfo);
        octree.Add(node);

        //++_totalNodes;


        // Root depth: subdivide and evaluate all eight new octants.
        if (depth == 0)
        {
            subdivide(ref bounds, out Bounds botFrontLeftBounds, out Bounds topFrontLeftBounds, out Bounds topFrontRightBounds, out Bounds botFrontRightBounds,
                out Bounds botBackLeftBounds, out Bounds topBackLeftBounds, out Bounds topBackRightBounds, out Bounds botBackRightBounds);

            ++depth;

            createOctreeCPU(ref botFrontLeftBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref topFrontLeftBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref topFrontRightBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref botFrontRightBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref botBackLeftBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref topBackLeftBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref topBackRightBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref botBackRightBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
        }
        // Greater than Root depth and a mix of positive and negative distances: subdivide and evaluate all eight new octants.  
        else if ((depth < maxDepth) && subdividePredicate)
        {
            subdivide(ref bounds, out Bounds botFrontLeftBounds, out Bounds topFrontLeftBounds, out Bounds topFrontRightBounds, out Bounds botFrontRightBounds,
                out Bounds botBackLeftBounds, out Bounds topBackLeftBounds, out Bounds topBackRightBounds, out Bounds botBackRightBounds);

            ++depth;

            createOctreeCPU(ref botFrontLeftBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref topFrontLeftBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref topFrontRightBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref botFrontRightBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref botBackLeftBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref topBackLeftBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref topBackRightBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
            createOctreeCPU(ref botBackRightBounds, depth, maxDepth, ref octree, ref octreeDebugInfo, ref shader);
        }

    }

    void subdivide(ref Bounds bounds, out Bounds botFrontLeft, out Bounds topFrontLeft, out Bounds topFrontRight, out Bounds botFrontRight,
                    out Bounds botBackLeft, out Bounds topBackLeft, out Bounds topBackRight, out Bounds botBackRight)
    {
        Vector3 size = bounds.size * 0.5f;
        Vector3 centre = bounds.center;

        Vector3 halfSize = size * 0.5f;

        botFrontLeft = new Bounds(new Vector3(centre.x - halfSize.x, centre.y - halfSize.y, centre.z + halfSize.z), size);
        topFrontLeft = new Bounds(new Vector3(centre.x - halfSize.x, centre.y + halfSize.y, centre.z + halfSize.z), size);
        topFrontRight = new Bounds(new Vector3(centre.x + halfSize.x, centre.y + halfSize.y, centre.z + halfSize.z), size);
        botFrontRight = new Bounds(new Vector3(centre.x + halfSize.x, centre.y - halfSize.y, centre.z + halfSize.z), size);
        botBackLeft = new Bounds(new Vector3(centre.x - halfSize.x, centre.y - halfSize.y, centre.z - halfSize.z), size);
        topBackLeft = new Bounds(new Vector3(centre.x - halfSize.x, centre.y + halfSize.y, centre.z - halfSize.z), size);
        topBackRight = new Bounds(new Vector3(centre.x + halfSize.x, centre.y + halfSize.y, centre.z - halfSize.z), size);
        botBackRight = new Bounds(new Vector3(centre.x + halfSize.x, centre.y - halfSize.y, centre.z - halfSize.z), size);
    }

    void createNode(out Node node, ref Bounds bounds, ref Vector3 botFrontLeft, ref Vector3 topFrontLeft, ref Vector3 topFrontRight, ref Vector3 botFrontRight,
                    ref Vector3 botBackLeft, ref Vector3 topBackLeft, ref Vector3 topBackRight, ref Vector3 botBackRight)
    {
        // Generate morton codes for each points position.
        // Cartesian coordinates or node coordinates??????

        // Cartesian method:
        // Normalize point to range [0, 1], based on bounding box's min and max points.
        //Vector3 min = bounds.min;

        //Vector3 PMinusMin = botFrontLeft - min;
        //Vector3 normBFL = new Vector3(PMinusMin.x / bounds.size.x, PMinusMin.y / bounds.size.y, PMinusMin.z / bounds.size.z);

        //PMinusMin = topFrontLeft - min;
        //Vector3 normTFL = new Vector3(PMinusMin.x / bounds.size.x, PMinusMin.y / bounds.size.y, PMinusMin.z / bounds.size.z);

        //PMinusMin = topFrontRight - min;
        //Vector3 normTFR = new Vector3(PMinusMin.x / bounds.size.x, PMinusMin.y / bounds.size.y, PMinusMin.z / bounds.size.z);

        //PMinusMin = botFrontRight - min;
        //Vector3 normBFR = new Vector3(PMinusMin.x / bounds.size.x, PMinusMin.y / bounds.size.y, PMinusMin.z / bounds.size.z);

        //PMinusMin = botBackLeft - min;
        //Vector3 normBBL = new Vector3(PMinusMin.x / bounds.size.x, PMinusMin.y / bounds.size.y, PMinusMin.z / bounds.size.z);

        //PMinusMin = topBackLeft - min;
        //Vector3 normTBL = new Vector3(PMinusMin.x / bounds.size.x, PMinusMin.y / bounds.size.y, PMinusMin.z / bounds.size.z);

        //PMinusMin = topBackRight - min;
        //Vector3 normTBR = new Vector3(PMinusMin.x / bounds.size.x, PMinusMin.y / bounds.size.y, PMinusMin.z / bounds.size.z);

        //PMinusMin = botBackRight - min;
        //Vector3 normBBR = new Vector3(PMinusMin.x / bounds.size.x, PMinusMin.y / bounds.size.y, PMinusMin.z / bounds.size.z);

        // Create a morton code with the normalized point.
        Vector3 normCentre = bounds.center - bounds.min;
        normCentre = new Vector3(normCentre.x / bounds.size.x, normCentre.y / bounds.size.y, normCentre.z / bounds.size.z);

        // Generate a node with all the points.
        node = new Node() { 
            _botFrontLeft = new NodePoint(),
            _topFrontLeft = new NodePoint(),
            _topFrontRight = new NodePoint(),
            _botFrontRight = new NodePoint(),
            _botBackLeft = new NodePoint(),
            _topBackLeft = new NodePoint(),
            _topBackRight = new NodePoint(),
            _botBackRight = new NodePoint(),
            _mortonCode = createMortonCode(ref normCentre)
        };
    }

    uint createMortonCode(ref Vector3 point)
    {
       return morton3D(point.x, point.y, point.z);
    }

    // Expands a 10-bit integer into 30 bits
    // by inserting 2 zeros after each bit.
    uint expandBits(uint v)
    {
        v = (v * 0x00010001u) & 0xFF0000FFu;
        v = (v * 0x00000101u) & 0x0F00F00Fu;
        v = (v * 0x00000011u) & 0xC30C30C3u;
        v = (v * 0x00000005u) & 0x49249249u;
        return v;
    }

    // Calculates a 30-bit Morton code for the
    // given 3D point located within the unit cube [0,1].
    uint morton3D(float x, float y, float z)
    {
        x = Mathf.Min(Mathf.Max(x * 1024.0f, 0.0f), 1023.0f);
        y = Mathf.Min(Mathf.Max(y * 1024.0f, 0.0f), 1023.0f);
        z = Mathf.Min(Mathf.Max(z * 1024.0f, 0.0f), 1023.0f);
        uint xx = expandBits((uint)x);
        uint yy = expandBits((uint)y);
        uint zz = expandBits((uint)z);
        return xx * 4 + yy * 2 + zz;
    }

    float map(ref Vector3 p, ref RMComputeRender shader)
    {
        float scene = 400.0f;

        Vector4 pos = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        Vector4 geoInfo = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        float obj;
        float obj2;

        float csg;
        float[] storedCSGs = new float[16];

        Vector3 cell = new Vector3(0.0f, 0.0f, 0.0f);

        // ######### New Game Object #########
        //pos = shader._invModelMats[0] * new Vector4(p.x, p.y, p.z, 1.0f);
        //geoInfo = shader._primitiveGeoInfo[0];
        //obj = sdSphere(pos, geoInfo.x);

        //scene = opSmoothUnion(scene, obj, shader._combineOps[0].y);
        // ######### New Game Object #########

        int primIndex = 0;
        int csgIndex = 0;
        int altIndex = 0;
        foreach (RMObj rmObj in shader.RenderList)
        {
            // Primitive object
            if (rmObj.IsPrim)
            {
                pos = shader._invModelMats[primIndex] * new Vector4(p.x, p.y, p.z, 1.0f);
                geoInfo = shader._primitiveGeoInfo[primIndex];
                obj = sdSphere(pos, geoInfo.x);

                scene = opSmoothUnion(scene, obj, shader._combineOps[primIndex].y);

                ++primIndex;
            }
            // CSG object
            else
            {

            }
        }

        return scene;
    }

    float sdSphere(Vector3 p, float s)
    {
        return p.magnitude - s;
    }

    float opSmoothUnion(float d1, float d2, float k)
    {
        float h = Mathf.Clamp(0.5f + (0.5f * (d2 - d1) / k), 0.0f, 1.0f);

        return Mathf.Lerp(d2, d1, h) - (k * h * (1.0f - h));
    }

    void countingSort(List<Node> octree, ref List<Node> sortedOctree, uint digit, uint radix)
    {
        uint[] _temp = new uint[radix];


        // Count the number of occurences of each digit in each node's morton code.
        uint i = 0;
        uint digitOfCodeI;
        foreach (Node node in octree)
        {
            digitOfCodeI = (node._mortonCode / (uint)Mathf.Pow(radix, digit)) % radix;
            //_temp[digitOfCodeI] = _temp[digitOfCodeI] + 1;
            ++_temp[digitOfCodeI];

            ++i;
        }

        // _temp is modified to show the cumulative # of digits up to that index of _temp.
        for (i = 1; i < radix; ++i)
        {
            _temp[i] = _temp[i] + _temp[i - 1];
        }

        /* Go through _octree backwards, add elements to _sortedOctree by checking the value of _octree[i],
         * going to _temp[_octree[i]], writing the value of the element at _octree[i] to _sortedOctree[_temp[_octree[i]]].
         * Decrement the value of _temp[_octree[i]] by 1 since that slot in _sortedOctree is now occupied.
         */
        for (int j = octree.Count - 1; j > -1; --j)
        {
            digitOfCodeI = (octree[j]._mortonCode / (uint)Mathf.Pow(radix, digit)) % radix;
            --_temp[digitOfCodeI];
            sortedOctree[(int)_temp[digitOfCodeI]] = octree[j];
        }
    }

    void radixSort(ref List<Node> octree, uint radix)
    {
        uint max = octree.Max(node => node._mortonCode);

        List<Node> sortedOctree = new List<Node>(octree);

        uint digits = (uint)(Mathf.FloorToInt(Mathf.Log(max, radix) + 1));

        for (uint digit = 0; digit < digits; ++digit)
        {
            countingSort(octree, ref sortedOctree, digit, radix);
            octree = new List<Node>(sortedOctree);
        }
    }




    void createOctreeGPUFullOctree(ref Bounds bounds, uint maxDepth, out Node[] octree, out GPUDebugNodeInfo[] octreeDebugInfo, ref RMComputeRender shader)
    {
        //int totalNodes = (int)(Mathf.Pow(8, maxDepth + 1) - 1) / 7;
        int totalNodes = (int)Mathf.Pow(2.0f, maxDepth);
        totalNodes *= totalNodes * totalNodes;
        octree = new Node[totalNodes];
        octreeDebugInfo = new GPUDebugNodeInfo[totalNodes];

        ComputeShader compShader = shader.OctreeGenShader;

        ComputeBuffer buffer = new ComputeBuffer(totalNodes, (sizeof(float) * 80) + sizeof(uint));
        buffer.SetData(octree);

        ComputeBuffer debugBuffer = new ComputeBuffer(totalNodes, (sizeof(float) * 14) + (sizeof(uint) * 3) + sizeof(uint) * 3 + sizeof(int));
        debugBuffer.SetData(octreeDebugInfo);

        //ComputeBuffer rootBoundsBuf = new ComputeBuffer(1, sizeof(float) * 6);
        //Bounds[] boundsArr = { bounds };
        //rootBoundsBuf.SetData(boundsArr);

        shader.updateOctreeData();


        int kernelIndex = compShader.FindKernel("CSMain");
        compShader.SetBuffer(kernelIndex, "_octree", buffer);
        compShader.SetBuffer(kernelIndex, "_octreeDebugInfo", debugBuffer);
        compShader.SetInt("_maxDepth", (int)maxDepth);
        compShader.SetVector("_rootBoundsMin", bounds.min);
        compShader.SetVector("_rootBoundsSize", bounds.size);
        compShader.SetMatrixArray("_invModelMats", shader._invModelMats);
        compShader.SetVectorArray("_rm_colours", shader._colours);
        compShader.SetVectorArray("_combineOps", shader._combineOps);
        compShader.SetVectorArray("_primitiveGeoInfo", shader._primitiveGeoInfo);
        compShader.SetVectorArray("_altInfo", shader._altInfo);
        compShader.SetVectorArray("_bufferedCSGs", shader._bufferedCSGs);
        compShader.SetVectorArray("_combineOpsCSGs", shader._combineOpsCSGs);

        int nodesPerAxis = (int)Mathf.Pow(2.0f, maxDepth);
        //compShader.Dispatch(kernelIndex, nodesPerAxis / 4, nodesPerAxis / 4, nodesPerAxis / 4);
        compShader.Dispatch(kernelIndex, nodesPerAxis / 2, nodesPerAxis / 2, nodesPerAxis / 2);

        buffer.GetData(octree);
        debugBuffer.GetData(octreeDebugInfo);

        buffer.Release();
        debugBuffer.Release();
    }


    public void addShader(ShaderType shaderType)
    {
        // RayMarchShader shader = ScriptableObject.CreateInstance<RayMarchShader>();
        // shader.ShaderName = "New Shader";
        // _shaders.Add(shader);


        RayMarchShader shader;

        switch (shaderType)
        {
            case ShaderType.FragRendering:
                shader = gameObject.AddComponent<RMRenderShader>();
                break;
            case ShaderType.MarchingCube:
                shader = gameObject.AddComponent<RMMarchingCubeShader>();
                shader.ShaderType = ShaderType.MarchingCube;
                break;
            case ShaderType.Collision:
                return;
            case ShaderType.Rendering:
                shader = gameObject.AddComponent<RMComputeRender>();
                break;
            default:
                return;
        }

        _shaders.Add(shader);
    }

    public void removeShader(RayMarchShader shader)
    {
        //int index = _shaders.IndexOf(shader);
        //_shaders[index] = _shaders[_shaders.Count - 1];
        //_shaders.RemoveAt(_shaders.Count - 1);

        _shaders.Remove(shader);
    }

    public void removeShader(int index)
    {
        //_shaders[index] = _shaders[_shaders.Count - 1];
        //_shaders.RemoveAt(_shaders.Count - 1);

        _shaders.RemoveAt(index);
    }

    public void removeAllShaders()
    {
        foreach (RayMarchShader shader in _shaders)
        {
#if UNITY_EDITOR
            DestroyImmediate(shader);
#else
            Destroy(shader);
#endif
        }

        _shaders.Clear();
    }

    public void moveUp(RayMarchShader shader)
    {
        // int index = _shaders.IndexOf(shader);
        // RayMarchShader temp = _shaders[index];
    }


    public void render(RenderTexture source, RenderTexture destination)
    {
        //EffectMaterial.EnableKeyword("BOUND_DEBUG");  // TO-DO Perform this only when debug is _enabled.
        //EffectMaterial.shaderKeywords = new string[1] { "BOUNDING_SPHERE_DEBUG" };
        //Matrix4x4 torusMat = Matrix4x4.TRS(
        //                                    Vector3.right * Mathf.Sin(Time.time) * 5.0f,
        //                                    Quaternion.identity,
        //                                    Vector3.one);
        //torusMat *= Matrix4x4.TRS(
        //                           Vector3.zero,
        //                           Quaternion.Euler(new Vector3(0.0f, 0.0f, (Time.time * 200.0f) % 360.0f)),
        //                           Vector3.one);

        //EffectMaterial.SetMatrix("_TorusMat_InvModel", torusMat.inverse);


        //if (!_distTex)
        //{
        //    _distTex = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.RFloat);
        //}
    }


    /// <summary>
    /// Custom version of Graphics.Blit that encodes frustum indices into the input vertices.
    /// 
    /// Top Left vertex:        z=0, u=0, v=0
    /// Top Right vertex:       z=1, u=1, v=0
    /// Bottom Right vertex:    z=2, u=1, v=1
    /// Bottom Left vertex:     z=3, u=1, v=0
    /// </summary>
    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNum, bool finalPass, ref RenderTexture distTex)
    {
        //RenderTexture.active = dest;

        RenderTexture distanceMap = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.RFloat);
        RenderTexture sceneTex = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        RenderBuffer[] buffers = new RenderBuffer[2] { sceneTex.colorBuffer, distanceMap.colorBuffer };
        //RenderBuffer[] buffers = new RenderBuffer[2] { sceneTex.colorBuffer, distTex.colorBuffer };
        Graphics.SetRenderTarget(buffers, dest.depthBuffer);


        fxMaterial.SetTexture("_MainTex", source);

        if (fxMaterial.IsKeywordEnabled("USE_DIST_TEX"))
        {
            fxMaterial.SetTexture("_distTex", distTex);
        }



        GL.PushMatrix();
        GL.LoadOrtho();

        fxMaterial.SetPass(passNum);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);   // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);   // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);   // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);   // TL

        GL.End();
        GL.PopMatrix();


        //Graphics.Blit(distanceMap, dest);

        if (finalPass)
        {
            Graphics.Blit(sceneTex, dest);
        }
        else
        {
            Graphics.Blit(sceneTex, source);
        }

        Graphics.Blit(distanceMap, distTex);

        //if (distTex)
        //{
        //    distTex.Release();
        //    distTex = new RenderTexture(distanceMap);
        //}

        //sceneTex.Release();
        //distanceMap.Release();
        RenderTexture.ReleaseTemporary(sceneTex);
        RenderTexture.ReleaseTemporary(distanceMap);
    }


    /// <summary>
    /// Stores the normalized rays representing the camera frustum in a 4x4 matrix. Each row is a vector.
    /// 
    /// The following rays are stored in each row (in eyespace, not worldspace):
    /// Top Left corner:        row=0
    /// Top Right corner:       row=1
    /// Bottom Right corner:    row=2
    /// Bottom Left  corner:    row=3
    /// </summary>
    /// <param name="cam">The camera to calculate the frustum corner rays.</param>
    /// <returns>A 4x4 matrix containing the 4 corner frustum rays.</returns>
    private Matrix4x4 GetFrustumCorners(Camera cam)
    {
        float camFov = cam.fieldOfView;
        float camAspect = cam.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovWHalf = camFov * 0.5f;

        float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 toRight = Vector3.right * tan_fov * camAspect;
        Vector3 toTop = Vector3.up * tan_fov;

        Vector3 topLeft = (-Vector3.forward - toRight + toTop);
        Vector3 topRight = (-Vector3.forward + toRight + toTop);
        Vector3 bottomRight = (-Vector3.forward + toRight - toTop);
        Vector3 bottomLeft = (-Vector3.forward - toRight - toTop);

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        return frustumCorners;
    }


    [MenuItem("GameObject/Ray Marched/Sphere", false, 10)]
    static void CreateBox(MenuCommand menuCommand)
    {
        GameObject obj = new GameObject();
        obj.AddComponent<RMPrimitive>();

        // Ensure the obj gets parented if this was a context click (otherwise does nothing).
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(obj, obj.name);
    }

    [MenuItem("GameObject/Ray Marched/CSG", false, 10)]
    static void CreateCSG(MenuCommand menuCommand)
    {
        GameObject obj = new GameObject();
        obj.AddComponent<CSG>();

        // Ensure the obj gets parented if this was a context click (otherwise does nothing).
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(obj, obj.name);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RayMarcher))]
public class RayMarcherEditor : Editor
{
    List<RayMarchShader> shaders;
    SerializedProperty _shaders;
    SerializedProperty _sunlight;
    SerializedProperty _renderTex;
    SerializedProperty _renderDepthTex;
    ShaderType _shaderType = ShaderType.FragRendering;
    //int _selectedShaderIndex = 0;
    //SerializedProperty _mortonTestShader;
    //SerializedProperty _testBounds;
    //SerializedProperty _totalNodes;

    private void OnEnable()
    {
        _shaders = serializedObject.FindProperty("_shaders");
        _sunlight = serializedObject.FindProperty("_sunlight");
        _renderTex = serializedObject.FindProperty("_renderTex");
        _renderDepthTex = serializedObject.FindProperty("_renderDepthTex");
        //_mortonTestShader = serializedObject.FindProperty("_mortonTestShader");
        //_testBounds = serializedObject.FindProperty("_testBounds");
        //_totalNodes = serializedObject.FindProperty("_totalNodes");
    }



    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        var rayMarcher = target as RayMarcher;

        serializedObject.Update();

        EditorGUILayout.PropertyField(_sunlight);
        EditorGUILayout.PropertyField(_renderTex);
        EditorGUILayout.PropertyField(_renderDepthTex);
        //EditorGUILayout.PropertyField(_mortonTestShader);
        //EditorGUILayout.PropertyField(_testBounds);
        //EditorGUILayout.LabelField("Total Nodes: " + _totalNodes.floatValue);

        //if (GUILayout.Button("Octree Test"))
        //{
        //    rayMarcher._interiorBounds.Clear();
        //    rayMarcher.createOctree(ref rayMarcher._testBounds, 0);
        //}

        if (GUILayout.Button("Clear Render Textures"))
        {
            rayMarcher.RenderTex.Release();
            rayMarcher.RenderTex = null;
            rayMarcher.RenderDepthTex.Release();
            rayMarcher.RenderDepthTex = null;
        }


        EditorGUILayout.Space(6.0f);
        GUIContent label = new GUIContent("Shaders", "");
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        label.text = "Shader Type";
        _shaderType = (ShaderType)EditorGUILayout.EnumPopup(label, _shaderType);

        // Add a another shader to this RayMarcher.
        if (GUILayout.Button("Add Shader"))
        {
            rayMarcher.addShader(_shaderType);
        }

        // Remove all shaders from this RayMarcher.
        EditorGUILayout.Space(4.0f);
        if (GUILayout.Button("Remove All Shaders"))
        {
            rayMarcher.removeAllShaders();
        }
        EditorGUILayout.Space(10.0f);

        // Display all shaders.
        shaders = rayMarcher.Shaders;
        for (int i = 0; i < shaders.Count; ++i)
        {
            //EditorGUILayout.Space(2.0f);

            if (GUILayout.Button(shaders[i].ShaderName))
            {
                ShaderEditorWindow.Init(shaders[i]);
            }
        }



        serializedObject.ApplyModifiedProperties();

    }
}

public class ShaderEditorWindow : EditorWindow, ISerializationCallbackReceiver
{
    static Camera _camera;
    static List<System.Type> _desiredDockNextTo = new List<System.Type>();


    static RayMarcher _rayMarcher;


    Vector2 shaderScrollPos;
    Vector2 renderListScrollPos;
    bool _renderListFoldout = false;
    RayMarchShader _shader;
    //event Action<RMObj> _onRemoveObj;
    //event Action _onRemoveShader;

    // Add menu named "Shader Editor" to the Window menu
    //[MenuItem("Window/Shader Editor")]
    //public static void Init()
    //{
    //    // Get existing open window or if none, make a new one:
    //    //ShaderEditorWindow window = (ShaderEditorWindow)EditorWindow.GetWindow(typeof(ShaderEditorWindow));
    //    //window.Show();

    //    ShaderEditorWindow window = CreateWindow<ShaderEditorWindow>(_desiredDockNextTo.ToArray());
    //    window.Show();
    //    _desiredDockNextTo.Add(window.GetType());

    //    _rayMarcher = RayMarcher.Instance;
    //}


    [Serializable]
    public struct SerializedShader
    {
        public int index;
        public bool _serialized;
        //public Shader _effectShader;
        //public string _shaderName;
        //public RayMarchShaderSettings _settings;
        //public ShaderType _shaderType;
        //public List<RMObj> _renderList;
        //public ComputeShader _sdfToMeshShader;
    }

    [SerializeField]
    SerializedShader _serializedShader;


    public static void Init(RayMarchShader shader)
    {
        // Get existing open window or if none, make a new one:
        //ShaderEditorWindow window = (ShaderEditorWindow)EditorWindow.GetWindow(typeof(ShaderEditorWindow));
        //window.Show();

        ShaderEditorWindow window = CreateWindow<ShaderEditorWindow>(_desiredDockNextTo.ToArray());
        window.Show();
        _desiredDockNextTo.Add(window.GetType());
        window._shader = shader;

        window.titleContent = new GUIContent(shader.ShaderName);

        _rayMarcher = RayMarcher.Instance;

        //window._onRemoveObj += window.removeObj;
        //window._onRemoveShader += window.removeShader;
    }

    void initIfNeeded()
    {
        if (!_rayMarcher)
            _rayMarcher = RayMarcher.Instance;

        if (!_shader && _serializedShader._serialized)
        {
            //Debug.Log("Deserialized YES");
            _rayMarcher = RayMarcher.Instance;

            //_serializedShader._serialized = false;

            _shader = _rayMarcher.Shaders[_serializedShader.index];
        }
    }

    void OnGUI()
    {
        initIfNeeded();

        shaderScrollPos = GUILayout.BeginScrollView(shaderScrollPos);


        GUIContent label = new GUIContent();


        // Display the current shader's effect shader.
        if (_shader.ShaderType == ShaderType.FragRendering)
        {
            label.text = "Effect Shader";
            label.tooltip = "";
            _shader.EffectShader = EditorGUILayout.ObjectField(label, _shader.EffectShader, typeof(Shader), true) as Shader;
        }
        else if (_shader.ShaderType == ShaderType.MarchingCube)
        {
            label.text = "SDF To Mesh Shader";
            label.tooltip = "";
            (_shader as RMMarchingCubeShader).SDFtoMeshShader = EditorGUILayout.ObjectField(label, (_shader as RMMarchingCubeShader).SDFtoMeshShader, typeof(ComputeShader), true) as ComputeShader;
        }
        else if (_shader.ShaderType == ShaderType.Rendering)
        {
            label.text = "Compute Shader";
            label.tooltip = "";
            (_shader as RMComputeRender).Shader = EditorGUILayout.ObjectField(label, (_shader as RMComputeRender).Shader, typeof(ComputeShader), true) as ComputeShader;
        }

        // Display the current shader's name.
        EditorGUILayout.BeginHorizontal();
        label.text = "Shader Name";
        EditorGUILayout.PrefixLabel(label);
        _shader.ShaderName = EditorGUILayout.TextField(_shader.ShaderName);
        EditorGUILayout.EndHorizontal();

        // Settings retrieved from a scriptable object.
        if (_shader.ShaderType == ShaderType.FragRendering)
        {
            label.text = "Settings";
            label.tooltip = "";
            _shader.Settings = EditorGUILayout.ObjectField(label, _shader.Settings, typeof(RayMarchShaderSettings), true) as RayMarchShaderSettings;
        }

        // Octree settings
        if (_shader.ShaderType == ShaderType.Rendering)
        {
            label.text = "Settings";
            label.tooltip = "";
            _shader.Settings = EditorGUILayout.ObjectField(label, _shader.Settings, typeof(RayMarchShaderSettings), true) as RayMarchShaderSettings;

            EditorGUILayout.Space(4.0f);

            label.text = "Octree Settings";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Octree Method";
            (_shader as RMComputeRender).OctreeMethod = (OctreeMethod)EditorGUILayout.EnumPopup(label, (_shader as RMComputeRender).OctreeMethod);

            label.text = "Octree Generation Shader";
            (_shader as RMComputeRender).OctreeGenShader = EditorGUILayout.ObjectField(label, (_shader as RMComputeRender).OctreeGenShader, typeof(ComputeShader), true) as ComputeShader;

            EditorGUILayout.Space(2.0f);

            if (GUILayout.Button("Generate Octrees"))
            {
                _rayMarcher.generateOctrees(_shader as RMComputeRender);
            }

            EditorGUILayout.Space(4.0f);
        }

        // Objects in the current shader's render list.
        label.text = "Render List";
        _renderListFoldout = EditorGUILayout.Foldout(_renderListFoldout, label, true);
        List<RMObj> _renderList;
        renderListScrollPos = EditorGUILayout.BeginScrollView(renderListScrollPos);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(6.0f);

        if (_renderListFoldout)
        {
            _renderList = _shader.RenderList;
            foreach (RMObj obj in _renderList)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField(obj.name, EditorStyles.centeredGreyMiniLabel);

                if (GUILayout.Button("Remove"))
                {
                    //_onRemoveObj.Invoke(obj);
                    removeObj(obj);
                    return;
                }

                EditorGUILayout.Space(2.0f);

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(6.0f);
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();


        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Remove Shader"))
        {
            //_onRemoveShader.Invoke();
            removeShader();
            return;
        }
    }

    //private void OnDestroy()
    //{
    //    _onRemoveObj -= removeObj;
    //    _onRemoveShader -= removeShader;
    //}

    private void removeObj(RMObj rmObj)
    {
        //_shader.removeFromRenderList(rmObj);
        //rmObj.removeFromShaderList(_shader);
        rmObj.remove();
    }

    private void removeShader()
    {
        RayMarcher.Instance.removeShader(_shader);
        //_shader.removeAllFromRenderList();
        _shader.remove();

        // Delete the shader.
#if UNITY_EDITOR
        DestroyImmediate(_shader);
#else
        Destroy(_shader);
#endif
    }

    public void OnBeforeSerialize()
    {
        // Now Unity is free to serialize this field, and we should get back the expected 
        // data when it is deserialized later.

        _serializedShader = new SerializedShader()
        {
            index = _rayMarcher.Shaders.IndexOf(_shader),
            _serialized = true
            //_effectShader = _shader.EffectShader,
            //_shaderName = _shader.ShaderName,
            //_settings = _shader.Settings,
            //_shaderType = _shader.ShaderType,
            //_renderList = _shader.RenderList,
        }
        ;

        switch (_shader.ShaderType)
        {
            case ShaderType.FragRendering:
                break;
            case ShaderType.MarchingCube:
                //_serializedShader._sdfToMeshShader = (_shader as RMMarchingCubeShader).SDFtoMeshShader;
                break;
            case ShaderType.Collision:
                break;
            default:
                break;
        }
    }

    public void OnAfterDeserialize()
    {
        //Unity has just written new data into the serializedNodes field.
        //let's populate our actual runtime data with those new values.

        // Transfer the deserialized data into the internal Node class
        //RayMarchShader shader = RayMarcher.Instance.Shaders[_serializedShader.index];

        //switch (_serializedShader._shaderType)
        //{
        //    case ShaderType.Rendering:
        //        shader = new RMRenderShader();
        //        shader.EffectShader = _serializedShader._effectShader;
        //        shader.ShaderName = _serializedShader._shaderName;
        //        shader.Settings = _serializedShader._settings;
        //        shader.ShaderType = _serializedShader._shaderType;
        //        shader.RenderList = _serializedShader._renderList;
        //        break;
        //    case ShaderType.MarchingCube:
        //        shader = new RMMarchingCubeShader();
        //        shader.EffectShader = _serializedShader._effectShader;
        //        shader.ShaderName = _serializedShader._shaderName;
        //        shader.Settings = _serializedShader._settings;
        //        shader.ShaderType = _serializedShader._shaderType;
        //        shader.RenderList = _serializedShader._renderList;
        //        (shader as RMMarchingCubeShader).SDFtoMeshShader = _serializedShader._sdfToMeshShader;
        //        break;
        //    case ShaderType.Collision:
        //        break;
        //    default:
        //        break;
        //}

        //_shader = shader;
    }
}

/* public class ShaderEditorWindow : EditorWindow
{
    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;
    static Camera _camera;
    static RayMarcher _rayMarcher;
    static List<RayMarchShader> _shaders;

    static List<string> _toolbarNames = new List<string>();
    static int _toolbarSelected = 0;
    Vector2 scrollPos;


    // Add menu named "Shader Editor" to the Window menu
    [MenuItem("Window/Shader Editor")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        ShaderEditorWindow window = (ShaderEditorWindow)EditorWindow.GetWindow(typeof(ShaderEditorWindow));
        window.Show();

        _camera = Camera.main;
        _rayMarcher = _camera.GetComponent<RayMarcher>();
    }

    public static void rebuildNames()
    {
        _toolbarNames.Clear();

        _shaders = _rayMarcher.Shaders;

        foreach (RayMarchShader shader in _shaders)
        {
            _toolbarNames.Add(shader.ShaderName);
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        _toolbarSelected = GUILayout.Toolbar(_toolbarSelected, _toolbarNames.ToArray());
        GUILayout.EndHorizontal();


        scrollPos = GUILayout.BeginScrollView(scrollPos);


        GUIContent label = new GUIContent();

        _shaders = _rayMarcher.Shaders;
        foreach (RayMarchShader shader in _shaders)
        {
            // ######### General Variables #########
            label.text = "General Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Shader Name";
            shader.ShaderName = EditorGUILayout.TextField(label, shader.ShaderName);

            label.text = "Max Steps";
            label.tooltip = "The maximum number of steps each ray can take.";
            shader.MaxSteps = EditorGUILayout.IntField(label, shader.MaxSteps);

            label.text = "Max Draw Dist";
            label.tooltip = "The maximum distance each pixel can travel.";
            shader.MaxDrawDist = EditorGUILayout.FloatField(label, shader.MaxDrawDist);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Light Variables #########
            label.text = "Light Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Specular Exp";
            label.tooltip = "Affects the size of the specular highlight";
            shader.SpecularExp = EditorGUILayout.FloatField(label, shader.SpecularExp);

            label.text = "Attenuation Constant";
            label.tooltip = "";
            shader.AttenuationConstant = EditorGUILayout.FloatField(label, shader.AttenuationConstant);

            label.text = "Attenuation Linear";
            label.tooltip = "";
            shader.AttenuationLinear = EditorGUILayout.FloatField(label, shader.AttenuationLinear);

            label.text = "Attenuation Quadratic";
            label.tooltip = "";
            shader.AttenuationQuadratic = EditorGUILayout.FloatField(label, shader.AttenuationQuadratic);

            label.text = "Ambient Colour";
            label.tooltip = "";
            shader.AmbientColour = EditorGUILayout.ColorField(label, shader.AmbientColour);

            label.text = "Diffuse Colour";
            label.tooltip = "";
            shader.DiffuseColour = EditorGUILayout.ColorField(label, shader.DiffuseColour);

            label.text = "Specular Colour";
            label.tooltip = "";
            shader.SpecualarColour = EditorGUILayout.ColorField(label, shader.SpecualarColour);

            label.text = "Light Constants";
            label.tooltip = "";
            shader.LightConstants = EditorGUILayout.Vector3Field(label, shader.LightConstants);

            label.text = "Rim Light Colour";
            label.tooltip = "";
            shader.RimLightColour = EditorGUILayout.ColorField(label, shader.RimLightColour);

            label.text = "Sun Light";
            label.tooltip = "Directional light representing the sun.";
            shader.sunLight = EditorGUILayout.ObjectField(label, shader.sunLight, typeof(Transform), true) as Transform;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Shadow Variables #########
            label.text = "Shadow Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Penumbra Factor";
            label.tooltip = "How soft the shadows appear, the further away they are from the occluder.";
            shader.PenumbraFactor = EditorGUILayout.FloatField(label, shader.PenumbraFactor);

            label.text = "Shadow Min Dist";
            label.tooltip = "A bias to prevent the shadow rays from getting stuck inside of their origin surface.";
            shader.ShadowmMinDist = EditorGUILayout.FloatField(label, shader.ShadowmMinDist);

            label.text = "Shadow Intensity";
            label.tooltip = "How strong the shadows appear.";
            shader.ShadowIntensity = EditorGUILayout.FloatField(label, shader.ShadowIntensity);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Reflection Variables #########
            label.text = "Reflection Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Reflection Count";
            label.tooltip = "The maximum amount of reflection rays sllowed.";
            shader.ReflectionCount = EditorGUILayout.IntField(label, shader.ReflectionCount);

            label.text = "Reflection Intensity";
            label.tooltip = "The strength of the reflection.";
            shader.ReflectionIntensity = EditorGUILayout.FloatField(label, shader.ReflectionIntensity);

            label.text = "Env Refl Intensity";
            label.tooltip = "The strength of the environment (skybox) reflection.";
            shader.ReflectionIntensity = EditorGUILayout.FloatField(label, shader.ReflectionIntensity);

            label.text = "Skybox";
            label.tooltip = "";
            shader.SkyBox = EditorGUILayout.ObjectField(label, shader.SkyBox, typeof(Texture), true) as Texture;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Ambient Occlusion Variables #########
            label.text = "Ambient Occlusion Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "AO Max Steps";
            label.tooltip = "The maximum number of steps each AO ray can take.";
            shader.AOMaxSteps = EditorGUILayout.IntField(label, shader.AOMaxSteps);

            label.text = "AO Step Size";
            label.tooltip = "The size of each step an AO ray marches.";
            shader.AOStepSize = EditorGUILayout.FloatField(label, shader.AOStepSize);

            label.text = "AO Intensity";
            label.tooltip = "The intensity of the AO effect.";
            shader.AOItensity = EditorGUILayout.FloatField(label, shader.AOItensity);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Vignette Variables #########
            label.text = "Vignette Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Vignette Intensity";
            label.tooltip = "";
            shader.VignetteIntesnity = EditorGUILayout.FloatField(label, shader.VignetteIntesnity);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // ######### Fog Variables #########
            label.text = "Fog Settings";
            label.tooltip = "";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            label.text = "Fog Extinction";
            label.tooltip = "TO-DO";
            shader.FogExtinction = EditorGUILayout.FloatField(label, shader.FogExtinction);

            label.text = "Fog Inscattering";
            label.tooltip = "TO-DO";
            shader.FogInscattering = EditorGUILayout.FloatField(label, shader.FogInscattering);

            label.text = "Fog Colour";
            label.tooltip = "";
            shader.FogColour = EditorGUILayout.ColorField(label, shader.FogColour);

            GUILayout.EndScrollView();
        }
    }
} */
#endif