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
    private const int MAX_RM_PRIMS = 32;
    private const int MAX_CSGS = 16;
    [SerializeField]
    private List<RMPrimitive> _rmPrims = new List<RMPrimitive>(MAX_RM_PRIMS);
    [SerializeField]
    private List<CSG> _csgs = new List<CSG>(MAX_CSGS);
    [SerializeField]
    private List<RMPrimitive> _rmPrimsInactive = new List<RMPrimitive>(MAX_RM_PRIMS);
    [SerializeField]
    private List<CSG> _csgsInactive = new List<CSG>(MAX_CSGS);
    //[SerializeField]
    //private static RMMemoryManager _instance;
    public bool reset = false;

    public List<RMPrimitive> RM_Prims
    {
        get
        {
            return _rmPrims;
        }
    }

    public List<CSG> CSGs
    {
        get
        {
            return _csgs;
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
            reset = false;
            loadSavedObjs();
        }
    }

    //private void createInstance()
    //{
    //    _instance = this;

    //    GameObject obj;
    //    RMPrimitive rmPrim;
    //    CSG csg;

    //    GameObject[] savedPrims = GameObject.FindGameObjectsWithTag("RM_Primitive");
    //    GameObject[] savedCSGs = GameObject.FindGameObjectsWithTag("RM_CSG");

    //    for (int i = 0; i < (MAX_RM_PRIMS - savedPrims.Length); ++i)
    //    {
    //        // Creates a game object, disables it, and hides it from the hierarchy.
    //        obj = new GameObject();
    //        obj.SetActive(false);
    //        obj.hideFlags = HideFlags.HideInHierarchy;

    //        // Add a RMPrimitive component.
    //        rmPrim = obj.AddComponent<RMPrimitive>();
    //        rmPrim.PrimitiveType = PrimitiveTypes.Sphere;
    //        _rmPrimsInactive.Add(rmPrim);
    //    }

    //    for (uint i = 0; i < savedPrims.Length; ++i)
    //    {
    //        rmPrim = savedPrims[i].GetComponent<RMPrimitive>();
    //        _rmPrims.Add(rmPrim);
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
        //GameObject obj;
        RMPrimitive prim;
        CSG csg;

        GameObject[] savedPrims = GameObject.FindGameObjectsWithTag("RM_Primitive");
        GameObject[] savedCSGs = GameObject.FindGameObjectsWithTag("RM_CSG");

        cleanUp();

        for (int i = 0; i < (MAX_RM_PRIMS - savedPrims.Length); ++i)
        {
            createPrimitive();
        }

        for (uint i = 0; i < savedPrims.Length; ++i)
        {
            prim = savedPrims[i].GetComponent<RMPrimitive>();
            _rmPrims.Add(prim);
        }



        for (int i = 0; i < (MAX_CSGS - savedCSGs.Length); ++i)
        {
            createCSG();
        }

        for (uint i = 0; i < savedCSGs.Length; ++i)
        {
            csg = savedCSGs[i].GetComponent<CSG>();
            _csgs.Add(csg);
        }
    }

    private void createPrimitive()
    {
        // Creates a game object, disables it, and hides it from the hierarchy.
        GameObject obj = new GameObject();
        obj.SetActive(false);
        obj.hideFlags = HideFlags.HideInHierarchy;

        // Add a RMPrimitive component.
        RMPrimitive prim = obj.AddComponent<RMPrimitive>();
        prim.PrimitiveType = PrimitiveTypes.Sphere;
        _rmPrimsInactive.Add(prim);
    }

    private void createCSG()
    {
        // Creates a game object, disables it, and hides it from the hierarchy.
        GameObject obj = new GameObject();
        obj.SetActive(false);
        obj.hideFlags = HideFlags.HideInHierarchy;

        // Add a CSG (Constructive Solid Geometry) component.
        CSG csg = obj.AddComponent<CSG>();
        _csgsInactive.Add(csg);
    }

    public RMPrimitive getRMPrimitive()
    {
        RMPrimitive rmPrim = null;
        int count = _rmPrimsInactive.Count;

        if (count > 0)
        {
            // Add the primitive to the active pool.
            rmPrim = _rmPrimsInactive[count - 1];
            _rmPrims.Add(rmPrim);

            // Remove the primitive from the inactive pool.
            _rmPrimsInactive.RemoveAt(count - 1);

            // Set the game object's active settings.
            GameObject obj = rmPrim.gameObject;
            obj.SetActive(true);
            obj.hideFlags = HideFlags.None;
            obj.tag = "RM_Primitive";

            // Return the pooled primitive.
            return rmPrim;
        }

        // Output an error message, and return null.
        Debug.LogError("Exceeded maximum amount of ray marched primitives!");

        return rmPrim;
    }

    public CSG getCSG()
    {
        CSG csg = null;
        int count = _csgsInactive.Count;

        if (count > 0)
        {
            // Add the csg to the active pool.
            csg = _csgsInactive[count - 1];
            _csgs.Add(csg);

            // Remove the csg from the inactive pool.
            _csgsInactive.RemoveAt(count - 1);

            // Set the game object's active settings.
            GameObject obj = csg.gameObject;
            obj.SetActive(true);
            obj.hideFlags = HideFlags.None;
            obj.tag = "RM_CSG";

            // Return the pooled csg.
            return csg;
        }

        // Output an error message, and return null.
        Debug.LogError("Exceeded maximum of amount csgs!");

        return csg;
    }

    public void reclaimRMPrimitive(RMPrimitive rmPrim)
    {
        if (!rmPrim)
        {
            Debug.LogWarning("Tried to reclaim null Primitive!");
            return;
        }

        int activeCount = _rmPrims.Count;
        int inactiveCount = _rmPrimsInactive.Count;

        if (activeCount <= 0 || inactiveCount >= MAX_RM_PRIMS)
        {
            Debug.LogError("Ray march Primitive memory reclaimation error!");
            return;
        }

        try
        {
            // Remove the primitive from the active pool.
            if (activeCount > 1)
            {
                RMPrimitive temp = _rmPrims[_rmPrims.Count - 1];
                _rmPrims[_rmPrims.IndexOf(rmPrim)] = temp;
            }
            _rmPrims.RemoveAt(_rmPrims.Count - 1);

            // Add the primitive to the inactive pool.
            _rmPrimsInactive.Add(rmPrim);

            // Set the game object's inactive settings.
            GameObject obj = rmPrim.gameObject;
            resetGameObject(obj, rmPrim);

            // Reset the Primitive component.
            rmPrim.resetPrim();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Primitive could not be reclaimed. Either it was already reclaimed, or it is not owned by the memory manager.\n" + 
                                ex.Message);
        }


#if UNITY_EDITOR
        // De-select the object, and refresh the hierarchy.
        Selection.activeObject = null;
        EditorApplication.DirtyHierarchyWindowSorting();
#endif
    }

    public void reclaimCSG(CSG csg)
    {
        if (!csg)
        {
            Debug.LogWarning("Tried to reclaim null CSG!");
            return;
        }

        int activeCount = _csgs.Count;
        int inactiveCount = _csgsInactive.Count;

        if (activeCount <= 0 || inactiveCount >= MAX_CSGS)
        {
            Debug.LogError("Ray march CSG memory reclaimation error!");
            return;
        }

        try
        {
            // Remove the csg from the active pool.
            if (activeCount > 1)
            {
                CSG temp = _csgs[_csgs.Count - 1];
                _csgs[_csgs.IndexOf(csg)] = temp;
            }
            _csgs.RemoveAt(_csgs.Count - 1);

            // Add the csg to the inactive pool.
            _csgsInactive.Add(csg);

            // Set the game object's inactive settings.
            GameObject obj = csg.gameObject;
            resetGameObject(obj, null, csg);

            // Relaim all primitives and CSGs belonging to this CSG.
            if (csg.FirstNode)
            {
                if (csg.IsFirstPrim)
                    reclaimRMPrimitive(csg.FirstNode as RMPrimitive);
                else
                    reclaimCSG(csg.FirstNode as CSG);
            }

            if (csg.SecondNode)
            {
                if (csg.IsSecondPrim)
                    reclaimRMPrimitive(csg.SecondNode as RMPrimitive);
                else
                    reclaimCSG(csg.SecondNode as CSG);
            }

            // Reset the CSG component.
            csg.resetCSG();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("CSG could not be reclaimed. Either it was already reclaimed, or it is not owned by the memory manager.\n" +
                                ex.Message);
        }


#if UNITY_EDITOR
        // De-select the object, and refresh the hierarchy.
        Selection.activeObject = null;
        EditorApplication.DirtyHierarchyWindowSorting();
#endif
    }

    private void resetGameObject(GameObject obj, RMPrimitive prim = null, CSG csg = null)
    {
        if ((prim && csg) || (!prim && !csg))
        {
            Debug.LogError("Ray march GameObject memory reclaimation error!");
            return;
        }

        // Set the game object's inactive settings.
        obj.SetActive(false);
        obj.hideFlags = HideFlags.HideInHierarchy;
        obj.tag = "Untagged";
        obj.transform.SetParent(null);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;


        // Reclaim all children game objects.
        List<Transform> children = new List<Transform>();
        foreach (Transform child in obj.transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            RMPrimitive childPrim = child.GetComponent<RMPrimitive>();
            CSG childCSG = child.GetComponent<CSG>();

            if (childPrim)
                reclaimRMPrimitive(childPrim);
            else if (childCSG)
                reclaimCSG(childCSG);
        }


        // Delete any components other than the Transform, Primitive, or CSG.
        Component[] components = obj.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if ((comp != obj.transform) && ((comp != prim) && (comp != csg)))
#if UNITY_EDITOR
                DestroyImmediate(comp);
#else
            Destroy(comp);
#endif
        }
    }

    public void cleanUp()
    {
        _rmPrims.Clear();
        _rmPrimsInactive.Clear();
        _csgs.Clear();
        _csgsInactive.Clear();
    }

    public void replaceDeletedPrimitive(int index)
    {
        Debug.LogWarning("Remember to use 'Delete RMObj', instead of 'Delete', for Ray Marched objects.");

        int activeCount = _rmPrims.Count;

        // Add a new primitive to the inactive pool.
        createPrimitive();

        // Remove the null primitive from the active pool.
        if (activeCount > 1)
        {
            RMPrimitive temp = _rmPrims[_rmPrims.Count - 1];
            _rmPrims[index] = temp;
        }
        _rmPrims.RemoveAt(_rmPrims.Count - 1);
    }

    public void replaceDeletedCSG(int index)
    {
        Debug.LogWarning("Remember to use 'Delete RMObj', instead of 'Delete', for Ray Marched objects.");

        int activeCount = _csgs.Count;

        // Add a new CSG to the inactive pool.
        createCSG();

        // Remove the null CSG from the active pool.
        if (activeCount > 1)
        {
            CSG temp = _csgs[_csgs.Count - 1];
            _csgs[index] = temp;
        }
        _csgs.RemoveAt(_csgs.Count - 1);
    }


#if UNITY_EDITOR
    [MenuItem("GameObject/Ray Marched/rmSphere", false, 10)]
    static void CreateSphere(MenuCommand menuCommand)
    {
        // TO-DO Check if memory manager exists.
        RMPrimitive rmPrim = Camera.main.GetComponent<RMMemoryManager>().getRMPrimitive();

        // Check if the memory manager was able to provide a primitive.
        if (!rmPrim)
            return;

        rmPrim.PrimitiveType = PrimitiveTypes.Sphere;
        GameObject obj = rmPrim.gameObject;
        obj.name = "rmSphere";

        // Ensure the obj gets parented if this was a context click (otherwise does nothing).
        GameObjectUtility.SetParentAndAlign(obj, menuCommand.context as GameObject);
        // Register the creation in the undo system.
        Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
        Selection.activeObject = obj;

        // Update and reload the shader.
        //Camera.main.GetComponent<ShaderBuilder>().build();
    }

    [MenuItem("GameObject/Ray Marched/rmBox", false, 10)]
    static void CreateBox(MenuCommand menuCommand)
    {
        RMPrimitive rmPrim = Camera.main.GetComponent<RMMemoryManager>().getRMPrimitive();

        // Check if the memory manager was able to provide a primitive.
        if (!rmPrim)
            return;

        rmPrim.PrimitiveType = PrimitiveTypes.Box;
        GameObject obj = rmPrim.gameObject;
        obj.name = "rmBox";

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
        CSG csg = Camera.main.GetComponent<RMMemoryManager>().getCSG();

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


    // ####################### FIX DELETING OBJECTS FROM UNITY ########################
    //[MenuItem("GameObject/DeleteRMPrim", false, 10)]
    //static void DeleteRMPrim(MenuCommand command)
    //{
    //    // TO-DO check for null context, and if its not a primitive.
    //    RMPrimitive rmPrim = ((GameObject)command.context).GetComponent<RMPrimitive>();
    //    Camera.main.GetComponent<RMMemoryManager>().reclaimRMPrimitive(rmPrim);
    //}

    //[MenuItem("GameObject/DeleteCSG", false, 10)]
    //static void DeleteCSG(MenuCommand command)
    //{
    //    // TO-DO check for null context, and if its not a CSG.
    //    CSG csg = ((GameObject)command.context).GetComponent<CSG>();
    //    Camera.main.GetComponent<RMMemoryManager>().reclaimCSG(csg);
    //}

    [MenuItem("GameObject/Delete RMObj", false, 10)]
    static void deleteRMObj(MenuCommand command)
    {
        if (command.context)
        {
            try
            {
                GameObject gameObj = command.context as GameObject;

                RMObj obj = gameObj.GetComponent<RMPrimitive>();

                // Game object did not have a RMPrimitive component attached.
                if (!obj)
                {
                    obj = gameObj.GetComponent<CSG>();

                    // Game object did not have a CSG component attached.
                    if (!obj)
                    {
                        Debug.LogWarning("The game object you tried to delete was not a ray marched object.");
                        return;
                    }
                }

                // Reclaim the ray marched object.
                if (obj.IsPrim)
                    Camera.main.GetComponent<RMMemoryManager>().reclaimRMPrimitive(obj as RMPrimitive);
                else
                    Camera.main.GetComponent<RMMemoryManager>().reclaimCSG(obj as CSG);

                // Update and reload the shader.
                //Camera.main.GetComponent<ShaderBuilder>().build();

            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to delete ray marched object.\n" + ex.Message);
            }
        }
    }
#endif
}
