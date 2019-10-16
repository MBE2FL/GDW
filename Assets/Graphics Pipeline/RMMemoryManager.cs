using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Ray Marching/RMMemoryManager")]
[DisallowMultipleComponent]

public class RMMemoryManager : MonoBehaviour
{
    private const int MAX_PRIMS = 32;
    private const int MAX_CSGS = 16;
    [SerializeField]
    private List<RMPrimitive> _prims = new List<RMPrimitive>(MAX_PRIMS);
    [SerializeField]
    private List<CSG> _csgs = new List<CSG>(MAX_CSGS);
    //[SerializeField]
    //private static RMMemoryManager _instance;
    public bool reset = false;
    private bool _dirty = false;
    [SerializeField]
    private bool _initialLoad = false;

    public List<RMPrimitive> RM_Prims
    {
        get
        {
            return _prims;
        }
    }

    public List<CSG> CSGs
    {
        get
        {
            return _csgs;
        }
    }

    public bool Dirty
    {
        get
        {
            return _dirty;
        }
        set
        {
            _dirty = value;
        }
    }

    //public static RMMemoryManager Instance
    //{
    //    get
    //    {
    //        // Instance is not set or is lost
    //        if (!_instance)
    //        {
    //            // Check if there is an instance already running.
    //            // Would only be on the main camera.
    //            _instance = Camera.main.GetComponent<RMMemoryManager>();

    //            // Create an instance if there are no other instances already running.
    //            if (!_instance)
    //                Camera.main.gameObject.AddComponent<RMMemoryManager>();
    //        }

    //        return _instance;
    //    }
    //}


    private void Awake()
    {
        // Create a memory manager instance, if none exists.
        //if (!_instance)
        //{
        //    createInstance();
        //}

        // Load previously saved objects back into the memory manager.
        //_prims.Clear();
        //_csgs.Clear();
        if (!_initialLoad)
            loadSavedObjs();
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        if (reset)
        {
            _prims.Clear();
            _csgs.Clear();
            reset = false;
            loadSavedObjs();
            _dirty = false;
        }

        if (_dirty)
            verifyMemory();
    }

    //private void createInstance()
    //{
    //    _instance = this;

    //    GameObject obj;
    //    RMPrimitive prim;
    //    CSG csg;

    //    GameObject[] savedPrims = GameObject.FindGameObjectsWithTag("RM_Primitive");
    //    GameObject[] savedCSGs = GameObject.FindGameObjectsWithTag("RM_CSG");

    //    for (int i = 0; i < (MAX_PRIMS - savedPrims.Length); ++i)
    //    {
    //        // Creates a game object, disables it, and hides it from the hierarchy.
    //        obj = new GameObject();
    //        obj.SetActive(false);
    //        obj.hideFlags = HideFlags.HideInHierarchy;

    //        // Add a RMPrimitive component.
    //        prim = obj.AddComponent<RMPrimitive>();
    //        prim.PrimitiveType = PrimitiveTypes.Sphere;
    //        _rmPrimsInactive.Add(prim);
    //    }

    //    for (uint i = 0; i < savedPrims.Length; ++i)
    //    {
    //        prim = savedPrims[i].GetComponent<RMPrimitive>();
    //        _prims.Add(prim);
    //    }



    //    for (int i = 0; i < (MAX_CSGS - savedCSGs.Length); ++i)
    //    {
    //        // Creates a game object, disables it, and hides it from the hierarchy.
    //        obj = new GameObject();
    //        obj.SetActive(false);
    //        obj.hideFlags = HideFlags.HideInHierarchy;

    //        // Add a CSG (Constructive Solid Geometry) component.
    //        csg = obj.AddComponent<CSG>();
    //        _csgsInactive.Add(csg);
    //    }

    //    for (uint i = 0; i < savedCSGs.Length; ++i)
    //    {
    //        csg = savedCSGs[i].GetComponent<CSG>();
    //        _csgs.Add(csg);
    //    }
    //}

    void loadSavedObjs()
    {
        RMPrimitive prim;
        CSG csg;

        GameObject[] savedPrims = GameObject.FindGameObjectsWithTag("RM_Primitive");
        GameObject[] savedCSGs = GameObject.FindGameObjectsWithTag("RM_CSG");

        for (uint i = 0; i < savedPrims.Length; ++i)
        {
            prim = savedPrims[i].GetComponent<RMPrimitive>();
            _prims.Add(prim);
        }

        for (uint i = 0; i < savedCSGs.Length; ++i)
        {
            csg = savedCSGs[i].GetComponent<CSG>();
            _csgs.Add(csg);
        }

        _initialLoad = true;
    }

    private RMPrimitive createPrimitive(PrimitiveTypes type)
    {
        // Creates a game object, disables it, and hides it from the hierarchy.
        GameObject obj = new GameObject();
        obj.tag = "RM_Primitive";

        // Add a RMPrimitive component.
        RMPrimitive prim = obj.AddComponent<RMPrimitive>();
        prim.PrimitiveType = type;
        _prims.Add(prim);

        return prim;
    }

    private CSG createCSG()
    {
        // Creates a game object, disables it, and hides it from the hierarchy.
        GameObject obj = new GameObject();
        obj.tag = "RM_CSG";

        // Add a CSG (Constructive Solid Geometry) component.
        CSG csg = obj.AddComponent<CSG>();
        _csgs.Add(csg);

        return csg;
    }

    public RMPrimitive getRMPrimitive(int index)
    {
        RMPrimitive prim = null;
        int count = _prims.Count;

        if (count > 0)
            prim = _prims[index];

        return prim;
    }

    public CSG getCSG(int index)
    {
        CSG csg = null;
        int count = _csgs.Count;

        if (count > 0)
            csg = _csgs[index];

        return csg;
    }

    public void verifyMemory()
    {
        RMPrimitive prim;
        RMPrimitive temp;

        // Check for null primitive objects.
        for (int i = 0; i < _prims.Count;)
        {
            prim = _prims[i];

            // Swap last element with current element you wish to remove.
            if (!prim)
            {
                temp = _prims[_prims.Count - 1];
                _prims[_prims.Count - 1] = prim;
                _prims[i] = temp;

                _prims.RemoveAt(_prims.Count - 1);
            }
            // Keep looking for null elements.
            else
                ++i;
        }


        CSG csg;
        CSG tempCSG;

        // Check for null csg objects.
        for (int i = 0; i < _csgs.Count;)
        {
            csg = _csgs[i];

            // Swap last element with current element you wish to remove.
            if (!csg)
            {
                tempCSG = _csgs[_csgs.Count - 1];
                _csgs[_csgs.Count - 1] = csg;
                _csgs[i] = tempCSG;

                _csgs.RemoveAt(_csgs.Count - 1);
            }
            else
                // Keep looking for null elements.
                ++i;
        }

        _dirty = false;
    }


#if UNITY_EDITOR
    [MenuItem("GameObject/Ray Marched/rmSphere", false, 10)]
    static void CreateSphere(MenuCommand menuCommand)
    {
        // TO-DO Check if memory manager exists.
        RMPrimitive prim = Camera.main.GetComponent<RMMemoryManager>().createPrimitive(PrimitiveTypes.Sphere);

        // Check if the memory manager was able to provide a primitive.
        if (!prim)
            return;

        GameObject obj = prim.gameObject;

        // Ensure the obj gets parented if this was a context click (otherwise does nothing).
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        // Register the creation in the undo system.
        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        //Undo.undoRedoPerformed += Camera.main.GetComponent<RMMemoryManager>().verifyMemory;
        Selection.activeObject = obj;

        // Update and reload the shader.
        //Camera.main.GetComponent<ShaderBuilder>().build();
    }

    [MenuItem("GameObject/Ray Marched/rmBox", false, 10)]
    static void CreateBox(MenuCommand menuCommand)
    {
        RMPrimitive prim = Camera.main.GetComponent<RMMemoryManager>().createPrimitive(PrimitiveTypes.Box);

        // Check if the memory manager was able to provide a primitive.
        if (!prim)
            return;

        GameObject obj = prim.gameObject;

        // Ensure the obj gets parented if this was a context click (otherwise does nothing).
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        // Register the creation in the undo system.
        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        Selection.activeObject = obj;

        // Update and reload the shader.
        //Camera.main.GetComponent<ShaderBuilder>().build();
    }

    [MenuItem("GameObject/Ray Marched/CSG", false, 10)]
    static void CreateCSG(MenuCommand menuCommand)
    {
        CSG csg = Camera.main.GetComponent<RMMemoryManager>().createCSG();

        // Check if the memory manager was able to provide a csg.
        if (!csg)
            return;

        GameObject obj = csg.gameObject;
        obj.name = "CSG";

        // Ensure the obj gets parented if this was a context click (otherwise does nothing).
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        // Register the creation in the undo system.
        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        Selection.activeObject = obj;

        // Update and reload the shader.
        //Camera.main.GetComponent<ShaderBuilder>().build();
    }
#endif
}
