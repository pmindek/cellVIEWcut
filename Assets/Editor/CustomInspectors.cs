using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CutObject))]
public class CutObjectCustomEditor : Editor
{
    public Vector2 _scrollPos;
    public bool showFilters = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CutObject cutObject = (CutObject)target;
        if (GUILayout.Button("Show/Hide Protein Filters"))
        {
            showFilters = !showFilters;
            
        }

        if (showFilters)
        {
            EditorUtility.SetDirty(cutObject);

            GUIStyle style_1 = new GUIStyle();
            style_1.margin = new RectOffset(10, 10, 10, 10);

            // Begin scroll view
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, style_1, GUILayout.ExpandWidth(true));
            {
                GUILayout.Label("Protein Filters: "); // + cutObject.gameObject.name);

                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical();
                {
                    for (int i = 0; i < cutObject.ProteinCutFilters.Count; i++)
                    {
                        cutObject.ProteinCutFilters[i].State = EditorGUILayout.ToggleLeft(cutObject.ProteinCutFilters[i].Name, cutObject.ProteinCutFilters[i].State);
                        GUILayout.Space(3);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}

[CustomEditor(typeof(CopyCameraEffects))]
public class CopyCameraCustomEffects : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var copyCameraEffects = (CopyCameraEffects)target;
        if (GUILayout.Button("Copy Camera Effects"))
        {
            copyCameraEffects.CopyEffects();
        }

        //if (GUILayout.Button("Clear Scene Camera Effects"))
        //{
        //    copyCameraEffects.ClearSceneCameraEffects();
        //}
    }
}