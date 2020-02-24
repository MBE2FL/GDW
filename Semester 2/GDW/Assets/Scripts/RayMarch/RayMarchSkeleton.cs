using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Assertions;


[System.Serializable]
public class RayMarchSkeleton : MonoBehaviour
{
    [SerializeField]
    List<RMObj> _rmObjs;

    [SerializeField]
    RayMarchSkeletonWIndow _skeletonWindow;

    IList<TreeViewItem> _treeItems;


    public RayMarchSkeletonWIndow SkeletonWindow
    {
        get
        {
            return _skeletonWindow;
        }
        set
        {
            _skeletonWindow = value;
        }
    }

    public IList<TreeViewItem> TreeItems
    {
        get
        {
            return _treeItems;
        }
        set
        {
            _treeItems = value;
        }
    }


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
[CustomEditor(typeof(RayMarchSkeleton))]
public class RayMarchSkeletonEditor : Editor
{

    private void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RayMarchSkeleton skeleton = target as RayMarchSkeleton;

        serializedObject.Update();


        if (GUILayout.Button("Skeleton Settings"))
        {
            if (!skeleton.SkeletonWindow)
                skeleton.SkeletonWindow = RayMarchSkeletonWIndow.initWindow(skeleton);

            if (skeleton.TreeItems == null)
                skeleton.TreeItems = skeleton.SkeletonWindow.init(skeleton);
            else
                skeleton.SkeletonWindow.init(skeleton, skeleton.TreeItems);
        }


        serializedObject.ApplyModifiedProperties();
    }
}



[System.Serializable]
public class RayMarchSkeletonWIndow : EditorWindow, ISerializationCallbackReceiver
{
    static List<System.Type> _desiredDockNextTo = new List<System.Type>();

    [SerializeField]
    RayMarchSkeleton _skeleton;
    [SerializeField]
    TreeViewState _skeletonTreeViewState;
    [SerializeField]
    MultiColumnHeaderState _headerState;
    [SerializeField]
    SkeletonTreeView _skeletonTreeView;
    bool _initialized = false;

    //[System.Serializable]
    //public struct SerializedTreeViewItem
    //{
    //    public Transform _rootBone;
    //    public IList<TreeViewItem> _items;
    //}

    //[SerializeField]
    //List<SerializedTreeViewItem> _serializedTreeViewItems;

    [System.Serializable]
    public struct SerializedTreeView
    {
        public BoneTreeViewItem _rootItem;
        public List<BoneTreeViewItem> _serializedItems;
    }

    [SerializeField]
    SerializedTreeView _serializedTreeView;

    Rect multiColumnTreeViewRect
    {
        get { return new Rect(20, 30, position.width - 40, position.height - 60); }
    }

    public static RayMarchSkeletonWIndow initWindow(RayMarchSkeleton skeleton)
    {
        // Get existing open window or if none, make a new one:
        //ShaderEditorWindow window = (ShaderEditorWindow)EditorWindow.GetWindow(typeof(ShaderEditorWindow));
        //window.Show();

        RayMarchSkeletonWIndow window = CreateWindow<RayMarchSkeletonWIndow>(_desiredDockNextTo.ToArray());
        window.Show();
        _desiredDockNextTo.Add(window.GetType());
        //window._skeleton = skeleton;
        //state = SkeletonTreeView.createDefaultMultiColumnHeaderState(window.multiColumnTreeViewRect.width);
        //MultiColumnHeader multiColumnHeader = new MultiColumnHeader(state);
        //window._skeletonTreeView = new SkeletonTreeView(window._skeletonTreeViewState, multiColumnHeader, skeleton.transform);

        window.titleContent = new GUIContent(skeleton.name + " Skeleton");

        //window.init(skeleton);

        //window._onRemoveObj += window.removeObj;
        //window._onRemoveShader += window.removeShader;

        return window;
    }

    public IList<TreeViewItem> init(RayMarchSkeleton skeleton)
    {
        _skeleton = skeleton;
        _headerState = SkeletonTreeView.createDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
        MultiColumnHeader multiColumnHeader = new MultiColumnHeader(_headerState);

        if (!_initialized)
        {
            _skeletonTreeView = new SkeletonTreeView(_skeletonTreeViewState, multiColumnHeader, skeleton.transform);
            _initialized = true;
        }

        return _skeletonTreeView.GetRows();
    }

    public IList<TreeViewItem> init(RayMarchSkeleton skeleton, IList<TreeViewItem> items)
    {
        if (!_initialized)
        {
            _skeleton = skeleton;
            _headerState = SkeletonTreeView.createDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
            MultiColumnHeader multiColumnHeader = new MultiColumnHeader(_headerState);


            _skeletonTreeView = new SkeletonTreeView(_skeletonTreeViewState, multiColumnHeader, skeleton.transform, items);
            _initialized = true;
        }

        return _skeletonTreeView.GetRows();
    }

    void initIfNeeded()
    {
        if (_skeletonTreeView.multiColumnHeader == null)
        {
            if (_skeletonTreeViewState == null)
                _skeletonTreeViewState = new TreeViewState();


            bool headerNotInitialized = _headerState == null;
            if (headerNotInitialized)
            {
                MultiColumnHeaderState newHeaderState = SkeletonTreeView.createDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);

                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_headerState, newHeaderState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_headerState, newHeaderState);

                _headerState = newHeaderState;
            }

            MultiColumnHeader multiColumnHeader = new MultiColumnHeader(_headerState);

            if (headerNotInitialized)
                multiColumnHeader.ResizeToFit();


            _skeletonTreeView = new SkeletonTreeView(_skeletonTreeViewState, multiColumnHeader, _serializedTreeView._rootItem, _serializedTreeView._serializedItems);


            //_initialized = true;
        }
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
        //init(_skeleton, _skeleton.TreeItems);
        initIfNeeded();

        _skeletonTreeView.OnGUI(new Rect(0, 0, position.width, position.height));
    }

    public void OnBeforeSerialize()
    {
        //throw new System.NotImplementedException();
        _serializedTreeView = new SerializedTreeView()
        {
            _rootItem = _skeletonTreeView.RootItem,
            _serializedItems = _skeletonTreeView.SavedItems
        };
    }

    public void OnAfterDeserialize()
    {
        //throw new System.NotImplementedException();
    }
}




[System.Serializable]
public class SkeletonTreeView : TreeView
{
    enum BoneColumns
    {
        BoneIcon,
        Name,
        RMIcon,
        HeadToTailTarget,
        LeafIcon
    }

    const float ROW_HEIGHT = 20f;
    const float TOGGLE_WIDTH = 18f;

    //[System.NonSerialized]
    //static Texture2D[] _icons =
    //{
    //    EditorGUIUtility.FindTexture ("tree_icon_branch"),
    //    EditorGUIUtility.FindTexture ("tree_icon_leaf"),
    //    EditorGUIUtility.FindTexture ("TreeEditor.Material")
    //};


    [SerializeField]
    Transform _rootBone;
    bool _rebuildTree;



    [SerializeField]
    List<BoneTreeViewItem> _savedItems;
    [SerializeField]
    BoneTreeViewItem _rootItem;
    [System.NonSerialized]
    bool _initialized = false;

    public List<BoneTreeViewItem> SavedItems
    {
        get
        {
            return _savedItems;
        }
    }

    public BoneTreeViewItem RootItem
    {
        get
        {
            return _rootItem;
        }
    }
    

    public SkeletonTreeView(TreeViewState treeViewState, Transform rootBone)
        : base(treeViewState)
    {
        _rootBone = rootBone;

        Reload();
    }

    public SkeletonTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader, Transform rootBone)
    : base(treeViewState, multiColumnHeader)
    {
        _rootBone = rootBone;

        showBorder = true;
        showAlternatingRowBackgrounds = true;
        extraSpaceBeforeIconAndLabel = 20.0f;
        columnIndexForTreeFoldouts = 1;

        Reload();
    }

    public SkeletonTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader, Transform rootBone, IList<TreeViewItem> items)
    : base(treeViewState, multiColumnHeader)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = true;
        extraSpaceBeforeIconAndLabel = 20.0f;
        columnIndexForTreeFoldouts = 1;

        _rootBone = rootBone;
        _rebuildTree = false;

        Reload();
    }

    public SkeletonTreeView(TreeViewState treeViewState, MultiColumnHeader multiColumnHeader, BoneTreeViewItem rootItem, List<BoneTreeViewItem> items)
    : base(treeViewState, multiColumnHeader)
    {
        showBorder = true;
        showAlternatingRowBackgrounds = true;
        extraSpaceBeforeIconAndLabel = 20.0f;
        columnIndexForTreeFoldouts = 1;

        _rootItem = rootItem;
        _savedItems = items;

        Reload();
    }

    protected override TreeViewItem BuildRoot()
    {
        // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
        // are created from data. Here we create a fixed set of items. In a real world example,
        // a data model should be passed into the TreeView and the items created from the model.


        // Utility method that initializes the TreeViewItem.children and .parent for all items.
        //SetupParentsAndChildrenFromDepths(root, allItems);

        if (_rebuildTree)
        {
            //TreeViewItem root = new BoneTreeViewItem { id = _rootBone.GetInstanceID(), depth = -1, displayName = _rootBone.name };
            //SetupParentsAndChildrenFromDepths(root, _items);
            //return root;
            return null;
        }

        List<TreeViewItem> items;
        TreeViewItem rootItem;

        if (_rootItem != null)
        {
            rootItem = _rootItem;

            items = new List<TreeViewItem>();
            foreach (BoneTreeViewItem savedItem in _savedItems)
            {
                items.Add(savedItem);
            }

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths(rootItem, items);

            return rootItem;
        }


        if (!_rootBone)
            return null;

        items = new List<TreeViewItem>();
        int boneDepth = -1;
        //TreeViewItem rootItem = new TreeViewItem { id = _rootBone.GetInstanceID(), depth = boneDepth, displayName = _rootBone.name };
        rootItem = new BoneTreeViewItem { id = _rootBone.GetInstanceID(), depth = boneDepth, displayName = _rootBone.name };

        //items.Add(rootItem);

        buildTree(_rootBone, boneDepth, ref items);


        // Utility method that initializes the TreeViewItem.children and .parent for all items.
        SetupParentsAndChildrenFromDepths(rootItem, items);


        // Save items for serialization.
        _savedItems = new List<BoneTreeViewItem>();
        foreach (TreeViewItem item in items)
        {
            _savedItems.Add(item as BoneTreeViewItem);
        }
        _rootItem = rootItem as BoneTreeViewItem;


        // Return root of the tree.
        return rootItem;
    }

    private void buildTree(Transform parent, int boneDepth, ref List<TreeViewItem> items)
    {
        ++boneDepth;

        foreach(Transform child in parent.transform)
        {
            TreeViewItem item = new BoneTreeViewItem { id = child.GetInstanceID(), depth = boneDepth, displayName = child.name };

            items.Add(item);

            buildTree(child, boneDepth, ref items);
        }
    }

    protected override void RowGUI(RowGUIArgs args)
    {
        //base.RowGUI(args);

        var item = args.item;

        for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
        {
            cellGUI(args.GetCellRect(i), item as BoneTreeViewItem, (BoneColumns)args.GetColumn(i), ref args);
        }
        
    }

    void cellGUI(Rect cellRect, BoneTreeViewItem item, BoneColumns column, ref RowGUIArgs args)
    {
        Texture2D[] _icons =
        {
            EditorGUIUtility.FindTexture ("tree_icon_branch"),
            EditorGUIUtility.FindTexture ("tree_icon_leaf"),
            EditorGUIUtility.FindTexture ("TreeEditor.Material")
        };

        // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
        CenterRectUsingSingleLineHeight(ref cellRect);

        switch (column)
        {
            case BoneColumns.BoneIcon:
                {
                    GUI.DrawTexture(cellRect, _icons[0], ScaleMode.ScaleToFit);
                }
                break;
            case BoneColumns.Name:
                {
                    // Do toggle
                    Rect toggleRect = cellRect;
                    toggleRect.x += GetContentIndent(item);
                    toggleRect.width = TOGGLE_WIDTH;
                    //if (toggleRect.xMax < cellRect.xMax)
                    //item.data._enabled = EditorGUI.Toggle(toggleRect, item.data._enabled); // hide when outside cell rect

                    // Default icon and label
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                }
                break;
            case BoneColumns.RMIcon:
                {
                    //GUI.DrawTexture(cellRect, _icons[2], ScaleMode.ScaleToFit);

                    //Rect toggleRect = cellRect;
                    //toggleRect.x += GetContentIndent(item);
                    //toggleRect.width = TOGGLE_WIDTH;
                    //toggleRect.width = 70.0f;

                    //toggleRect.xMin += 5.0f;

                    //if (toggleRect.xMax < cellRect.xMax)
                    //item.data._enabled = EditorGUI.Toggle(toggleRect, item.data._enabled); // hide when outside cell rect

                    Rect objRect = new Rect();

                    List<RMObj> objs = item.Objs;

                    //if (objs.Count <= 3 && item.displayName == "Bone")
                    //{
                    //    objs.Add(null);
                    //}

                    float centerOffset = objs.Count > 0 ? (cellRect.height * 0.5f * objs.Count) : 0.0f;

                    //objs[0] = EditorGUI.ObjectField(cellRect, GUIContent.none, objs[0], typeof(RMObj), true) as RMObj;
                    for (int i = 0; i < objs.Count; ++i)
                    {
                        objRect = cellRect;
                        //objRect.position = new Vector2(cellRect.x, cellRect.y * i);
                        objRect.y = (cellRect.yMin - centerOffset) + (objRect.height * i);
                        objs[i] = EditorGUI.ObjectField(objRect, GUIContent.none, objs[i], typeof(RMObj), true) as RMObj;
                    }

                    //item.CellHeight = cellRect.height * objs.Count;
                    item.CellHeight = objs.Count > 0 ? cellRect.height * objs.Count : cellRect.height;


                    Rect buttonRect = cellRect;
                    buttonRect.width = 80.0f;
                    //CenterRectUsingSingleLineHeight(ref buttonRect);
                    buttonRect.x = cellRect.center.x - (buttonRect.width * 0.5f);
                    buttonRect.y = objs.Count > 0 ? objRect.yMax : cellRect.yMin;
                    GUILayout.BeginArea(buttonRect);
                    if (GUILayout.Button("Add", GUILayout.MaxWidth(100.0f)))
                    {
                        objs.Add(null);
                    }
                    GUILayout.EndArea();

                    item.CellHeight += buttonRect.height;

                    // Default icon and label
                    //args.rowRect = cellRect;

                    //args.rowRect.height = cellRect.y * objs.Count;
                    //base.RowGUI(args);
                }
                break;
            case BoneColumns.LeafIcon:
                {
                    GUI.DrawTexture(cellRect, _icons[1], ScaleMode.ScaleToFit);
                }
                break;


            //case BoneColumns.Value1:
            //case BoneColumns.Value2:
            //case BoneColumns.Value3:
            //    {
            //        if (showControls)
            //        {
            //            cellRect.xMin += 5f; // When showing controls make some extra spacing

            //            if (column == BoneColumns.Value1)
            //                item.data.floatValue1 = EditorGUI.Slider(cellRect, GUIContent.none, item.data.floatValue1, 0f, 1f);
            //            if (column == BoneColumns.Value2)
            //                item.data.material = (Material)EditorGUI.ObjectField(cellRect, GUIContent.none, item.data.material, typeof(Material), false);
            //            if (column == BoneColumns.Value3)
            //                item.data.text = GUI.TextField(cellRect, item.data.text);
            //        }
            //        else
            //        {
            //            string value = "Missing";
            //            if (column == BoneColumns.Value1)
            //                value = item.data.floatValue1.ToString("f5");
            //            if (column == BoneColumns.Value2)
            //                value = item.data.floatValue2.ToString("f5");
            //            if (column == BoneColumns.Value3)
            //                value = item.data.floatValue3.ToString("f5");

            //            DefaultGUI.LabelRightAligned(cellRect, value, args.selected, args.focused);
            //        }
            //    }
            //    break;
        }
    }

    protected override void BeforeRowsGUI()
    {
        base.BeforeRowsGUI();
    }

    protected override void AfterRowsGUI()
    {
        base.AfterRowsGUI();

        RefreshCustomRowHeights();
    }

    protected override float GetCustomRowHeight(int row, TreeViewItem item)
    {
        int numObjs = (item as BoneTreeViewItem).Objs.Count;
        float cellHeight = (item as BoneTreeViewItem).CellHeight;


        if (numObjs > 0 && cellHeight > 0.0f)
        {
            //Debug.Log("DEBUG " + item.displayName + ", " + cellHeight);

            //float height = base.GetCustomRowHeight(row, item) + (numObjs * 15.0f);
            float height = (item as BoneTreeViewItem).CellHeight;
            return height;

            //Rect rowRect = GetRowRect(row);
            //rowRect.height *= numObjs;
        }




        return base.GetCustomRowHeight(row, item);
    }

    public static MultiColumnHeaderState createDefaultMultiColumnHeaderState(float treeViewWidth)
    {
        Texture2D[] _icons =
        {
            EditorGUIUtility.FindTexture ("tree_icon_branch"),
            EditorGUIUtility.FindTexture ("tree_icon_leaf"),
            EditorGUIUtility.FindTexture ("TreeEditor.Material")
        };

        var columns = new[]
        {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Bones", _icons[0], "All bones in a skeleton. "),
                    contextMenuText = "Bones",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 50,
                    minWidth = 30,
                    maxWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("RMObj", EditorGUIUtility.FindTexture("PreMatSphere"), "The ray march object to visually represent a bone."),
                    contextMenuText = "RMObj",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 150,
                    minWidth = 30,
                    maxWidth = 250,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Multiplier", "In sed porta ante. Nunc et nulla mi."),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 110,
                    minWidth = 60,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Material", "Maecenas congue non tortor eget vulputate."),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 95,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = true
                },
            };

        Assert.AreEqual(columns.Length, System.Enum.GetValues(typeof(BoneColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

        var state = new MultiColumnHeaderState(columns);
        return state;
    }
}




[System.Serializable]
public class BoneTreeViewItem : TreeViewItem
{
    float _cellHeight = 0.0f;
    public bool _enabled = false;

    [SerializeField]
    List<RMObj> _objs = new List<RMObj>();



    public float CellHeight
    {
        get
        {
            return _cellHeight;
        }
        set
        {
            _cellHeight = value;
        }
    }

    public List<RMObj> Objs
    {
        get
        {
            return _objs;
        }
        set
        {
            _objs = value;
        }
    }

    public void addRMObj(RMObj rmObj, int index)
    {
        _objs[index] = rmObj;
    }

    public void addRMObj(RMObj rmObj)
    {
        _objs.Add(rmObj);
    }

    public void removeRMObj(RMObj rmObj)
    {
        _objs.Remove(rmObj);

        //_objs[index] = rmObj;
    }

    public void removeRMObj(int index)
    {
        _objs.RemoveAt(index);
    }
}


#endif
