using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShaderType
{
    Rendering,
    MarchingCube,
    Collision
}

[System.Serializable]
[ExecuteInEditMode]
public abstract class RayMarchShader : MonoBehaviour
{
    [SerializeField]
    protected Shader _effectShader = null;
    [SerializeField]
    protected string _shaderName = "New Shader";
    [SerializeField]
    protected RayMarchShaderSettings _settings;
    [SerializeField]
    protected ShaderType _shaderType = ShaderType.Rendering;
    [SerializeField]
    protected List<RMObj> _renderList = new List<RMObj>();
    //private RMObj[] objects;                                // The array of objects to render.


    protected Matrix4x4[] _invModelMats = new Matrix4x4[32];   // The inverse transformation matrices of every object.
    protected float[] _scaleBuffer = new float[32];             // The scaling info for every object.
    protected Vector4[] _combineOps = new Vector4[32];         // The object to scene combine operations, for every object.
    protected Vector4[] _primitiveGeoInfo = new Vector4[32];   // The geometric info for every primitive object.
    protected Vector4[] _altInfo = new Vector4[32];           // The alteration info for every object.
    protected Vector4[] _bufferedCSGs = new Vector4[16];       // The list of node indices for a each CSG.
    protected Vector4[] _combineOpsCSGs = new Vector4[16];     // The node to node combine operations for each CSG.
    protected Vector4[] _boundGeoInfo = new Vector4[32];      // The geometric info for every object's bounding volume.



    public RayMarchShaderSettings Settings
    {
        get
        {
            return _settings;
        }
        set
        {
            _settings = value;
        }
    }
    
    public Shader EffectShader
    {
        get
        {
            return _effectShader;
        }
        set
        {
            _effectShader = value;
        }
    }

    public ShaderType ShaderType
    {
        get
        {
            return _shaderType;
        }
        set
        {
            _shaderType = value;
        }
    }

    public List<RMObj> RenderList
    {
        get
        {
            return _renderList;
        }
        set
        {
            // WARNING!!
            // For deserializing only!!
            _renderList = value;
        }
    }

    public bool Ready
    {
        get
        {
            return (_effectShader && _settings);
        }
    }

    public void AddToRenderList(RMObj rmObj)
    {
        // Check if the object is not already in the render list.
        if (!_renderList.Contains(rmObj))
        {
            // Add the object to the render list, and sort the list based on draw order.
            _renderList.Add(rmObj);
            _renderList.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        }
    }

    public void removeFromRenderList(RMObj rmObj)
    {
        int index = _renderList.IndexOf(rmObj);

        // Check if the object is in the list, if so then remove it.
        if (index > -1)
        {
            _renderList[index] = _renderList[_renderList.Count - 1];
            _renderList.RemoveAt(index);
        }
        
    }

    public void removeAllFromRenderList()
    {
        // Also run rmObj remove actions
        foreach (RMObj obj in _renderList)
        {
            obj.removeFromShaderList(this);
        }

        _renderList.Clear();
    }

    public void remove()
    {
        // Notify all listeners
        removeAllFromRenderList();
    }

    public string ShaderName
    {
        get
        {
            return _shaderName;
        }

        set
        {
            _shaderName = value;
        }
    }


    //public RayMarchShader()
    //{
    //    // TO-DO load saved render list

    //    _renderList = new List<RMObj>();
    //}


    //void Awake()
    //{
        //if (Application.isPlaying)
        //{
        //    objects = FindObjectsOfType<RMObj>();
        //    _renderList = new List<RMObj>(objects);

        //    _renderList.Sort((obj1, obj2) => obj1.DrawOrder.CompareTo(obj2.DrawOrder));
        //}
    //}

    public void disableKeywords(Material material)
    {
        // Disable this shader's defines
        foreach (ShaderKeywords keyword in _settings.Keywords)
        {
            material.DisableKeyword(keyword.ToString());
        }
    }

    protected virtual void renderPrimitive(RMPrimitive rmPrim, ref int primIndex, ref int altIndex)
    {
       // Homogeneous transformation matrices
       _invModelMats[primIndex] = rmPrim.transform.localToWorldMatrix.inverse;

       // Primitive to render
       //primitiveTypes[primIndex] = (float)rmPrim.PrimitiveType;

       // Combine Operation
       _combineOps[primIndex] = rmPrim.CombineOp;

       // Primitive Geometry Information
       _primitiveGeoInfo[primIndex] = rmPrim.GeoInfo;

       // Alterations' Information
       foreach (Alteration alt in rmPrim.Alterations)
       {
           _altInfo[altIndex] = alt.info;
           ++altIndex;
       }
       //foreach(Vector4 altInfo in rmPrim.AlterationInfo)
       //{
       //    _altInfo[altIndex] = altInfo;
       //    ++altIndex;
       //}

       _boundGeoInfo[primIndex] = rmPrim.BoundGeoInfo;

       ++primIndex;
    }

    protected void renderCSG(CSG csg, ref int primIndex, ref int csgIndex, ref int altIndex)
    {
       // TO-DO Don't let incomplete CSG children nodes be added to other CSGs.
       // Base case: Both nodes are primitives.
       if (csg.AllPrimNodes)
       {
           // Render both nodes.
           renderPrimitive(csg.FirstNode as RMPrimitive, ref primIndex, ref altIndex);
           renderPrimitive(csg.SecondNode as RMPrimitive, ref primIndex, ref altIndex);

           // Buffer this CSG.
           _bufferedCSGs[csgIndex] = new Vector4(primIndex - 1, primIndex, -1, -1);
           _combineOpsCSGs[csgIndex] = csg.CombineOp;
           //_boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
           ++csgIndex;
           return;
       }
       // Only first node is a primitive.
       else if (csg.IsFirstPrim)
       {
           // Recurse through second node (Must be a CSG).
           renderCSG(csg.SecondNode as CSG, ref primIndex, ref csgIndex, ref altIndex);

           // Render first node.
           renderPrimitive(csg.FirstNode as RMPrimitive, ref primIndex, ref altIndex);

           // Buffer this CSG.
           _bufferedCSGs[csgIndex] = new Vector4(primIndex, -1, -1, csgIndex - 1);
           _combineOpsCSGs[csgIndex] = csg.CombineOp;
           //_boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
           ++csgIndex;
           return;
       }
       // Only second node is a primitive.
       else if (csg.IsSecondPrim)
       {
           // Recurse through first node (Must be a csg).
           renderCSG(csg.FirstNode as CSG, ref primIndex, ref csgIndex, ref altIndex);

           // Render second node.
           renderPrimitive(csg.SecondNode as RMPrimitive, ref primIndex, ref altIndex);

           // Buffer this CSG.
           _bufferedCSGs[csgIndex] = new Vector4(-1, primIndex, csgIndex - 1, -1);
           _combineOpsCSGs[csgIndex] = csg.CombineOp;
           //_boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
           ++csgIndex;
           return;
       }
       // Both nodes are CSGs.
       else
       {
           Vector4 tempCSG = new Vector4(-1, -1, -1, -1);

           // Recurse through first node.
           renderCSG(csg.FirstNode as CSG, ref primIndex, ref csgIndex, ref altIndex);
           tempCSG.z = csgIndex;

           // Recurse through second node.
           renderCSG(csg.SecondNode as CSG, ref primIndex, ref csgIndex, ref altIndex);
           tempCSG.w = csgIndex;

           // Buffer this CSG.
           _bufferedCSGs[csgIndex] = tempCSG;
           _combineOpsCSGs[csgIndex] = csg.CombineOp;
           //_boundGeoInfo[csgIndex] = csg.BoundGeoInfo;
           ++csgIndex;
           return;
       }
    }
}
