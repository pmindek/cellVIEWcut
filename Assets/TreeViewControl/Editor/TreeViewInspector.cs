using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TreeViewControlEditor))]
public class TreeViewInspector : Editor
{
    /// <summary>
    /// Add a tree view control to the game object
    /// </summary>
    /// <param name="go"></param>
    public static TreeViewControlEditor AddTreeView(GameObject go)
    {
        if (null == go)
        {
            return null;
        }

        TreeViewControlEditor item = go.AddComponent<TreeViewControlEditor>();
        AssignDefaults(go);
        return item;
    }

    public static void ApplySkin(GameObject go)
    {
        if (null == go)
        {
            return;
        }

        TreeViewControlEditor[] items = go.GetComponents<TreeViewControlEditor>();
        if (null == items)
        {
            return;
        }

        foreach (TreeViewControlEditor item in items)
        {
            if (null == item)
            {
                continue;
            }

            if (null == item.m_skinHover)
            {
                Debug.LogError("TreeView is missing Hover Skin");
                continue;
            }

            if (null == item.m_skinSelected)
            {
                Debug.LogError("TreeView is missing Selected Skin");
                continue;
            }

            if (null == item.m_skinUnselected)
            {
                Debug.LogError("TreeView is missing Unselected Skin");
                continue;
            }

            // create new skin instance
            GUISkin skinHover = (GUISkin)Object.Instantiate(item.m_skinHover);
            GUISkin skinSelected = (GUISkin)Object.Instantiate(item.m_skinSelected);
            GUISkin skinUnselected = (GUISkin)Object.Instantiate(item.m_skinUnselected);

            // name the skins
            skinHover.name = "Hover";
            skinSelected.name = "Selected";
            skinUnselected.name = "Unselected";

            item.m_skinHover = skinHover;
            item.m_skinSelected = skinSelected;
            item.m_skinUnselected = skinUnselected;
        }
    }

    public static void AssignDefaults(GameObject go)
    {
        if (null == go)
        {
            return;
        }

        TreeViewControlEditor[] items = go.GetComponents<TreeViewControlEditor>();
        if (null == items)
        {
            return;
        }

        // create new skin instance
        GUISkin skinHover = ScriptableObject.CreateInstance<GUISkin>();
        GUISkin skinSelected = ScriptableObject.CreateInstance<GUISkin>();
        GUISkin skinUnselected = ScriptableObject.CreateInstance<GUISkin>();

        // name the skins
        skinHover.name = "Hover";
        skinSelected.name = "Selected";
        skinUnselected.name = "Unselected";
		
		foreach (TreeViewControlEditor item in items)
		{
			if (null == item)
			{
				continue;
			}

	        item.m_textureBlank = GetTexture("Assets/TreeViewControl/blank.png");
			item.m_textureGuide = GetTexture("Assets/TreeViewControl/guide.png");
	        item.m_textureLastSiblingCollapsed = GetTexture("Assets/TreeViewControl/last_sibling_collapsed.png");
	        item.m_textureLastSiblingExpanded = GetTexture("Assets/TreeViewControl/last_sibling_expanded.png");
	        item.m_textureLastSiblingNoChild = GetTexture("Assets/TreeViewControl/last_sibling_nochild.png");
	        item.m_textureMiddleSiblingCollapsed = GetTexture("Assets/TreeViewControl/middle_sibling_collapsed.png");
	        item.m_textureMiddleSiblingExpanded = GetTexture("Assets/TreeViewControl/middle_sibling_expanded.png");
	        item.m_textureMiddleSiblingNoChild = GetTexture("Assets/TreeViewControl/middle_sibling_nochild.png");
			item.m_textureNormalChecked = GetTexture("Assets/TreeViewControl/normal_checked.png");
			item.m_textureNormalUnchecked = GetTexture("Assets/TreeViewControl/normal_unchecked.png");
			item.m_textureSelectedBackground = GetTexture("Assets/TreeViewControl/selected_background_color.png");
            item.m_skinHover = skinHover;
            item.m_skinSelected = skinSelected;
            item.m_skinUnselected = skinUnselected;
	
	        SetBackground(item.m_skinHover.button, null);
	        SetBackground(item.m_skinHover.toggle, null);
	        SetButtonFontSize(item.m_skinHover.button);
	        SetButtonFontSize(item.m_skinHover.toggle);
	        RemoveMargins(item.m_skinHover.button);
	        RemoveMargins(item.m_skinHover.toggle);
	        SetTextColor(item.m_skinHover.button, Color.yellow);
	        SetTextColor(item.m_skinHover.toggle, Color.yellow);
	
	        SetBackground(item.m_skinSelected.button, item.m_textureSelectedBackground);
	        SetBackground(item.m_skinSelected.toggle, item.m_textureSelectedBackground);
	        SetButtonFontSize(item.m_skinSelected.button);
	        SetButtonFontSize(item.m_skinSelected.toggle);
	        RemoveMargins(item.m_skinSelected.button);
	        RemoveMargins(item.m_skinSelected.toggle);
	        SetTextColor(item.m_skinSelected.button, Color.yellow);
	        SetTextColor(item.m_skinSelected.toggle, Color.yellow);
	
	        SetBackground(item.m_skinUnselected.button, null);
	        SetBackground(item.m_skinUnselected.toggle, null);
	        SetButtonFontSize(item.m_skinUnselected.button);
	        SetButtonFontSize(item.m_skinUnselected.toggle);
	        RemoveMargins(item.m_skinUnselected.button);
	        RemoveMargins(item.m_skinUnselected.toggle);
	        SetTextColor(item.m_skinUnselected.button, Color.white);
	        SetTextColor(item.m_skinUnselected.toggle, Color.white);
    	}
    }

	static void SetBackground(GUIStyle style, Texture2D texture)
	{
		style.active.background = texture;
		style.focused.background = texture;
		style.hover.background = texture;
		style.normal.background = texture;
		style.onActive.background = texture;
		style.onFocused.background = texture;
		style.onHover.background = texture;
		style.onNormal.background = texture;
	}
	
	static void SetTextColor(GUIStyle style, Color color)
	{
		style.active.textColor = color;
		style.focused.textColor = color;
		style.hover.textColor = color;
		style.normal.textColor = color;
		style.onActive.textColor = color;
		style.onFocused.textColor = color;
		style.onHover.textColor = color;
		style.onNormal.textColor = color;
	}
	
	static void RemoveMargins(GUIStyle style)
	{
		style.margin.bottom = 0;
		style.margin.left = 0;
		style.margin.right = 0;
		style.margin.top = 0;
	}

    static void SetButtonFontSize(GUIStyle style)
    {
        style.fontSize = 12;
    }

    static Texture2D GetTexture(string texturePath)
    {
        try
        {
            Texture2D item = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
            return item;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("Failed to find local texture: {0}", ex));
            return null;
        }
    }

    static GUISkin GetGUISkin(string skinPath)
    {
        try
        {
            GUISkin item = (GUISkin)AssetDatabase.LoadAssetAtPath(skinPath, typeof(GUISkin));
            return item;
        }
        catch (System.Exception ex)
        {
            Debug.LogError(string.Format("Failed to find local GUI skin: {0}", ex));
            return null;
        }
    }

    [MenuItem("TreeView/Assign Defaults", validate = true)]
    public static bool CheckAssignDefaults()
    {
        GameObject go = Selection.activeGameObject;
        if (null == go ||
            !(go is GameObject))
        {
            return false;
        }

        TreeViewControlEditor item = go.GetComponent<TreeViewControlEditor>();
        if (null == item)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    [MenuItem("TreeView/Apply Skin", validate = true)]
    public static bool CheckApplySkin()
    {
        GameObject go = Selection.activeGameObject;
        if (null == go ||
            !(go is GameObject))
        {
            return false;
        }

        TreeViewControlEditor item = go.GetComponent<TreeViewControlEditor>();
        if (null == item)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    Vector2 m_mousePos = Vector2.zero;

    public void OnSceneGUI()
    {
        bool needsRepainted = false;
        if (null != Event.current &&
            m_mousePos != Event.current.mousePosition)
        {
            needsRepainted = true;
        }

        if (null == target &&
            !(target is TreeViewControlEditor))
        {
            Debug.LogError("Not a TreeViewControl");
            return;
        }

        TreeViewControlEditor item = (TreeViewControlEditor)target;
        if (null == item)
        {
            Debug.LogError("TreeViewControl is null");
            return;
        }

        if (!item.DisplayOnScene)
        {
            return;
        }

        item.DisplayTreeView(TreeViewControlEditor.DisplayTypes.USE_SCROLL_AREA);

        if (item.SelectedItemEditor != _mLastSelectedItemEditor)
        {
            _mLastSelectedItemEditor = item.SelectedItemEditor;
            needsRepainted = true;
        }

        if (needsRepainted)
        {
            Repaint();
            SceneView.RepaintAll();
        }
    }

    TreeViewItemEditor _mLastSelectedItemEditor = null;

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.Label("Please avoid editing these fields while playing");
        }

        if (null == target &&
            !(target is TreeViewControlEditor))
        {
            Debug.LogError("Not a TreeViewControl");
            return;
        }

        TreeViewControlEditor item = (TreeViewControlEditor)target;
        if (null == item)
        {
            Debug.LogError("TreeViewControl is null");
            return;
        }

        bool needsRepainted = false;

        if (null != Event.current &&
            m_mousePos != Event.current.mousePosition)
        {
            needsRepainted = true;
        }
        
        if (item.SelectedItemEditor != _mLastSelectedItemEditor)
        {
            _mLastSelectedItemEditor = item.SelectedItemEditor;
            needsRepainted = true;
        }

        if (null != item.SelectedItemEditor &&
            string.IsNullOrEmpty(item.SelectedItemEditor.Header))
        {
            item.SelectedItemEditor.Header = "Root item";
            needsRepainted = true;
        }

        if (GUILayout.Button("Select Root TreeViewItem"))
        {
            item.SelectedItemEditor = item.RootItemEditor;
            Selection.activeGameObject = item.gameObject;
            needsRepainted = true;
        }

        if (null != item.SelectedItemEditor &&
            GUILayout.Button("Add Child TreeView Item"))
        {
            item.SelectedItemEditor.AddItem("Default text");
            needsRepainted = true;
        }

        if (null != item.SelectedItemEditor &&
            item.SelectedItemEditor != item.RootItemEditor &&
            null != item.SelectedItemEditor.Parent &&
            GUILayout.Button("Remove TreeView Item"))
        {
            TreeViewItemEditor oldItemEditor = item.SelectedItemEditor;
            item.SelectedItemEditor.Parent.Items.Remove(oldItemEditor);
            item.SelectedItemEditor = null;
            needsRepainted = true;
        }

        if (null != item.SelectedItemEditor &&
            null != item.SelectedItemEditor.Parent &&
            item.SelectedItemEditor != item.RootItemEditor &&
            item.SelectedItemEditor.Parent.Items.IndexOf(item.SelectedItemEditor) > 0 &&
            GUILayout.Button("Move Up"))
        {
            TreeViewItemEditor oldItemEditor = item.SelectedItemEditor;
            item.SelectedItemEditor = null;
            int index = oldItemEditor.Parent.Items.IndexOf(oldItemEditor);
            oldItemEditor.Parent.Items.Remove(oldItemEditor);
            oldItemEditor.Parent.Items.Insert(index - 1, oldItemEditor);
            item.SelectedItemEditor = oldItemEditor;
            needsRepainted = true;
        }

        if (null != item.SelectedItemEditor &&
            null != item.SelectedItemEditor.Parent &&
            item.SelectedItemEditor != item.RootItemEditor &&
            (item.SelectedItemEditor.Parent.Items.IndexOf(item.SelectedItemEditor) + 1) < item.SelectedItemEditor.Parent.Items.Count &&
            GUILayout.Button("Move Down"))
        {
            TreeViewItemEditor oldItemEditor = item.SelectedItemEditor;
            item.SelectedItemEditor = null;
            int index = oldItemEditor.Parent.Items.IndexOf(oldItemEditor);
            oldItemEditor.Parent.Items.Remove(oldItemEditor);
            oldItemEditor.Parent.Items.Insert(index + 1, oldItemEditor);
            item.SelectedItemEditor = oldItemEditor;
            needsRepainted = true;
        }

        if (null != item.SelectedItemEditor &&
            null != item.SelectedItemEditor.Parent &&
            null != item.SelectedItemEditor.Parent.Parent &&
            item.SelectedItemEditor != item.RootItemEditor &&
            item.SelectedItemEditor.Parent != item.RootItemEditor &&
            GUILayout.Button("Promote TreeView Item"))
        {
            TreeViewItemEditor oldItemEditor = item.SelectedItemEditor;
            item.SelectedItemEditor = null;
            oldItemEditor.Parent.Items.Remove(oldItemEditor);
            oldItemEditor.Parent.Parent.Items.Insert(0, oldItemEditor);
            oldItemEditor.Parent = oldItemEditor.Parent.Parent;
            item.SelectedItemEditor = oldItemEditor;
            needsRepainted = true;
        }

        EditorGUILayout.Separator();

        if (null != item.SelectedItemEditor)
        {
            EditorGUILayout.LabelField("Parent:", (item.SelectedItemEditor.Parent == null) ? "(null)" : "(valid)");
            EditorGUILayout.LabelField("Parent Control:", (item.SelectedItemEditor.ParentControlEditor == null) ? "(null)" : "(valid)");
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("***** Selected Item:", "(editable fields) *******");
        if (null != item.SelectedItemEditor)
        {
            if (!string.IsNullOrEmpty(item.SelectedItemEditor.Header))
            {
                string header = EditorGUILayout.TextField("Header:", item.SelectedItemEditor.Header);
                if (!item.SelectedItemEditor.Header.Equals(header))
                {
                    item.SelectedItemEditor.Header = header;
                    needsRepainted = true;
                }
            }
            bool isExpanded = EditorGUILayout.Toggle("IsExpanded:", item.SelectedItemEditor.IsExpanded);
            if (isExpanded != item.SelectedItemEditor.IsExpanded)
            {
                item.SelectedItemEditor.IsExpanded = isExpanded;
                needsRepainted = true;
            }
			bool isCheckBox = EditorGUILayout.Toggle("IsCheckBox:", item.SelectedItemEditor.IsCheckBox);
            if (isCheckBox != item.SelectedItemEditor.IsCheckBox)
            {
                item.SelectedItemEditor.IsCheckBox = isCheckBox;
                needsRepainted = true;
            }
			bool isChecked = EditorGUILayout.Toggle("IsChecked:", item.SelectedItemEditor.IsChecked);
            if (isChecked != item.SelectedItemEditor.IsChecked)
            {
                item.SelectedItemEditor.IsChecked = isChecked;
                needsRepainted = true;
            }
        }

        if (needsRepainted)
        {
            Repaint();
            SceneView.RepaintAll();
        }

		base.OnInspectorGUI();

        if (item.DisplayInInspector)
		{
			EditorGUILayout.Separator();
            item.DisplayTreeView(TreeViewControlEditor.DisplayTypes.NONE);
			EditorGUILayout.Separator();
		}
    }
}
