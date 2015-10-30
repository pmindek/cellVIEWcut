using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CutObject))]
public class CutObjectCustomEditor : Editor
{
    public Vector2 _scrollPos;

    [SerializeField]
    public bool showFilters = false;

    //private bool[] proteinFoldout = new bool[100];
    
    
    TreeViewItem m_lastSelectedItem = null;

    Vector2 m_mousePos = Vector2.zero;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CutObject cutObject = (CutObject)target;


        TreeViewControl item = cutObject.GetComponent<TreeViewControl>();
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

        if (item.SelectedItem != m_lastSelectedItem)
        {
            m_lastSelectedItem = item.SelectedItem;
            needsRepainted = true;
        }

        if (null != item.SelectedItem &&
            string.IsNullOrEmpty(item.SelectedItem.Header))
        {
            item.SelectedItem.Header = "Root item";
            needsRepainted = true;
        }
        
        showFilters = EditorGUILayout.Foldout(showFilters, "Protein Filters");
        if (showFilters)
        {
            EditorUtility.SetDirty(cutObject);

            EditorGUILayout.Separator();
            RecipeTreeUI ui = cutObject.GetComponent<RecipeTreeUI>();

            /*Debug.Log("GR" + ui.currentSelectedIngredient);
            Debug.Log("GL");
            for (int i = 0; i < ui.selectedIngredients.Count; i++)
            {
                Debug.Log(" .~" + ui.selectedIngredients[i]);
            }*/

            //Debug.Log(Random.Range(1, 999));
            //Debug.Log(ui.selectedIngredients.Count);

            if (ui.currentSelectedIngredient != -1)
            {
                var name = SceneManager.Instance.ProteinNames[ui.currentSelectedIngredient];

                var rangeValues = cutObject.GetRangeValues(ui.currentSelectedIngredient);

                GUILayout.Label("Visibility: " + name + " ~ " + ui.currentSelectedIngredient);
                MultiRangeSlider.HandleCascadeSliderGUI(ref rangeValues);
                EditorGUILayout.Separator();

                cutObject.SetRangeValues(ui.currentSelectedIngredient, rangeValues);

                /*rangeValues[0] = (float)Random.Range(0, 999) / 1000.0f;
                rangeValues[1] = 0.15f;*/

                float st_cutaway = (float) SceneManager.Instance.stats[0];
                float st_all = (float) SceneManager.Instance.stats[1];
                float st_occluding = (float) SceneManager.Instance.stats[2];

                rangeValues[0] = st_occluding / st_all;
                rangeValues[1] = (1.0f - st_cutaway / st_all) - rangeValues[0];

                //Debug.Log(st_visible + " / " + st_all);
                //Debug.Log("ú " + rangeValues);

            }
            else
            {
                GUILayout.Label("Nevym.");
            }

            // Begin scroll view
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));
            {
                if (needsRepainted)
                {
                    Repaint();
                    SceneView.RepaintAll();
                }
                
                item.DisplayTreeView(TreeViewControl.DisplayTypes.NONE);
            }
            EditorGUILayout.EndScrollView();

            
        }

        //showFilters = EditorGUILayout.Foldout(showFilters, "Protein Filters");
        //if(showFilters)
        //{
        //    EditorUtility.SetDirty(cutObject);

        //    GUIStyle style_1 = new GUIStyle();
        //    style_1.margin = new RectOffset(10, 0, 0, 0);
        //    ////style_1.padding = new RectOffset(50, 0, 0, 0);

        //    //GUIStyle style_2 = EditorStyles.foldout;
        //    //style_1.margin = new RectOffset(50, 0, 0, 0);
        //    //style_2.padding = new RectOffset(50, 0, 0, 0);

        //    // Begin scroll view
        //    _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));
        //    {
        //        //GUILayout.Label(""); // + cutObject.gameObject.name);

        //        //EditorGUILayout.Space();

        //        EditorGUILayout.BeginVertical();
        //        {
        //            for (int i = 0; i < cutObject.ProteinCutFilters.Count; i++)
        //            {
        //                cutObject.ProteinCutFilters[i].State = EditorGUILayout.ToggleLeft(cutObject.ProteinCutFilters[i].Name, cutObject.ProteinCutFilters[i].State);

        //                EditorGUILayout.BeginVertical(style_1);
        //                {
        //                    proteinFoldout[i] = EditorGUILayout.Foldout(proteinFoldout[i], "Protein filter params");
        //                    if (proteinFoldout[i])
        //                    {
        //                        MultiRangeSlider.HandleCascadeSliderGUI(ref rangeValues);
        //                    }
        //                }
        //                EditorGUILayout.EndVertical();


        //                GUILayout.Space(3);
        //            }
        //        }
        //        EditorGUILayout.EndVertical();
        //    }
        //    EditorGUILayout.EndScrollView();
        //}
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