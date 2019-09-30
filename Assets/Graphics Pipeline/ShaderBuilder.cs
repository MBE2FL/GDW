using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Text;

[ExecuteInEditMode]
[AddComponentMenu("Ray Marching/Shader Builder")]
[DisallowMultipleComponent]
public class ShaderBuilder : MonoBehaviour
{
    [SerializeField]
    private string _path;
    [SerializeField]
    private string _name = "Unamed Parsed Shader";
    [SerializeField]
    private string _templateHLSLPath;
    [SerializeField]
    private string _templateShaderPath;
    [HideInInspector]
    private RMMemoryManager _rmMemoryManager;
    [HideInInspector]
    private RayMarcher _rayMarcher;


    private void Awake()
    {
        _path = Application.dataPath + "/Graphics Pipeline/Shaders/";
        _templateHLSLPath = Application.dataPath + "/Graphics Pipeline/Shaders/RayMarchTemplate.hlsl";
        _templateShaderPath = Application.dataPath + "/Graphics Pipeline/Shaders/RayMarchTemplate.shader";
    }

    // Start is called before the first frame update
    void Start()
    {
        // Retrieve a reference to the ray marching memory manager from the main camera.
        _rmMemoryManager = Camera.main.GetComponent<RMMemoryManager>();

        // Make sure a memory manager exists, else create one.
        if (!_rmMemoryManager)
        {
            _rmMemoryManager = Camera.main.gameObject.AddComponent<RMMemoryManager>();
        }

        // Retrieve a reference to the ray marcher from the main camera.
        _rayMarcher = Camera.main.GetComponent<RayMarcher>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void build()
    {
        Debug.Log("Building shader.");

        StringBuilder file = new StringBuilder();


        foreach (string line in File.ReadAllLines(_templateHLSLPath))
        {
            // Replace insert statement.
            if (line.Contains("//<Insert Map Here>"))
            {
                buildMap(ref file);
            }
            else if (line.Contains("//<Insert MapMat Here>"))
            {
                buildMapMat(ref file);
            }
            else if (line.Contains("//<Insert Reflection Here>"))
            {
                parseReflection(ref file);
            }
            // Copy line from template.
            else
            {
                file.AppendLine(line);
            }
        }


        // Write new shader.
        using (StreamWriter shader = new StreamWriter(File.Create(_path + "/" + _name + ".hlsl")))
        {
            shader.Write(file.ToString());
        }


        file.Clear();

        foreach (string line in File.ReadAllLines(_templateShaderPath))
        {
            // Replace insert statements.
            if (line.Contains("//<Insert Shader Name>"))
            {
                file.AppendLine("Shader \"MyPipeline/" + _name + "\"");
            }
            else if (line.Contains("//<Insert Include>"))
            {
                file.AppendLine("\t\t\t#include \"" + _name + ".hlsl\"");
            }
            // Copy line from template.
            else
            {
                file.AppendLine(line);
            }
        }


        // Write new shader.
        using (StreamWriter shader = new StreamWriter(File.Create(_path + "/" + _name + ".shader")))
        {
            shader.Write(file.ToString());
        }


#if UNITY_EDITOR
        AssetDatabase.Refresh();
        //AssetDatabase.ImportAsset("Assets/Graphics Pipeline/Shaders/Resources/" + _name + ".shader");
#endif
    }

    private void buildMap(ref StringBuilder map)
    {
        map.AppendLine("\tfloat scene = _maxDrawDist;");
        map.AppendLine();
        map.AppendLine("\tfloat4 pos = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tfloat4 geoInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine();
        map.AppendLine("\tfloat obj;");
        map.AppendLine("\tfloat obj2;");
        map.AppendLine();
        map.AppendLine("\tfloat csg;");
        map.AppendLine("\tfloat storedCSGs[MAX_CSG_CHILDREN];");
        map.AppendLine();

        uint primIndex = 0;
        uint csgIndex = 0;

        List<RMPrimitive> prims = _rmMemoryManager.RM_Prims;
        List<CSG> csgs = _rmMemoryManager.CSGs;

        List<RMObj> objs = new List<RMObj>(prims.Count + csgs.Count);
        objs.AddRange(prims);
        objs.AddRange(csgs);

        objs.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));

        RMPrimitive prim;
        CSG csg;
        foreach (RMObj obj in objs)
        {
            // Primitive
            if (obj.IsPrim)
            {
                prim = obj as RMPrimitive;

                // Skip any primitives belonging to a csg, as they will be rendered recursively by thier respective csgs.
                if (prim.CSGNode)
                    continue;

                map.AppendLine("\t// ######### " + prim.gameObject.name + " #########");
                parsePrimitive(ref map, prim, ref primIndex);
                map.AppendLine("\t// ######### " + prim.gameObject.name + " #########");
                map.AppendLine();
            }
            // CSG
            else
            {
                csg = obj as CSG;

                // Skip any non-root CSGs, as they will be rendered recursively by thier parents.
                // Skip any CSGs which don't have two nodes.
                if (!csg.IsRoot || !csg.IsValid)
                    continue;

                map.AppendLine("\t// ######### " + csg.gameObject.name + " #########");

                parseCSG(ref map, csg, ref primIndex, ref csgIndex);

                determineCombineOp(ref map, null, csg, csgIndex - 1);
                map.AppendLine("\t// ######### " + csg.gameObject.name + " #########");
                map.AppendLine();
            }
        }

        map.AppendLine("\treturn scene;");
    }

    #region Old
    //private void buildPrimitives(ref StringBuilder map, ref uint primIndex)
    //{

    //    foreach (RMPrimitive prim in _rmMemoryManager.RM_Prims)
    //    {
    //        // Skip any primitives belonging to a csg, as they will be rendered recursively by thier respective csgs.
    //        if (prim.CSGNode)
    //            continue;

    //        parsePrimitive(ref map, prim, ref primIndex);
    //    }
    //}

    //private void buildCSGs(ref StringBuilder map, ref uint primIndex)
    //{
    //    uint csgIndex = 0;

    //    map.AppendLine("\t// ######### Render CSGs #########");
    //    map.AppendLine("\trmPixel obj2;");
    //    map.AppendLine("\trmPixel csg;");
    //    map.AppendLine("\trmPixel storedCSGs[MAX_CSG_CHILDREN];");
    //    map.AppendLine();

    //    foreach (CSG csg in _rmMemoryManager.CSGs)
    //    {
    //        // Skip any non-root CSGs, as they will be rendered recursively by thier parents.
    //        // Skip any CSGs which don't have two nodes.
    //        if (!csg.IsRoot || !csg.IsValid)
    //            continue;

    //        map.AppendLine("\t// ######### " + csg.gameObject.name + " #########");

    //        parseCSG(ref map, csg, ref primIndex, ref csgIndex);

    //        determineCombineOp(ref map, null, csg, csgIndex - 1);
    //        map.AppendLine("\t// ######### " + csg.gameObject.name + " #########");
    //        map.AppendLine();
    //    }
    //}
    #endregion Old

    private void parsePrimitive(ref StringBuilder map, RMPrimitive prim, ref uint primIndex, bool csgNodeTwo = false)
    {
        if (!prim.Static)
        {
            // Determine position and geometric information
            map.AppendLine("\tpos = mul(_invModelMats[" + primIndex + "], float4(p, 1.0));");
            map.AppendLine("\tgeoInfo = _primitiveGeoInfo[" + primIndex + "];");

            string obj = "obj";
            if (csgNodeTwo)
                obj = "obj2";

            map.Append("\t" + obj + " = ");

            // Determine primitive type
            switch (prim.PrimitiveType)
            {
                case PrimitiveTypes.Sphere:
                    map.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                    break;
                case PrimitiveTypes.Box:
                    map.AppendLine("sdBox(pos.xyz, geoInfo.xyz);");
                    break;
                case PrimitiveTypes.Torus:
                    map.AppendLine("sdTorus(pos.xyz, geoInfo.xy);");
                    break;
                case PrimitiveTypes.Cylinder:
                    map.AppendLine("sdCylinder(pos.xyz, geoInfo.x, geoInfo.y);");
                    break;
                case PrimitiveTypes.Tetrahedron:
                    map.AppendLine("sdTetra(pos.xyz);");
                    break;
                case PrimitiveTypes.Mandelbulb:
                    map.AppendLine("sdMandelbulb(pos.xyz, geoInfo.xy);");
                    break;
                default:
                    map.AppendLine("0.0;");
                    Debug.LogError("Shader Parse: Unkown Primitive Type!");
                    break;
            }

            // Store distance into distance buffer
            map.AppendLine("\tdistBuffer[" + primIndex + "] = " + obj + ";");
            map.AppendLine();

            // Determine combining operation
            if (!prim.CSGNode)
            {
                determineCombineOp(ref map, prim, null, primIndex);
            }
            else
                map.AppendLine();

            //map.AppendLine();
            ++primIndex;
        }
        else
        {
            Matrix4x4 mat;
            Vector4 info;

            // Determine position and geometric information
            //map.AppendLine("\tpos = mul(_invModelMats[" + primIndex + "], float4(p, 1.0));");
            mat = prim.transform.localToWorldMatrix.inverse;
            map.AppendLine("\tpos = mul(float4x4(" + mat.m00 + ", " + mat.m01 + ", " + mat.m02 + ", " + mat.m03 + ", "
                                               + mat.m10 + ", " + mat.m11 + ", " + mat.m12 + ", " + mat.m13 + ", "
                                               + mat.m20 + ", " + mat.m21 + ", " + mat.m22 + ", " + mat.m23 + ", "
                                               + mat.m30 + ", " + mat.m31 + ", " + mat.m32 + ", " + mat.m33 + "), float4(p, 1.0));");

            //map.AppendLine("\tgeoInfo = _primitiveGeoInfo[" + primIndex + "];");
            info = prim.GeoInfo;
            map.AppendLine("\tgeoInfo = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

            string obj = "obj";
            if (csgNodeTwo)
                obj = "obj2";

            map.Append("\t" + obj + " = ");

            // Determine primitive type
            switch (prim.PrimitiveType)
            {
                case PrimitiveTypes.Sphere:
                    map.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                    break;
                case PrimitiveTypes.Box:
                    map.AppendLine("sdBox(pos.xyz, geoInfo.xyz);");
                    break;
                case PrimitiveTypes.Torus:
                    map.AppendLine("sdTorus(pos.xyz, geoInfo.xy);");
                    break;
                case PrimitiveTypes.Cylinder:
                    map.AppendLine("sdCylinder(pos.xyz, geoInfo.x, geoInfo.y);");
                    break;
                case PrimitiveTypes.Tetrahedron:
                    map.AppendLine("sdTetra(pos.xyz);");
                    break;
                case PrimitiveTypes.Mandelbulb:
                    map.AppendLine("sdMandelbulb(pos.xyz, geoInfo.xy);");
                    break;
                default:
                    map.AppendLine("0.0;");
                    Debug.LogError("Shader Parse: Unkown Primitive Type!");
                    break;
            }

            // Store distance into distance buffer
            map.AppendLine("\tdistBuffer[" + primIndex + "] = " + obj + ";");
            map.AppendLine();

            // Determine combining operation
            if (!prim.CSGNode)
            {
                determineCombineOp(ref map, prim, null, primIndex);
            }
            else
                map.AppendLine();

            //map.AppendLine();
            ++primIndex;
        }
    }

    private void parseCSG(ref StringBuilder map, CSG csg, ref uint primIndex, ref uint csgIndex)
    {
        // Base case: Both nodes are primitives.
        if (csg.AllPrimNodes)
        {
            // Parse both nodes.
            parsePrimitive(ref map, csg.FirstNode as RMPrimitive, ref primIndex);
            parsePrimitive(ref map, csg.SecondNode as RMPrimitive, ref primIndex, true);

            // Parse this CSG.
            determineCSGNodeCombineOp(ref map, csg, csgIndex);
            //map.AppendLine("\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Only first node is a primitive.
        else if (csg.IsFirstPrim)
        {
            // Recurse through second node (Must be a CSG).
            parseCSG(ref map, csg.SecondNode as CSG, ref primIndex, ref csgIndex);

            // Parse first node.
            parsePrimitive(ref map, csg.FirstNode as RMPrimitive, ref primIndex);

            // Parse this CSG.
            map.AppendLine("\tobj2 = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();
            determineCSGNodeCombineOp(ref map, csg, csgIndex);
            //map.AppendLine("\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Only second node is a primitive.
        else if (csg.IsSecondPrim)
        {
            // Recurse through first node (Must be a csg).
            parseCSG(ref map, csg.FirstNode as CSG, ref primIndex, ref csgIndex);

            map.AppendLine("\tobj = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();

            // Parse second node.
            parsePrimitive(ref map, csg.SecondNode as RMPrimitive, ref primIndex, true);

            // Parse this CSG.
            determineCSGNodeCombineOp(ref map, csg, csgIndex);
            //map.AppendLine("\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Both nodes are CSGs.
        else
        {
            // Recurse through first node.
            parseCSG(ref map, csg.FirstNode as CSG, ref primIndex, ref csgIndex);

            uint firstNodeIndex = (csgIndex - 1);

            // Recurse through second node.
            parseCSG(ref map, csg.SecondNode as CSG, ref primIndex, ref csgIndex);

            map.AppendLine("\tobj = storedCSGs[" + firstNodeIndex + "];");
            map.AppendLine();
            map.AppendLine("\tobj2 = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();

            // Parse this CSG.
            determineCSGNodeCombineOp(ref map, csg, csgIndex);
            //map.AppendLine("\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
    }

    private void determineCSGNodeCombineOp(ref StringBuilder map, CSG csg, uint csgIndex)
    {
        string result = "\tstoredCSGs[" + csgIndex + "]";

        switch (csg.NodeCombineOpType)
        {
            case NodeCombineOpsTypes.Union:
                map.AppendLine(result + " = opU(obj, obj2);");
                break;
            case NodeCombineOpsTypes.Subtraction:
                map.AppendLine(result + " = opS(obj2, obj);");
                break;
            case NodeCombineOpsTypes.Intersection:
                map.AppendLine(result + " = opI(obj, obj2);");
                break;
            case NodeCombineOpsTypes.SmoothUnion:
                map.AppendLine(result + " = opSmoothUnion(obj, obj2, _combineOpsCSGs[" + csgIndex + "].y);");
                break;
            case NodeCombineOpsTypes.SmoothSubtraction:
                map.AppendLine(result + " = opSmoothSub(obj2, obj, _combineOpsCSGs[" + csgIndex + "].y);");
                break;
            case NodeCombineOpsTypes.SmoothIntersection:
                map.AppendLine(result + " = opSmoothInt(obj, obj2, _combineOpsCSGs[" + csgIndex + "].y);");
                break;
            case NodeCombineOpsTypes.Lerp:
                map.AppendLine("\tcsg = lerp(obj, obj2, _combineOpsCSGs[" + csgIndex + "].y);");
                map.AppendLine(result + " = csg;");
                break;
            default:
                break;
        }
    }

    private void determineCombineOp(ref StringBuilder map, RMPrimitive prim, CSG csg, uint index)
    {
        CombineOpsTypes combineOpType;
        string obj = "obj";
        string combineOps = "_combineOps" + "[" + index + "].y);";

        if (prim)
            combineOpType = prim.CombineOpType;
        else
        {
            combineOpType = csg.CombineOpType;
            obj = "storedCSGs[" + index + "]";
            combineOps = "_combineOpsCSGs" + "[" + index + "].w);";
        }

        switch (combineOpType)
        {
            case CombineOpsTypes.Union:
                map.AppendLine("\tscene = opU(scene, " + obj + ");");
                break;
            case CombineOpsTypes.Subtraction:
                map.AppendLine("\tscene = opS(" + obj + ", scene);");
                break;
            case CombineOpsTypes.Intersection:
                map.AppendLine("\tscene = opI(scene, " + obj + ");");
                break;
            case CombineOpsTypes.SmoothUnion:
                map.AppendLine("\tscene = opSmoothUnion(scene, " + obj + ", " + combineOps);
                break;
            case CombineOpsTypes.SmoothSubtraction:
                map.AppendLine("\tscene = opSmoothSub(" + obj + ", scene, " + combineOps);
                break;
            case CombineOpsTypes.SmoothIntersection:
                map.AppendLine("\tscene = opSmoothInt(scene, " + obj + ", " + combineOps);
                break;
            case CombineOpsTypes.AbsUnion:
                map.AppendLine("\tscene = opUAbs(scene, " + obj + ");");
                break;
            default:
                map.AppendLine("\tscene = opU(scene, " + obj + ");");
                Debug.LogError("Shader Parse: Unkown Combining Operation!");
                break;
        }
    }

    private void parseReflection(ref StringBuilder file)
    {
        file.AppendLine("\t\t// Distance field reflection.");
        file.AppendLine("\t\tfloat quality;");
        file.AppendLine("\t\tfloat4 refl = distField.reflInfo;");
        file.AppendLine("\t\tfloat prevRefl = 0;");

        float quality;
        string reflComp = "";

        for (uint i = 0; i < _rayMarcher.ReflectionCount; ++i)
        {
            quality = (i + 1) * 2;

            switch (i)
            {
                case 0:
                    reflComp = ".x";
                    quality = 0.5f;
                    break;
                case 1:
                    reflComp = ".y";
                    quality = 0.25f;
                    break;
                case 2:
                    reflComp = ".z";
                    quality = 0.125f;
                    break;
                default:
                    Debug.LogError("Shader Parse: Broke Max Reflection Limit!");
                    break;
            }

            file.AppendLine();
            file.AppendLine("\t\tquality = " + quality + ";");
            file.AppendLine("\t\trayDir = normalize(reflect(rayDir, normal));");
            file.AppendLine("\t\trayOrigin = p + (rayDir * 0.01);");
            if (i > 0)
            {
                file.AppendLine("\t\tprevRefl = distField.reflInfo.x;");
                file.AppendLine("\t\trayHit = raymarch(rayOrigin, rayDir, _maxDrawDist, (_maxSteps * refl" + reflComp + " * prevRefl) * quality, _maxDrawDist * quality, p, distField);");
            }
            else
                file.AppendLine("\t\trayHit = raymarch(rayOrigin, rayDir, _maxDrawDist, (_maxSteps * refl" + reflComp + ") * quality, _maxDrawDist * quality, p, distField);");
            file.AppendLine();
            file.AppendLine("\t\tif (rayHit)");
            file.AppendLine("\t\t{");
            file.AppendLine("\t\t\tnormal = calcNormal(p);");
            file.AppendLine("\t\t\tadd += float4(calcLighting(p, normal, distField).rgb, 0.0) * refl.w * ratio.x;//_reflectionIntensity;");
            file.AppendLine("\t\t}");
        }

        file.AppendLine("\t\t// Skybox reflection.");
        file.AppendLine("\t\t//add += float4(texCUBE(_skybox, ogNormal).rgb * _envReflIntensity * _reflectionIntensity, 0.0) * (1.0 - rayHit) * refl.x * prevRefl;");
    }


    // ********* Material parsing *********
    private void buildMapMat(ref StringBuilder map)
    {
        map.AppendLine("\trmPixel scene;");
        map.AppendLine("\tscene.dist = _maxDrawDist;");
        map.AppendLine("\tscene.colour = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tscene.reflInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tscene.refractInfo = float2(0.0, 1.0);");
        map.AppendLine("\tscene.texID = 0;");
        map.AppendLine();
        map.AppendLine("\trmPixel obj;");
        map.AppendLine("\tobj.colour = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tobj.reflInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tobj.refractInfo = float2(0.0, 1.0);");
        map.AppendLine("\tobj.texID = 0;");
        map.AppendLine();
        map.AppendLine("\trmPixel obj2;");
        map.AppendLine("\tobj2.colour = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tobj2.reflInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tobj2.refractInfo = float2(0.0, 1.0);");
        map.AppendLine("\tobj2.texID = 0;");
        map.AppendLine();
        map.AppendLine("\trmPixel csg;");
        map.AppendLine("\tcsg.colour = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tcsg.reflInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\tcsg.refractInfo = float2(0.0, 1.0);");
        map.AppendLine("\tcsg.texID = 0;");
        map.AppendLine("\trmPixel storedCSGs[MAX_CSG_CHILDREN];");
        map.AppendLine();
        map.AppendLine("\tfloat reflWeight;");

        uint primIndex = 0;
        uint csgIndex = 0;

        List<RMPrimitive> prims = _rmMemoryManager.RM_Prims;
        List<CSG> csgs = _rmMemoryManager.CSGs;

        List<RMObj> objs = new List<RMObj>(prims.Count + csgs.Count);
        objs.AddRange(prims);
        objs.AddRange(csgs);

        objs.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));

        RMPrimitive prim;
        CSG csg;
        foreach (RMObj obj in objs)
        {
            // Primitive
            if (obj.IsPrim)
            {
                prim = obj as RMPrimitive;

                // Skip any primitives belonging to a csg, as they will be rendered recursively by thier respective csgs.
                if (prim.CSGNode)
                    continue;

                map.AppendLine("\t// ######### " + prim.gameObject.name + " #########");
                parsePrimitiveMat(ref map, prim, ref primIndex);
                map.AppendLine("\t// ######### " + prim.gameObject.name + " #########");
                map.AppendLine();
            }
            // CSG
            else
            {
                csg = obj as CSG;

                // Skip any non-root CSGs, as they will be rendered recursively by thier parents.
                // Skip any CSGs which don't have two nodes.
                if (!csg.IsRoot || !csg.IsValid)
                    continue;

                map.AppendLine("\t// ######### " + csg.gameObject.name + " #########");

                parseCSGMat(ref map, csg, ref primIndex, ref csgIndex);

                determineCombineOpMat(ref map, null, csg, csgIndex - 1);
                map.AppendLine("\t// ######### " + csg.gameObject.name + " #########");
                map.AppendLine();
            }
        }

        map.AppendLine("\treturn scene;");
    }

    private void parsePrimitiveMat(ref StringBuilder map, RMPrimitive prim, ref uint primIndex, bool csgNodeTwo = false)
    {
        string obj = "obj";
        if (csgNodeTwo)
            obj = "obj2";

        if (!prim.Static)
        {
            // Retrieve distance and other material information
            map.AppendLine("\t" + obj + ".dist = distBuffer[" + primIndex + "];");
            map.AppendLine("\t" + obj + ".colour = _rm_colours[" + primIndex + "];");
            map.AppendLine("\t" + obj + ".reflInfo = _reflInfo[" + primIndex + "];");
            map.AppendLine("\t" + obj + ".refractInfo = _refractInfo[" + primIndex + "];");

            // Determine combining operation
            if (!prim.CSGNode)
            {
                determineCombineOpMat(ref map, prim, null, primIndex);
            }
            else
                map.AppendLine();

            ++primIndex;
        }
        else
        {
            Vector4 info;

            // Retrieve distance and other material information
            map.AppendLine("\t" + obj + ".dist = distBuffer[" + primIndex + "];");

            info = prim.Colour;
            map.AppendLine("\t" + obj + ".colour = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

            info = prim.ReflectionInfo;
            map.AppendLine("\t" + obj + ".reflInfo = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

            info = prim.RefractionInfo;
            map.AppendLine("\t" + obj + ".refractInfo = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

            // Determine combining operation
            if (!prim.CSGNode)
            {
                determineCombineOpMat(ref map, prim, null, primIndex);
            }
            else
                map.AppendLine();

            ++primIndex;
        }
    }

    private void determineCombineOpMat(ref StringBuilder map, RMPrimitive prim, CSG csg, uint index)
    {
        CombineOpsTypes combineOpType;
        string obj = "obj";
        string combineOps = "_combineOps" + "[" + index + "].y);";

        if (prim)
            combineOpType = prim.CombineOpType;
        else
        {
            combineOpType = csg.CombineOpType;
            obj = "storedCSGs[" + index + "]";
            combineOps = "_combineOpsCSGs" + "[" + index + "].w);";
        }

        switch (combineOpType)
        {
            case CombineOpsTypes.Union:
                map.AppendLine("\tscene = opUMat(scene, " + obj + ");");
                break;
            case CombineOpsTypes.Subtraction:
                map.AppendLine("\tscene = opSMat(" + obj + ", scene);");
                break;
            case CombineOpsTypes.Intersection:
                map.AppendLine("\tscene = opIMat(scene, " + obj + ");");
                break;
            case CombineOpsTypes.SmoothUnion:
                map.AppendLine("\tscene = opSmoothUnionMat(scene, " + obj + ", " + combineOps);
                break;
            case CombineOpsTypes.SmoothSubtraction:
                map.AppendLine("\tscene = opSmoothSubMat(" + obj + ", scene, " + combineOps);
                break;
            case CombineOpsTypes.SmoothIntersection:
                map.AppendLine("\tscene = opSmoothIntMat(scene, " + obj + ", " + combineOps);
                break;
            case CombineOpsTypes.AbsUnion:
                map.AppendLine("\tscene = opUAbsMat(scene, " + obj + ");");
                break;
            default:
                map.AppendLine("\tscene = opUMat(scene, " + obj + ");");
                Debug.LogError("Shader Parse: Unkown Combining Operation!");
                break;
        }
    }

    private void parseCSGMat(ref StringBuilder map, CSG csg, ref uint primIndex, ref uint csgIndex)
    {
        // Base case: Both nodes are primitives.
        if (csg.AllPrimNodes)
        {
            // Parse both nodes.
            parsePrimitiveMat(ref map, csg.FirstNode as RMPrimitive, ref primIndex);
            parsePrimitiveMat(ref map, csg.SecondNode as RMPrimitive, ref primIndex, true);

            // Parse this CSG.
            determineCSGCombineOpMat(ref map, csg, csgIndex);
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Only first node is a primitive.
        else if (csg.IsFirstPrim)
        {
            // Recurse through second node (Must be a CSG).
            parseCSGMat(ref map, csg.SecondNode as CSG, ref primIndex, ref csgIndex);

            // Parse first node.
            parsePrimitiveMat(ref map, csg.FirstNode as RMPrimitive, ref primIndex);

            // Parse this CSG.
            map.AppendLine("\tobj2 = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();
            determineCSGCombineOpMat(ref map, csg, csgIndex);
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Only second node is a primitive.
        else if (csg.IsSecondPrim)
        {
            // Recurse through first node (Must be a csg).
            parseCSGMat(ref map, csg.FirstNode as CSG, ref primIndex, ref csgIndex);

            map.AppendLine("\tobj = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();

            // Parse second node.
            parsePrimitiveMat(ref map, csg.SecondNode as RMPrimitive, ref primIndex, true);

            // Parse this CSG.
            determineCSGCombineOpMat(ref map, csg, csgIndex);
            //map.AppendLine("\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Both nodes are CSGs.
        else
        {
            // Recurse through first node.
            parseCSGMat(ref map, csg.FirstNode as CSG, ref primIndex, ref csgIndex);

            uint firstNodeIndex = (csgIndex - 1);

            // Recurse through second node.
            parseCSGMat(ref map, csg.SecondNode as CSG, ref primIndex, ref csgIndex);

            map.AppendLine("\tobj = storedCSGs[" + firstNodeIndex + "];");
            map.AppendLine();
            map.AppendLine("\tobj2 = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();

            // Parse this CSG.
            determineCSGCombineOpMat(ref map, csg, csgIndex);
            //map.AppendLine("\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
    }

    private void determineCSGCombineOpMat(ref StringBuilder map, CSG csg, uint csgIndex)
    {
        string result = "\tstoredCSGs[" + csgIndex + "]";

        switch (csg.NodeCombineOpType)
        {
            case NodeCombineOpsTypes.Union:
                map.AppendLine(result + " = opUMat(obj, obj2);");
                break;
            case NodeCombineOpsTypes.Subtraction:
                map.AppendLine(result + " = opSMat(obj2, obj);");
                break;
            case NodeCombineOpsTypes.Intersection:
                map.AppendLine(result + " = opIMat(obj, obj2);");
                break;
            case NodeCombineOpsTypes.SmoothUnion:
                map.AppendLine(result + " = opSmoothUnionMat(obj, obj2, _combineOpsCSGs[" + csgIndex + "].y);");
                break;
            case NodeCombineOpsTypes.SmoothSubtraction:
                map.AppendLine(result + " = opSmoothSubMat(obj2, obj, _combineOpsCSGs[" + csgIndex + "].y);");
                break;
            case NodeCombineOpsTypes.SmoothIntersection:
                map.AppendLine(result + " = opSmoothIntMat(obj, obj2, _combineOpsCSGs[" + csgIndex + "].y);");
                break;
            case NodeCombineOpsTypes.Lerp:
                map.AppendLine("\tcsg.dist = lerp(obj.dist, obj2.dist, _combineOpsCSGs[" + csgIndex + "].y);");
                map.AppendLine("\tcsg.colour = lerp(obj.colour, obj2.colour, _combineOpsCSGs[" + csgIndex + "].y);");
                map.AppendLine("\treflWeight = step(0.5, _combineOpsCSGs[7].y);");
                map.AppendLine("\tcsg.reflInfo = (1.0 - reflWeight) * obj.reflInfo + reflWeight * obj2.reflInfo;");
                map.AppendLine(result + " = csg;");
                break;
            default:
                break;
        }
    }

    [MenuItem("My Commands/Special Command _F6")]
    static void SpecialCommand()
    {
        Camera.main.GetComponent<ShaderBuilder>().build();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ShaderBuilder))]
public class ShaderBuilderEditor : Editor
{
    SerializedProperty _path;
    SerializedProperty _name;
    SerializedProperty _templateHLSLPath;
    SerializedProperty _templateShaderPath;


    private void OnEnable()
    {
        _path = serializedObject.FindProperty("_path");
        _name = serializedObject.FindProperty("_name");
        _templateHLSLPath = serializedObject.FindProperty("_templateHLSLPath");
        _templateShaderPath = serializedObject.FindProperty("_templateShaderPath");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        var shaderBuilder = target as ShaderBuilder;

        serializedObject.Update();

        EditorGUILayout.PropertyField(_path);
        EditorGUILayout.PropertyField(_name);
        EditorGUILayout.PropertyField(_templateHLSLPath);
        EditorGUILayout.PropertyField(_templateShaderPath);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Build"))
        {
            shaderBuilder.build();
        }
    }

    //private void OnSceneGUI()
    //{
    //    var shaderBuilder = target as ShaderBuilder;

    //    Event e = Event.current;
    //    switch (e.type)
    //    {
    //        case EventType.KeyDown:
    //            if (e.keyCode == KeyCode.F6)
    //                shaderBuilder.build();
    //            break;
    //    }
    //}
}
#endif
