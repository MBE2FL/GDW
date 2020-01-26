﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Text;


public enum BuildMode
{
    BuildSelected,
    BuildAll
}

public enum BuildType
{
    Rendering,
    Collision,
    MarchingCubes
}

[ExecuteInEditMode]
[AddComponentMenu("Ray Marching/Shader Builder")]
[DisallowMultipleComponent]
public class ShaderBuilder : MonoBehaviour
{
    [SerializeField]
    private string _path;
    [SerializeField]
    private string _shaderTemplatePath;
    [SerializeField]
    Shader shaderTemplate;
    [HideInInspector]
    private RayMarcher _rayMarcher;
    [SerializeField]
    private RayMarchShader _currentShader;
    [SerializeField]
    BuildMode _buildMode = BuildMode.BuildSelected;
    [SerializeField]
    BuildType _buildType = BuildType.Rendering;


    public RayMarcher GetRayMarcher
    {
        get
        {
            return _rayMarcher;
        }
    }

    public RayMarchShader CurrentShader
    {
        get
        {
            return _currentShader;
        }
        set
        {
            _currentShader = value;
        }
    }


    private void Awake()
    {
        //_path = Application.dataPath + "/Graphics Pipeline/Shaders/";
        //_shaderTemplatePath = Application.dataPath + "/Graphics Pipeline/Shaders/RayMarchTemplate.shader";
        _shaderTemplatePath = AssetDatabase.GetAssetPath(shaderTemplate);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Retrieve a reference to the ray marcher from the main camera.
        //_rayMarcher = Camera.main.GetComponent<RayMarcher>();
        _rayMarcher = RayMarcher.Instance;
    }

    public void build()
    {
        Debug.Log("Building shader.");

        StringBuilder file = new StringBuilder();

        string shaderName = _currentShader.ShaderName;

        foreach (string line in File.ReadAllLines(_shaderTemplatePath))
        {
            // Replace insert statement.
            if (line.Contains("// <Insert Shader Name Here>"))
            {
                file.AppendLine("Shader \"RayMarch/" + shaderName + "\"");
            }
            else if (line.Contains("// <Insert Maps Here>"))
            {
                file.AppendLine("\tfloat cheapMap(float3 p)");
                file.AppendLine("\t{");
                buildCheapMap(ref file);
                file.AppendLine("\t}");
                file.AppendLine();


                file.AppendLine("\tfloat map(float3 p)");
                file.AppendLine("\t{");
                buildMap(ref file);
                file.AppendLine("\t}");
                file.AppendLine();


                file.AppendLine("\trmPixel mapMat()");
                file.AppendLine("\t{");
                buildMapMat(ref file);
                file.AppendLine("\t}");
                file.AppendLine();
            }
            else if (line.Contains("// <Insert Reflection Here>"))
            {
                //parseReflection(ref file);
            }
            // Copy line from template.
            else
            {
                file.AppendLine(line);
            }
        }


        // Write new shader.
        using (StreamWriter shader = new StreamWriter(File.Create(_path + "/" + shaderName + ".shader")))
        {
            shader.Write(file.ToString());
        }


        file.Clear();


#if UNITY_EDITOR
        AssetDatabase.Refresh();
        //AssetDatabase.ImportAsset("Assets/Graphics Pipeline/Shaders/Resources/" + _name + ".shader");
#endif
    }

    public void buildCheapMap(ref StringBuilder map)
    {
        map.AppendLine("\t\tfloat scene = _maxDrawDist;");
        map.AppendLine();
        map.AppendLine("\t\tfloat4 pos = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tfloat4 geoInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tfloat radius = 0.0;");
        map.AppendLine();
        map.AppendLine("\t\tfloat obj;");
        map.AppendLine();

        uint primIndex = 0;
        uint csgIndex = 0;

        List<RMObj> objs = _currentShader.RenderList;

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

                //parseCheapObj(ref map, prim, ref primIndex, ref csgIndex);
            }
            // CSG
            else
            {
                csg = obj as CSG;

                // Skip any non-root CSGs, as they will be rendered recursively by thier parents.
                // Skip any CSGs which don't have two nodes.
                if (!csg.IsRoot || !csg.IsValid)
                    continue;

                //parseCheapObj(ref map, csg, ref primIndex, ref csgIndex);

                //determineCombineOp(ref map, null, csg, csgIndex - 1);
            }

            map.AppendLine("\t\t// ######### " + obj.gameObject.name + " #########");
            parseCheapObj(ref map, obj, ref primIndex, ref csgIndex);
            map.AppendLine("\t\t// ######### " + obj.gameObject.name + " #########");
            map.AppendLine();
        }

        map.AppendLine("\t\treturn scene;");
    }

    private void buildMap(ref StringBuilder map)
    {
        map.AppendLine("\t\tfloat scene = _maxDrawDist;");
        map.AppendLine();
        map.AppendLine("\t\tfloat4 pos = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tfloat4 geoInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine();
        map.AppendLine("\t\tfloat obj;");
        map.AppendLine("\t\tfloat obj2;");
        map.AppendLine();
        map.AppendLine("\t\tfloat csg;");
        map.AppendLine("\t\tfloat storedCSGs[MAX_CSG_CHILDREN];");
        map.AppendLine();
        map.AppendLine("\t\tfloat3 cell = float3(0.0, 0.0, 0.0);");
        map.AppendLine();

        uint primIndex = 0;
        uint csgIndex = 0;
        uint altIndex = 0;

        //List<RMPrimitive> prims = _rmMemoryManager.RM_Prims;
        //List<CSG> csgs = _rmMemoryManager.CSGs;

        //List<RMObj> objs = new List<RMObj>(prims.Count + csgs.Count);
        //objs.AddRange(prims);
        //objs.AddRange(csgs);

        //objs.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));

        List<RMObj> objs = _currentShader.RenderList;

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

                map.AppendLine("\t\t// ######### " + prim.gameObject.name + " #########");
                parsePrimitive(ref map, prim, ref primIndex, ref altIndex);
                map.AppendLine("\t\t// ######### " + prim.gameObject.name + " #########");
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

                map.AppendLine("\t\t// ######### " + csg.gameObject.name + " #########");

                parseCSG(ref map, csg, ref primIndex, ref csgIndex, ref altIndex);

                determineCombineOp(ref map, null, csg, csgIndex - 1);
                map.AppendLine("\t\t// ######### " + csg.gameObject.name + " #########");
                map.AppendLine();
            }
        }

        map.AppendLine("\t\treturn scene;");
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



    private void parseCheapObj(ref StringBuilder cheapMap, RMObj obj, ref uint primIndex, ref uint csgIndex)
    {
        if (!obj.Static)
        {
            // Determine position and geometric information
            cheapMap.AppendLine("\t\tpos = mul(_invModelMats[" + primIndex + "], float4(p, 1.0));");
            cheapMap.AppendLine("\t\tgeoInfo = _boundGeoInfo[" + primIndex + "];");


            cheapMap.Append("\t\tobj = ");

            // Determine primitive type
            switch (obj.BoundShape)
            {
                case BoundingShapes.Sphere:
                    cheapMap.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                    break;
                case BoundingShapes.Box:
                    cheapMap.AppendLine("sdBox(pos.xyz, geoInfo.xyz);");
                    break;
                default:
                    Debug.LogError("Obj's bound shape, in cheap map, could not be parsed.");
                    break;
            }

            cheapMap.AppendLine();

            // Determine combining operation
            determineCheapCombineOp(ref cheapMap, obj, primIndex);


            if (obj.IsPrim)
                ++primIndex;
            else
                parseCheapCSG(ref cheapMap, obj as CSG, ref primIndex, ref csgIndex);
        }
        else
        {
            Matrix4x4 mat;
            Vector4 info;

            // Determine position and geometric information
            mat = obj.transform.localToWorldMatrix.inverse;
            cheapMap.AppendLine("\t\tpos = mul(float4x4(" + mat.m00 + ", " + mat.m01 + ", " + mat.m02 + ", " + mat.m03 + ", "
                                               + mat.m10 + ", " + mat.m11 + ", " + mat.m12 + ", " + mat.m13 + ", "
                                               + mat.m20 + ", " + mat.m21 + ", " + mat.m22 + ", " + mat.m23 + ", "
                                               + mat.m30 + ", " + mat.m31 + ", " + mat.m32 + ", " + mat.m33 + "), float4(p, 1.0));");

            info = obj.BoundGeoInfo;
            cheapMap.AppendLine("\t\tgeoInfo = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

            // Determine primitive type
            switch (obj.BoundShape)
            {
                case BoundingShapes.Sphere:
                    cheapMap.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                    break;
                case BoundingShapes.Box:
                    cheapMap.AppendLine("sdBox(pos.xyz, geoInfo.xyz);");
                    break;
                default:
                    Debug.LogError("Obj's bound shape, in cheap map, could not be parsed.");
                    break;
            }


            cheapMap.AppendLine();

            // Determine combining operation
            determineCheapCombineOp(ref cheapMap, obj, primIndex);

            //map.AppendLine();
            ++primIndex;
        }
    }

    private void parseCheapCSG(ref StringBuilder cheapMap, CSG csg, ref uint primIndex, ref uint csgIndex)
    {
        // Base case: Both nodes are primitives.
        if (csg.AllPrimNodes)
        {
            ++primIndex;
            ++primIndex;
            ++csgIndex;
            return;
        }
        // Only first node is a primitive.
        else if (csg.IsFirstPrim)
        {
            ++primIndex;
            ++csgIndex;
            return;
        }
        // Only second node is a primitive.
        else if (csg.IsSecondPrim)
        {
            ++primIndex;
            ++csgIndex;
            return;
        }
        // Both nodes are CSGs.
        else
        {
            // Recurse through first node.
            parseCheapCSG(ref cheapMap, csg.FirstNode as CSG, ref primIndex, ref csgIndex);

            // Recurse through second node.
            parseCheapCSG(ref cheapMap, csg.SecondNode as CSG, ref primIndex, ref csgIndex);

            // Parse this CSG.
            cheapMap.AppendLine();
            ++csgIndex;
            return;
        }
    }

    private void determineCheapCombineOp(ref StringBuilder cheapMap, RMObj obj, uint index)
    {
        CombineOpsTypes combineOpType = obj.CombineOpType;
        string combineOps = "_combineOps" + "[" + index + "].y);";

        if (!obj.IsPrim)
            combineOps = "_combineOpsCSGs" + "[" + index + "].w);";

        switch (combineOpType)
        {
            case CombineOpsTypes.Union:
                cheapMap.AppendLine("\t\tscene = opU(scene, obj);");
                break;
            case CombineOpsTypes.SmoothUnion:
                cheapMap.AppendLine("\t\tscene = opSmoothUnion(scene, obj, " + combineOps);
                break;
            case CombineOpsTypes.SmoothSubtraction:
                cheapMap.AppendLine("\t\tscene = opSmoothUnion(scene, obj, " + combineOps);
                break;
            case CombineOpsTypes.SmoothIntersection:
                cheapMap.AppendLine("\t\tscene = opSmoothUnion(scene, obj, " + combineOps);
                break;
            default:
                cheapMap.AppendLine("\t\tscene = opU(scene, obj);");
                break;
        }
    }



    private void parsePrimitive(ref StringBuilder map, RMPrimitive prim, ref uint primIndex, ref uint altIndex, bool csgNodeTwo = false)
    {
        if (!prim.Static)
        {
            // Determine position and geometric information
            map.AppendLine("\t\tpos = mul(_invModelMats[" + primIndex + "], float4(p, 1.0));");
            map.AppendLine("\t\tgeoInfo = _primitiveGeoInfo[" + primIndex + "];");

            parseAlterationTypes(ref map, prim.Alterations, ref altIndex);

            string obj = "obj";
            if (csgNodeTwo)
                obj = "obj2";

            map.Append("\t\t" + obj + " = ");

            // Determine primitive type
            parsePrimitiveType(ref map, prim.PrimitiveType);

            // Store distance into distance buffer
            parseAlterationTypes(ref map, prim.Alterations, ref altIndex, false);
            map.AppendLine("\t\tdistBuffer[" + primIndex + "] = " + obj + ";");
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
            map.AppendLine("\t\tpos = mul(float4x4(" + mat.m00 + ", " + mat.m01 + ", " + mat.m02 + ", " + mat.m03 + ", "
                                               + mat.m10 + ", " + mat.m11 + ", " + mat.m12 + ", " + mat.m13 + ", "
                                               + mat.m20 + ", " + mat.m21 + ", " + mat.m22 + ", " + mat.m23 + ", "
                                               + mat.m30 + ", " + mat.m31 + ", " + mat.m32 + ", " + mat.m33 + "), float4(p, 1.0));");

            //map.AppendLine("\tgeoInfo = _primitiveGeoInfo[" + primIndex + "];");
            info = prim.GeoInfo;
            map.AppendLine("\t\tgeoInfo = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

            string obj = "obj";
            if (csgNodeTwo)
                obj = "obj2";

            map.Append("\t\t" + obj + " = ");

            // Determine primitive type
            parsePrimitiveType(ref map, prim.PrimitiveType);

            // Store distance into distance buffer
            map.AppendLine("\t\tdistBuffer[" + primIndex + "] = " + obj + ";");
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

    private void parsePrimitiveType(ref StringBuilder map, PrimitiveTypes type)
    {
        switch (type)
        {
            case PrimitiveTypes.Sphere:
                map.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                break;
            case PrimitiveTypes.Box:
                map.AppendLine("sdBox(pos.xyz, geoInfo.xyz);");
                break;
            case PrimitiveTypes.RoundBox:
                map.AppendLine("sdRoundBox(pos.xyz, geoInfo.xyz, geoInfo.w);");
                break;
            case PrimitiveTypes.Torus:
                map.AppendLine("sdTorus(pos.xyz, geoInfo.xy);");
                break;
            case PrimitiveTypes.CappedTorus:
                map.AppendLine("sdCappedTorus(pos.xyz, geoInfo.xy, geoInfo.z, geoInfo.z);");
                break;
            case PrimitiveTypes.Link:
                map.AppendLine("sdLink(pos.xyz, geoInfo.x, geoInfo.y, geoInfo.z);");
                break;
            case PrimitiveTypes.Cylinder:
                map.AppendLine("sdCylinder(pos.xyz, geoInfo.x, geoInfo.y);");
                break;
            case PrimitiveTypes.CappedCylinder:
                map.AppendLine("sdCappedCylinder(pos.xyz, geoInfo.x, geoInfo.y);");
                break;
            case PrimitiveTypes.CappedCylinderSlower:
                map.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                Debug.LogError("Capped Cylinder Slower is not yet implemented!");
                break;
            case PrimitiveTypes.RoundedCylinder:
                map.AppendLine("sdRoundedCylinder(pos.xyz, geoInfo.x, geoInfo.y, geoInfo.z);");
                break;
            case PrimitiveTypes.Cone:
                map.AppendLine("sdCone(pos.xyz, geoInfo.xy);");
                break;
            case PrimitiveTypes.CappedCone:
                map.AppendLine("sdCappedCone(pos.xyz, geoInfo.x, geoInfo.y, geoInfo.z);");
                break;
            case PrimitiveTypes.RoundCone:
                map.AppendLine("sdRoundCone(pos.xyz, geoInfo.x, geoInfo.y, geoInfo.z);");
                break;
            case PrimitiveTypes.Plane:
                map.AppendLine("sdPlane(pos.xyz, geoInfo);");
                break;
            case PrimitiveTypes.HexagonalPrism:
                map.AppendLine("sdHexagonalPrism(pos.xyz, geoInfo.xy);");
                break;
            case PrimitiveTypes.TriangularPrism:
                map.AppendLine("sdTriangularPrism(pos.xyz, geoInfo.xy);");
                break;
            case PrimitiveTypes.Capsule:
                map.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                Debug.LogError("Capsule is not yet implemented!");
                break;
            case PrimitiveTypes.VerticalCapsule:
                map.AppendLine("sdVerticalCapsule(pos.xyz, geoInfo.x, geoInfo.y);");
                break;
            case PrimitiveTypes.SolidAngle:
                map.AppendLine("sdSolidAngle(pos.xyz, geoInfo.xy, geoInfo.z);");
                break;
            case PrimitiveTypes.Ellipsoid:
                map.AppendLine("sdEllipsoid(pos.xyz, geoInfo.xyz);");
                break;
            case PrimitiveTypes.Octahedron:
                map.AppendLine("sdOctahedron(pos.xyz, geoInfo.x);");
                break;
            case PrimitiveTypes.OctahedronBound:
                map.AppendLine("sdOctahedronBound(pos.xyz, geoInfo.x);");
                break;
            case PrimitiveTypes.Triangle:
                map.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                Debug.LogError("Triangle is not yet implemented!");
                break;
            case PrimitiveTypes.Quad:
                map.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                Debug.LogError("Quad is not yet implemented!");
                break;
            case PrimitiveTypes.Tetrahedron:
                map.AppendLine("sdTetra(pos.xyz);");
                break;
            case PrimitiveTypes.Mandelbulb:
                map.AppendLine("sdMandelbulb(pos.xyz, geoInfo.xy);");
                break;
            default:
                map.AppendLine("sdSphere(pos.xyz, geoInfo.x);");
                Debug.LogError("Shader Parse: Unkown Primitive Type!");
                break;
        }
    }

    private void parseAlterationTypes(ref StringBuilder map, List<Alteration> alterations, ref uint altIndex, bool posAlt = true)
    {
        foreach (Alteration alt in alterations)
        {
            //// Skip any non-active alterations.
            //if (!alt.active)
            //    continue;

            if (alt.posAlt && posAlt)
            {
                switch (alt.type)
                {
                    case AlterationTypes.Elongate1D:
                        map.AppendLine("\t\topElongate1D(pos.xyz, _altInfo[" + altIndex + "]);");
                        ++altIndex;
                        break;
                    case AlterationTypes.Elongate:
                        break;
                    case AlterationTypes.SymX:
                        map.AppendLine("\t\topSymX(pos.xyz, _altInfo[" + altIndex + "].x);");
                        ++altIndex;
                        break;
                    case AlterationTypes.SymXZ:
                        map.AppendLine("\t\topSymXZ(pos.xyz, _altInfo[" + altIndex + "].xy);");
                        ++altIndex;
                        break;
                    case AlterationTypes.RepXZ:
                        map.AppendLine("\t\topRepXZ(pos.xyz, _altInfo[" + altIndex + "].xz, cell.xz);");
                        ++altIndex;
                        break;
                    case AlterationTypes.RepFinite:
                        map.AppendLine("\t\topRepLim(pos.xyz, _altInfo[" + altIndex + "].x, _altInfo[" + altIndex + "].yzw);");
                        ++altIndex;
                        break;
                    case AlterationTypes.Twist:
                        map.AppendLine("\t\topTwist(pos.xyz, _altInfo[" + altIndex + "].x);");
                        ++altIndex;
                        break;
                    case AlterationTypes.Bend:
                        map.AppendLine("\t\topCheapBend(pos.xyz, _altInfo[" + altIndex + "].x);");
                        ++altIndex;
                        break;
                    case AlterationTypes.Custom:
                        map.AppendLine("\t\t" + alt.command);
                        ++altIndex;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (alt.type)
                {
                    case AlterationTypes.Round:
                        map.AppendLine("\t\topRound(obj, _altInfo[" + altIndex + "].x);");
                        ++altIndex;
                        break;
                    case AlterationTypes.Onion:
                        map.AppendLine("\t\topOnion(obj, _altInfo[" + altIndex + "].x);");
                        ++altIndex;
                        break;
                    case AlterationTypes.Displace:
                        map.AppendLine("\t\topDisplace(pos.xyz, obj, _altInfo[" + altIndex + "].xyz);");
                        ++altIndex;
                        break;
                    default:
                        break;
                }
                //++altIndex;
            }
        }
    }

    private void parseCSG(ref StringBuilder map, CSG csg, ref uint primIndex, ref uint csgIndex, ref uint altIndex)
    {
        // Base case: Both nodes are primitives.
        if (csg.AllPrimNodes)
        {
            // Parse both nodes.
            parsePrimitive(ref map, csg.FirstNode as RMPrimitive, ref primIndex, ref altIndex);
            parsePrimitive(ref map, csg.SecondNode as RMPrimitive, ref primIndex, ref altIndex, true);

            // Parse this CSG.
            determineCSGNodeCombineOp(ref map, csg, csgIndex);
            //map.AppendLine("\t\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Only first node is a primitive.
        else if (csg.IsFirstPrim)
        {
            // Recurse through second node (Must be a CSG).
            parseCSG(ref map, csg.SecondNode as CSG, ref primIndex, ref csgIndex, ref altIndex);

            // Parse first node.
            parsePrimitive(ref map, csg.FirstNode as RMPrimitive, ref primIndex, ref altIndex);

            // Parse this CSG.
            map.AppendLine("\t\tobj2 = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();
            determineCSGNodeCombineOp(ref map, csg, csgIndex);
            //map.AppendLine("\t\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Only second node is a primitive.
        else if (csg.IsSecondPrim)
        {
            // Recurse through first node (Must be a csg).
            parseCSG(ref map, csg.FirstNode as CSG, ref primIndex, ref csgIndex, ref altIndex);

            map.AppendLine("\t\tobj = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();

            // Parse second node.
            parsePrimitive(ref map, csg.SecondNode as RMPrimitive, ref primIndex, ref altIndex, true);

            // Parse this CSG.
            determineCSGNodeCombineOp(ref map, csg, csgIndex);
            //map.AppendLine("\t\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
        // Both nodes are CSGs.
        else
        {
            // Recurse through first node.
            parseCSG(ref map, csg.FirstNode as CSG, ref primIndex, ref csgIndex, ref altIndex);

            uint firstNodeIndex = (csgIndex - 1);

            // Recurse through second node.
            parseCSG(ref map, csg.SecondNode as CSG, ref primIndex, ref csgIndex, ref altIndex);

            map.AppendLine("\t\tobj = storedCSGs[" + firstNodeIndex + "];");
            map.AppendLine();
            map.AppendLine("\t\tobj2 = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();

            // Parse this CSG.
            determineCSGNodeCombineOp(ref map, csg, csgIndex);
            //map.AppendLine("\t\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
    }

    private void determineCSGNodeCombineOp(ref StringBuilder map, CSG csg, uint csgIndex)
    {
        string result = "\t\tstoredCSGs[" + csgIndex + "]";

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
                map.AppendLine("\t\tcsg = lerp(obj, obj2, _combineOpsCSGs[" + csgIndex + "].y);");
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
                map.AppendLine("\t\tscene = opU(scene, " + obj + ");");
                break;
            case CombineOpsTypes.Subtraction:
                map.AppendLine("\t\tscene = opS(" + obj + ", scene);");
                break;
            case CombineOpsTypes.Intersection:
                map.AppendLine("\t\tscene = opI(scene, " + obj + ");");
                break;
            case CombineOpsTypes.SmoothUnion:
                map.AppendLine("\t\tscene = opSmoothUnion(scene, " + obj + ", " + combineOps);
                break;
            case CombineOpsTypes.SmoothSubtraction:
                map.AppendLine("\t\tscene = opSmoothSub(" + obj + ", scene, " + combineOps);
                break;
            case CombineOpsTypes.SmoothIntersection:
                map.AppendLine("\t\tscene = opSmoothInt(scene, " + obj + ", " + combineOps);
                break;
            case CombineOpsTypes.AbsUnion:
                map.AppendLine("\t\tscene = opUAbs(scene, " + obj + ");");
                break;
            default:
                map.AppendLine("\t\tscene = opU(scene, " + obj + ");");
                Debug.LogError("Shader Parse: Unkown Combining Operation!");
                break;
        }
    }




    private void parseReflection(ref StringBuilder file)
    {
        file.AppendLine("\t\t\t// Distance field reflection.");
        file.AppendLine("\t\t\tfloat quality;");
        file.AppendLine("\t\t\tfloat4 refl = distField.reflInfo;");
        file.AppendLine("\t\t\tfloat prevRefl = 0;");

        float quality;
        string reflComp = "";

        for (uint i = 0; i < _currentShader.Settings.ReflectionCount; ++i)
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
            file.AppendLine("\t\t\tquality = " + quality + ";");
            file.AppendLine("\t\t\trayDir = normalize(reflect(rayDir, normal));");
            file.AppendLine("\t\t\trayOrigin = p + (rayDir * 0.01);");
            if (i > 0)
            {
                file.AppendLine("\t\t\tprevRefl = distField.reflInfo.x;");
                file.AppendLine("\t\t\trayHit = raymarch(rayOrigin, rayDir, _maxDrawDist, (_maxSteps * refl" + reflComp + " * prevRefl) * quality, _maxDrawDist * quality, p, distField);");
            }
            else
                file.AppendLine("\t\t\trayHit = raymarch(rayOrigin, rayDir, _maxDrawDist, (_maxSteps * refl" + reflComp + ") * quality, _maxDrawDist * quality, p, distField);");
            file.AppendLine();
            file.AppendLine("\t\t\tif (rayHit)");
            file.AppendLine("\t\t\t{");
            file.AppendLine("\t\t\t\tnormal = calcNormal(p);");
            file.AppendLine("\t\t\t\tadd += float4(calcLighting(p, normal, distField).rgb, 0.0) * refl.w * ratio.x;//_reflectionIntensity;");
            file.AppendLine("\t\t\t}");
        }

        file.AppendLine("\t\t\t// Skybox reflection.");
        file.AppendLine("\t\t\t//add += float4(texCUBE(_skybox, ogNormal).rgb * _envReflIntensity * _reflectionIntensity, 0.0) * (1.0 - rayHit) * refl.x * prevRefl;");
    }





    // ********* Material parsing *********
    private void buildMapMat(ref StringBuilder map)
    {
        map.AppendLine("\t\trmPixel scene;");
        map.AppendLine("\t\tscene.dist = _maxDrawDist;");
        map.AppendLine("\t\tscene.colour = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tscene.reflInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tscene.refractInfo = float2(0.0, 1.0);");
        map.AppendLine("\t\tscene.texID = 0;");
        map.AppendLine();
        map.AppendLine("\t\trmPixel obj;");
        map.AppendLine("\t\tobj.colour = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tobj.reflInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tobj.refractInfo = float2(0.0, 1.0);");
        map.AppendLine("\t\tobj.texID = 0;");
        map.AppendLine();
        map.AppendLine("\t\trmPixel obj2;");
        map.AppendLine("\t\tobj2.colour = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tobj2.reflInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tobj2.refractInfo = float2(0.0, 1.0);");
        map.AppendLine("\t\tobj2.texID = 0;");
        map.AppendLine();
        map.AppendLine("\t\trmPixel csg;");
        map.AppendLine("\t\tcsg.colour = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tcsg.reflInfo = float4(0.0, 0.0, 0.0, 0.0);");
        map.AppendLine("\t\tcsg.refractInfo = float2(0.0, 1.0);");
        map.AppendLine("\t\tcsg.texID = 0;");
        map.AppendLine("\t\trmPixel storedCSGs[MAX_CSG_CHILDREN];");
        map.AppendLine();
        map.AppendLine("\t\tfloat reflWeight;");

        uint primIndex = 0;
        uint csgIndex = 0;
        

        List<RMObj> objs = _currentShader.RenderList;


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

                map.AppendLine("\t\t// ######### " + prim.gameObject.name + " #########");
                parsePrimitiveMat(ref map, prim, ref primIndex);
                map.AppendLine("\t\t// ######### " + prim.gameObject.name + " #########");
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

                map.AppendLine("\t\t// ######### " + csg.gameObject.name + " #########");

                parseCSGMat(ref map, csg, ref primIndex, ref csgIndex);

                determineCombineOpMat(ref map, null, csg, csgIndex - 1);
                map.AppendLine("\t\t// ######### " + csg.gameObject.name + " #########");
                map.AppendLine();
            }
        }

        map.AppendLine("\t\treturn scene;");
    }

    private void parsePrimitiveMat(ref StringBuilder map, RMPrimitive prim, ref uint primIndex, bool csgNodeTwo = false)
    {
        string obj = "obj";
        if (csgNodeTwo)
            obj = "obj2";

        if (!prim.Static)
        {
            // Retrieve distance and other material information
            map.AppendLine("\t\t" + obj + ".dist = distBuffer[" + primIndex + "];");
            map.AppendLine("\t\t" + obj + ".colour = _rm_colours[" + primIndex + "];");
            map.AppendLine("\t\t" + obj + ".reflInfo = _reflInfo[" + primIndex + "];");
            map.AppendLine("\t\t" + obj + ".refractInfo = _refractInfo[" + primIndex + "];");

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
            map.AppendLine("\t\t" + obj + ".dist = distBuffer[" + primIndex + "];");

            info = prim.Colour;
            map.AppendLine("\t\t" + obj + ".colour = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

            info = prim.ReflectionInfo;
            map.AppendLine("\t\t" + obj + ".reflInfo = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

            info = prim.RefractionInfo;
            map.AppendLine("\t\t" + obj + ".refractInfo = float4(" + info.x + ", " + info.y + ", " + info.z + ", " + info.w + ");");

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
                map.AppendLine("\t\tscene = opUMat(scene, " + obj + ");");
                break;
            case CombineOpsTypes.Subtraction:
                map.AppendLine("\t\tscene = opSMat(" + obj + ", scene);");
                break;
            case CombineOpsTypes.Intersection:
                map.AppendLine("\t\tscene = opIMat(scene, " + obj + ");");
                break;
            case CombineOpsTypes.SmoothUnion:
                map.AppendLine("\t\tscene = opSmoothUnionMat(scene, " + obj + ", " + combineOps);
                break;
            case CombineOpsTypes.SmoothSubtraction:
                map.AppendLine("\t\tscene = opSmoothSubMat(" + obj + ", scene, " + combineOps);
                break;
            case CombineOpsTypes.SmoothIntersection:
                map.AppendLine("\t\tscene = opSmoothIntMat(scene, " + obj + ", " + combineOps);
                break;
            case CombineOpsTypes.AbsUnion:
                map.AppendLine("\t\tscene = opUAbsMat(scene, " + obj + ");");
                break;
            default:
                map.AppendLine("\t\tscene = opUMat(scene, " + obj + ");");
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
            map.AppendLine("\t\tobj2 = storedCSGs[" + (csgIndex - 1) + "];");
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

            map.AppendLine("\t\tobj = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();

            // Parse second node.
            parsePrimitiveMat(ref map, csg.SecondNode as RMPrimitive, ref primIndex, true);

            // Parse this CSG.
            determineCSGCombineOpMat(ref map, csg, csgIndex);
            //map.AppendLine("\t\tstoredCSGs[" + csgIndex + "] = csg;");
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

            map.AppendLine("\t\tobj = storedCSGs[" + firstNodeIndex + "];");
            map.AppendLine();
            map.AppendLine("\t\tobj2 = storedCSGs[" + (csgIndex - 1) + "];");
            map.AppendLine();

            // Parse this CSG.
            determineCSGCombineOpMat(ref map, csg, csgIndex);
            //map.AppendLine("\t\tstoredCSGs[" + csgIndex + "] = csg;");
            map.AppendLine();
            ++csgIndex;
            return;
        }
    }

    private void determineCSGCombineOpMat(ref StringBuilder map, CSG csg, uint csgIndex)
    {
        string result = "\t\tstoredCSGs[" + csgIndex + "]";

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
                map.AppendLine("\t\tcsg.dist = lerp(obj.dist, obj2.dist, _combineOpsCSGs[" + csgIndex + "].y);");
                map.AppendLine("\t\tcsg.colour = lerp(obj.colour, obj2.colour, _combineOpsCSGs[" + csgIndex + "].y);");
                map.AppendLine("\t\treflWeight = step(0.5, _combineOpsCSGs[7].y);");
                map.AppendLine("\t\tcsg.reflInfo = (1.0 - reflWeight) * obj.reflInfo + reflWeight * obj2.reflInfo;");
                map.AppendLine(result + " = csg;");
                break;
            default:
                break;
        }
    }




    [MenuItem("Shader Builder/Build Command _F6")]
    static void BuildCommand()
    {
        //Camera.main.GetComponent<ShaderBuilder>().build();
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(ShaderBuilder))]
public class ShaderBuilderEditor : Editor
{
    SerializedProperty _path;
    SerializedProperty shaderTemplatePath;
    SerializedProperty _shaderTemplate;
    List<RayMarchShader> _shaders = new List<RayMarchShader>();
    SerializedProperty _currentShader;
    int _selectedShaderIndex = 0;
    string[] _shaderNames;
    SerializedProperty _buildMode;
    SerializedProperty _buildType;



    private void OnEnable()
    {
        _path = serializedObject.FindProperty("_path");
        shaderTemplatePath = serializedObject.FindProperty("_shaderTemplatePath");
        _shaderTemplate = serializedObject.FindProperty("shaderTemplate");
        _currentShader = serializedObject.FindProperty("_currentShader");
        _buildMode = serializedObject.FindProperty("_buildMode");
        _buildType = serializedObject.FindProperty("_buildType");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        var shaderBuilder = target as ShaderBuilder;

        serializedObject.Update();

        GUIContent label = new GUIContent("General", "");
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_path);
        //EditorGUILayout.PropertyField(_shaderTemplatePath);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_shaderTemplate);
        if (EditorGUI.EndChangeCheck())
        {
            shaderTemplatePath.stringValue = AssetDatabase.GetAssetPath(_shaderTemplate.objectReferenceValue);
        }

        // Select a shader to build.
        label.text = "Shaders";
        label.tooltip = "Available shaders to build.";
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        _shaders = shaderBuilder.GetRayMarcher.Shaders;
        _shaderNames = new string[_shaders.Count];

        for (int i = 0; i < _shaders.Count; ++i)
        {
            _shaderNames[i] = _shaders[i].ShaderName;
        }

        _selectedShaderIndex = EditorGUILayout.Popup(_selectedShaderIndex, _shaderNames);


        label.text = "Build Options";
        label.tooltip = "";
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_buildMode);
        EditorGUILayout.PropertyField(_buildType);


        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        if (GUILayout.Button("Build"))
        {
            shaderBuilder.CurrentShader = _shaders[_selectedShaderIndex];
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
