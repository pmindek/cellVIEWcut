using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using UnityEditorInternal;

[CustomEditor(typeof(AnimationManager))]
public class AnimationInspector : Editor {


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //AnimationManager aniManager = (AnimationManager)target;

        base.OnInspectorGUI();

        // Actually draw the list in the inspector
        reorderableList.DoLayoutList();
               
    }

    private ReorderableList reorderableList;
    private AnimationManager aniManager
    {
        get
        {
            return target as AnimationManager;
        }
    }


    private void OnEnable()
    {
        reorderableList = new ReorderableList(aniManager.list, typeof(ListItemExample), true, true, true, true);

        // This could be used aswell, but I only advise this your class inherrits from UnityEngine.Object or has a CustomPropertyDrawer
        // Since you'll find your item using: serializedObject.FindProperty("list").GetArrayElementAtIndex(index).objectReferenceValue
        // which is a UnityEngine.Object
        // reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("list"), true, true, true, true);

        // Add listeners to draw events
        reorderableList.drawHeaderCallback += DrawHeader;
        reorderableList.drawElementCallback += DrawElement;

        reorderableList.onAddCallback += AddItem;
        reorderableList.onRemoveCallback += RemoveItem;
    }

    private void OnDisable()
    {
        // Make sure we don't get memory leaks etc.
        reorderableList.drawHeaderCallback -= DrawHeader;
        reorderableList.drawElementCallback -= DrawElement;

        reorderableList.onAddCallback -= AddItem;
        reorderableList.onRemoveCallback -= RemoveItem;
    }

    /// <summary>
    /// Draws the header of the list
    /// </summary>
    /// <param name="rect"></param>
    private void DrawHeader(Rect rect)
    {
        GUI.Label(rect, "Molecule Animation Order");
    }

    /// <summary>
    /// Draws one element of the list (ListItemExample)
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="index"></param>
    /// <param name="active"></param>
    /// <param name="focused"></param>
    private void DrawElement(Rect rect, int index, bool active, bool focused)
    {
        ListItemExample item = aniManager.list[index];

        EditorGUI.BeginChangeCheck();
        item.Selected = EditorGUI.Toggle(new Rect(rect.x, rect.y, 18, rect.height), item.Selected);
        item.MoleculeName = EditorGUI.TextField(new Rect(rect.x + 18, rect.y, rect.width - 18, rect.height), item.MoleculeName);
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }

        // If you are using a custom PropertyDrawer, this is probably better
        // EditorGUI.PropertyField(rect, serializedObject.FindProperty("list").GetArrayElementAtIndex(index));
        // Although it is probably smart to cach the list as a private variable ;)
    }

    private void AddItem(ReorderableList list)
    {
        //aniManager.list.Add(new ListItemExample());

        EditorUtility.SetDirty(target);
    }

    private void RemoveItem(ReorderableList list)
    {
        //aniManager.list.RemoveAt(list.index);

        EditorUtility.SetDirty(target);
    }


}

