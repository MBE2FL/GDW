using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RayMarchSkeleton : MonoBehaviour
{
    [SerializeField]
    List<RMObj> _rmObjs;


    // Update is called once per frame
    void Update()
    {
        //int index = 0;

        //recurseHierachy(transform, ref index);
    }

    void recurseHierachy(Transform parent, ref int index)
    {
        foreach (Transform child in parent)
        {
            if (index > _rmObjs.Count)
                break;

            _rmObjs[index].transform.position = child.position;

            ++index;

            recurseHierachy(child, ref index);
        }
    }
}

#if UNITY_EDITOR
public class RayMarchSkeletonWIndow : EditorWindow
{
    static List<System.Type> _desiredDockNextTo = new List<System.Type>();

    RayMarchSkeleton _skeleton;
    [SerializeField]
    TreeViewState _skeletonTreeViewState;
    SkeletonTreeView _skeletonTreeView;

    public static void init(RayMarchSkeleton skeleton)
    {
        // Get existing open window or if none, make a new one:
        //ShaderEditorWindow window = (ShaderEditorWindow)EditorWindow.GetWindow(typeof(ShaderEditorWindow));
        //window.Show();

        RayMarchSkeletonWIndow window = CreateWindow<RayMarchSkeletonWIndow>(_desiredDockNextTo.ToArray());
        window.Show();
        _desiredDockNextTo.Add(window.GetType());
        window._skeleton = skeleton;
        window._skeletonTreeView = new SkeletonTreeView(window._skeletonTreeViewState, skeleton.transform);

        window.titleContent = new GUIContent(skeleton.name + " Skeleton");


        //window._onRemoveObj += window.removeObj;
        //window._onRemoveShader += window.removeShader;
    }

    private void OnEnable()
    {
        // Check whether there is already a serialized view state (state 
        // that survived assembly reloading)
        if (_skeletonTreeViewState == null)
            _skeletonTreeViewState = new TreeViewState();

        //_skeletonTreeView = new SkeletonTreeView(_skeletonTreeViewState, _skeleton.transform);
    }

    private void OnGUI()
    {
        _skeletonTreeView.OnGUI(new Rect(0, 0, position.width, position.height));
    }
}

public class SkeletonTreeView : TreeView
{
    [SerializeField]
    Transform _rootBone;

    public SkeletonTreeView(TreeViewState treeViewState, Transform rootBone)
        : base(treeViewState)
    {
        _rootBone = rootBone;

        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
        // are created from data. Here we create a fixed set of items. In a real world example,
        // a data model should be passed into the TreeView and the items created from the model.

        // This section illustrates that IDs should be unique. The root item is required to 
        // have a depth of -1, and the rest of the items increment from that.
        //var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
        //var allItems = new List<TreeViewItem>
        //{
        //    new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
        //    new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
        //    new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
        //    new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
        //    new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
        //    new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
        //    new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
        //    new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
        //    new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
        //};

        // Utility method that initializes the TreeViewItem.children and .parent for all items.
        //SetupParentsAndChildrenFromDepths(root, allItems);

        // Return root of the tree
        //return root;


        if (!_rootBone)
            return null;

        List<TreeViewItem> items = new List<TreeViewItem>();
        int boneDepth = -1;
        TreeViewItem rootItem = new TreeViewItem { id = _rootBone.GetInstanceID(), depth = boneDepth, displayName = _rootBone.name };

        buildTree(_rootBone, boneDepth, ref items);


        // Utility method that initializes the TreeViewItem.children and .parent for all items.
        SetupParentsAndChildrenFromDepths(rootItem, items);

        // Return root of the tree.
        return rootItem;
    }

    private void buildTree(Transform parent, int boneDepth, ref List<TreeViewItem> items)
    {
        ++boneDepth;

        foreach(Transform child in parent.transform)
        {
            TreeViewItem item = new TreeViewItem { id = child.GetInstanceID(), depth = boneDepth, displayName = child.name };

            buildTree(child, boneDepth, ref items);
        }
    }
}

[CustomEditor(typeof(RayMarchSkeleton))]
public class RayMarchSkeletonEditor : Editor
{
    private void OnEnable()
    {
        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();


        if (GUILayout.Button("Skeleton Settings"))
        {
            RayMarchSkeletonWIndow.init(target as RayMarchSkeleton);
        }


        serializedObject.ApplyModifiedProperties();
    }
}
#endif
